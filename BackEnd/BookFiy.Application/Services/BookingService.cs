using BookFiy.Application.Comman;
using BookFiy.Application.Dtos.Booking;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Domain.IRepositories;
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
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(
            ITenantProvider tenantProvider,
            IBookingRepository bookingRepository,
            IBookingStatusRepository bookingStatusRepository,
            IServiceRepository serviceRepository,
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _tenantProvider = tenantProvider;
            _bookingRepository = bookingRepository;
            _bookingStatusRepository = bookingStatusRepository;
            _serviceRepository = serviceRepository;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> UpdateBookingAsync(Guid bookingId, UpdateBookingDto dto)
        {

            await _unitOfWork.BeginTransactionAsync();
            try
            {

                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                    return Result<bool>.Failure("Booking not found", ErrorType.NotFound);

                if (booking.TenantId != _tenantProvider.TenantId)
                     return Result<bool>.Failure("Tenant mismatch", ErrorType.Forbidden);

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
                    if (service == null)
                        return Result<bool>.Failure("Service not found", ErrorType.NotFound);
                    var newStart = dto.StartTime.Value;
                    var newEnd = newStart.AddMinutes(service.DurationMinutes);

                    var conflict = await _bookingRepository.HasConflictAsync(booking.TenantId, booking.EmployeeId, newStart, newEnd, booking.Id);
                    if (conflict) 
                        return Result<bool>.Failure("Time slot already booked", ErrorType.Conflict);

                    booking.StartTime = newStart;
                    booking.EndTime = newEnd;
                }

                if (dto.StatusId.HasValue)
                    booking.StatusId = dto.StatusId.Value;

                if (dto.Notes != null)
                    booking.Notes = dto.Notes;



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
                await _bookingRepository.AddAuditAsync(audit);
                await _unitOfWork.CommitAsync();
                return Result<bool>.Success(true, "booking updated successfully");
                
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Result<bool>.Failure(ex.Message, ErrorType.ServerError);
                
            }
        }

        public async Task<Result<bool>> CreateBookingAsync(Guid TenantId, CreateBookingDto bookingDto)
        {

            await _unitOfWork.BeginTransactionAsync();

            try
            {


                if (TenantId != _tenantProvider.TenantId)
                    return Result<bool>.Failure("Tenant mismatch", ErrorType.Forbidden);

                var service = await _serviceRepository.GetByIdAsync(bookingDto.ServiceId, TenantId);
                if (service == null)
                    return Result<bool>.Failure("Invalid service ID.", ErrorType.NotFound);

                var user = await _userManager.FindByIdAsync(bookingDto.UserId.ToString());
                if (user == null)
                    return Result<bool>.Failure("Invalid user ID.", ErrorType.NotFound);

                var startTime = bookingDto.StartTime;
                var endTime = startTime.AddMinutes(service.DurationMinutes);

                var lockKey = $"booking:{TenantId}:{service.Id}:{startTime:yyyyMMddHHmm}";

                var bookingStatus = await _bookingStatusRepository.GetByNameAsync("Pending");

                if (bookingStatus == null)
                    return Result<bool>.Failure("Booking status 'Pending' not found.", ErrorType.NotFound);

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

                await _bookingRepository.CreateBookingWithLockAndAuditAsync(booking, audit, lockKey);
                await _unitOfWork.CommitAsync();
                return Result<bool>.Success(true, "Booking created successfully");

            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Result<bool>.Failure(ex.Message, ErrorType.ServerError);

            }
        }
        public async Task<Result<bool>> DeleteBookingAsync(Guid bookingId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {

                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                    return Result<bool>.Failure("Booking not found", ErrorType.NotFound);

                var cancelledStatus = await _bookingStatusRepository.GetByNameAsync("Cancelled");
                if (cancelledStatus == null)
                    return Result<bool>.Failure("Booking status 'Cancelled' not found.", ErrorType.NotFound);

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
                await _bookingRepository.AddAuditAsync(audit);
                await _unitOfWork.CommitAsync();
                return Result<bool>.Success(true, "Booking deleted successfully");
            }
            catch (Exception ex)
            {
                
                    await _unitOfWork.RollbackAsync();
                    return Result<bool>.Failure(ex.Message,ErrorType.ServerError);
                
            }
        }

        public async Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId)
        {
            var booking =await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return Result<BookingDto>.Failure("Booking not found",ErrorType.NotFound);

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

            return  Result<BookingDto>.Success(bookingDto); 
        }

        public async Task<Result<List<BookingDto>>> GetBookingsByDateAsync(DateOnly date)
        {
            var bookings = await _bookingRepository.GetByDateAsync(_tenantProvider.TenantId, date);
            var bookingDtos = bookings.Select(booking => new BookingDto
            {
                Id = booking.Id,
                TenantId = booking.TenantId,
                UserName = booking.User?.UserName ?? "Unknown",
                UserEmail = booking.User?.Email ?? "Unknown",
                UserPhone = booking.User?.PhoneNumber ?? "Unknown",
                BookingDate = booking.StartTime,
                StatusId = booking.StatusId,
                StatusName = booking.Status.Name
            }).ToList();


            return Result<List<BookingDto>>.Success(bookingDtos);

        }

        public  Task<Result<List<BookingDto>>> GetBookingsByEmployeeAsync(Guid employeeId)
        {
            return  GetBookingsByEmployeeAsync(employeeId, null, null, 1, 50, "asc");
        }

        public Task<Result<List<BookingDto>>> GetBookingsByUserAsync(Guid employeeId)
        {
            return GetBookingsByEmployeeAsync(employeeId, null, null, 1, 50, "asc");
        }

        public async Task<Result<List<BookingDto>>> GetBookingsByEmployeeAsync(Guid employeeId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
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
            return Result<List<BookingDto>>.Success(results);
        }

        public async Task<Result<List<BookingDto>>> GetBookingsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
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

            return Result<List<BookingDto>>.Success(bookingDtos);
        }
    }
}
