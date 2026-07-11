using BookFiy.Application.Dtos.Booking;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingStatusRepository _bookingStatusRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly RedisService _redisService;

        public BookingService(
            ITenantProvider tenantProvider,
            IBookingRepository bookingRepository,
            IBookingStatusRepository bookingStatusRepository,
            IServiceRepository serviceRepository,
            IEmployeeRepository employeeRepository,
            AppDbContext dbContext,
            RedisService redisService,
            UserManager<ApplicationUser> userManager)
        {
            _tenantProvider = tenantProvider;
            _bookingRepository = bookingRepository;
            _bookingStatusRepository = bookingStatusRepository;
            _serviceRepository = serviceRepository;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _dbContext=dbContext;
            _redisService = redisService;
        }

        public async Task UpdateBookingAsync(Guid bookingId, UpdateBookingDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) throw new KeyNotFoundException("Booking not found");

            if (booking.TenantId != _tenantProvider.TenantId)
                throw new UnauthorizedAccessException("Tenant mismatch");

            var before = new
            {
                booking.StartTime,
                booking.EndTime,
                booking.StatusId,
                booking.Notes
            };

            if (dto.StartTime.HasValue)
            {
                var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, booking.TenantId);
                if (service == null) throw new InvalidOperationException("Service not found");
                var newStart = dto.StartTime.Value;
                var newEnd = newStart.AddMinutes(service.DurationMinutes);

                var conflict = await _bookingRepository.HasConflictAsync(booking.TenantId, booking.EmployeeId, newStart, newEnd, booking.Id);
                if (conflict) throw new InvalidOperationException("Time slot already booked");

                booking.StartTime = newStart;
                booking.EndTime = newEnd;
            }

            if (dto.StatusId.HasValue)
                booking.StatusId = dto.StatusId.Value;

            if (dto.Notes != null)
                booking.Notes = dto.Notes;

            await _bookingRepository.UpdateAsync();

            var after = new
            {
                booking.StartTime,
                booking.EndTime,
                booking.StatusId,
                booking.Notes
            };

            var audit = new BookingAudit
            {
                BookingId = booking.Id,
                TenantId = booking.TenantId,
                EventType = "Updated",
                Data = System.Text.Json.JsonSerializer.Serialize(new { before, after }),
                CreatedBy = null,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.Set<BookingAudit>().AddAsync(audit);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CreateBookingAsync(Guid TenantId, CreateBookingDto bookingDto)
        {
            if (TenantId != _tenantProvider.TenantId)
                throw new UnauthorizedAccessException("Tenant mismatch");

            var service = await _serviceRepository.GetByIdAsync(bookingDto.ServiceId, TenantId);
            if (service == null)
                throw new ArgumentException("Invalid service ID.");

            var user = await _userManager.FindByIdAsync(bookingDto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("Invalid user ID.");

            var startTime = bookingDto.StartTime;
            var endTime = startTime.AddMinutes(service.DurationMinutes);

            var lockKey = $"booking:{TenantId}:{service.Id}:{startTime:yyyyMMddHHmm}";

            await using var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                await using (var lockCommand = connection.CreateCommand())
                {
                    lockCommand.CommandText = @"
                    EXEC sp_getapplock 
                    @Resource = @resource,
                    @LockMode = 'Exclusive',
                    @LockTimeout = 5000;
                    ";

                    var param = lockCommand.CreateParameter();
                    param.ParameterName = "@resource";
                    param.Value = lockKey;
                    lockCommand.Parameters.Add(param);

                    await lockCommand.ExecuteNonQueryAsync();
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var hasConflict = await _bookingRepository.HasConflictAsync(
                        TenantId,
                        service.Id,
                        startTime,
                        endTime);

                    if (hasConflict)
                        throw new Exception("Time slot already booked");

                    var bookingStatus = await _bookingStatusRepository.GetByNameAsync("Pending");

                    if (bookingStatus == null)
                        throw new Exception("Booking status 'Pending' not found.");

                    var booking = new Booking
                    {
                        Id = Guid.NewGuid(),
                        TenantId = TenantId,
                        ServiceId = service.Id,
                        EmployeeId = service.Employee.Id,
                        UserId = user.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        StatusId = bookingStatus.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _bookingRepository.AddAsync(booking);
                    await _dbContext.SaveChangesAsync();

                    var auditData = new
                    {
                        booking.Id,
                        booking.TenantId,
                        booking.ServiceId,
                        booking.EmployeeId,
                        booking.UserId,
                        booking.StartTime,
                        booking.EndTime,
                        booking.StatusId,
                        booking.CreatedAt
                    };

                    var audit = new BookingAudit
                    {
                        BookingId = booking.Id,
                        TenantId = booking.TenantId,
                        EventType = "Created",
                        Data = System.Text.Json.JsonSerializer.Serialize(auditData),
                        CreatedBy = booking.UserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _dbContext.Set<BookingAudit>().AddAsync(audit);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                try
                {
                    await using var releaseCommand = connection.CreateCommand();
                    releaseCommand.CommandText = @"
                    EXEC sp_releaseapplock 
                    @Resource = @resource,
                    @LockOwner = 'Session';
                ";

                    var param = releaseCommand.CreateParameter();
                    param.ParameterName = "@resource";
                    param.Value = lockKey;
                    releaseCommand.Parameters.Add(param);

                    await releaseCommand.ExecuteNonQueryAsync();
                }
                catch
                {
                   
                }

                await connection.CloseAsync();
            }
        }
        public async Task DeleteBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            var cancelledStatus = await _bookingStatusRepository.GetByNameAsync("Cancelled");
            if (cancelledStatus == null)
                throw new Exception("Booking status 'Cancelled' not found.");

            booking.StatusId = cancelledStatus.Id;
            await _bookingRepository.UpdateAsync();

            var audit = new BookingAudit
            {
                BookingId = booking.Id,
                TenantId = booking.TenantId,
                EventType = "Cancelled",
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    booking.Id,
                    booking.StatusId,
                    CancelledAt = DateTime.UtcNow
                }),
                CreatedBy = null,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.Set<BookingAudit>().AddAsync(audit);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking =await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            var user= await _userManager.FindByIdAsync(booking.UserId.ToString());
            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                TenantId = booking.TenantId,
                UserName = booking?.User?.UserName ?? "Unknown",
                UserEmail = booking?.User?.Email ?? "Unknown",
                UserPhone = booking?.User?.PhoneNumber ?? "Unknown",
                BookingDate = booking.StartTime,
                StatusId = booking.StatusId,
                StatusName = booking.Status.Name
            };

            return bookingDto;
        }

        public async Task<List<BookingDto>> GetBookingsByDateAsync(DateOnly date)
        {
            var bookings = await _bookingRepository.GetByDateAsync(_tenantProvider.TenantId, date);
            var bookingDtos = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var user = await _userManager.FindByIdAsync(booking.UserId.ToString());
                bookingDtos.Add(new BookingDto
                {
                    Id = booking.Id,
                    TenantId = booking.TenantId,
                    UserName = user?.UserName ?? "Unknown",
                    UserEmail = user?.Email ?? "Unknown",
                    UserPhone = user?.PhoneNumber ?? "Unknown",
                    BookingDate = booking.StartTime,
                    StatusId = booking.StatusId,
                    StatusName = booking.Status.Name
                });
            }
            return bookingDtos;

        }

        public Task<List<BookingDto>> GetBookingsByEmployeeAsync(Guid employeeId)
        {
            return GetBookingsByEmployeeAsync(employeeId, null, null, 1, 50, "asc");
        }

        public Task<List<BookingDto>> GetBookingsByUserAsync(Guid employeeId)
        {
            return GetBookingsByEmployeeAsync(employeeId, null, null, 1, 50, "asc");
        }

        public async Task<List<BookingDto>> GetBookingsByEmployeeAsync(Guid employeeId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var tenantId = _tenantProvider.TenantId;
            var bookings = await _bookingRepository.GetByEmployeeAsync(tenantId, employeeId, from, to, page, pageSize, sort);
            var results = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var user = await _userManager.FindByIdAsync(booking.UserId.ToString());
                results.Add(new BookingDto
                {
                    Id = booking.Id,
                    TenantId = booking.TenantId,
                    UserName = user?.UserName ?? "Unknown",
                    UserEmail = user?.Email ?? "",
                    UserPhone = user?.PhoneNumber ?? "",
                    BookingDate = booking.StartTime,
                    StatusId = booking.StatusId,
                    StatusName = booking.Status?.Name ?? string.Empty
                });
            }
            return results;
        }

        public async Task<List<BookingDto>> GetBookingsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var bookings =await _bookingRepository.GetByUserAsync(_tenantProvider.TenantId, userId,from,to,page,pageSize,sort);

            var bookingDtos = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var user = await _userManager.FindByIdAsync(booking.UserId.ToString());
                bookingDtos.Add(new BookingDto
                {
                    Id = booking.Id,
                    TenantId = booking.TenantId,
                    UserName = user?.UserName ?? "Unknown",
                    UserEmail = user?.Email ?? "Unknown",
                    UserPhone = user?.PhoneNumber ?? "Unknown",
                    BookingDate = booking.StartTime,
                    StatusId = booking.StatusId,
                    StatusName = booking.Status.Name
                });
            }

            return bookingDtos;
        }
    }
}
