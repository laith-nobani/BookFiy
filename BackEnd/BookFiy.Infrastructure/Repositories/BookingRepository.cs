using BookFiy.Domain.Entites;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _dbContext;

        public BookingRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Booking booking)
        {

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

        }

        public async Task DeleteAsync(Booking booking)
        {
            var existingBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            if (existingBooking != null)
            {
                _dbContext.Bookings.Remove(existingBooking);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task<bool> ExistsAsync(Guid id)
        {
            var existingBooking = await _dbContext.Bookings.FindAsync(id);
            return existingBooking != null;
        }

        public async Task<List<Booking>> GetAllAsync(Guid tenantId)
        {
             var bookings = await _dbContext.Bookings
                .Where(b => b.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();
            return bookings;
        }

        public Task<List<Booking>> GetByDateAsync(Guid tenantId, DateOnly date)
        {
             var bookings = _dbContext.Bookings
                .Where(b => b.TenantId == tenantId && b.StartTime.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                .ToListAsync();
            return bookings;
        }

        public async Task<List<Booking>> GetByEmployeeAsync(Guid tenantId, Guid employeeId)
        {
             var bookings = await _dbContext.Bookings
                .Where(b => b.TenantId == tenantId && b.EmployeeId == employeeId)
                .ToListAsync();
            return bookings;
        }

        public async Task<List<Booking>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, DateTime? from, DateTime? to, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var query = _dbContext.Bookings.AsNoTracking().Where(b => b.TenantId == tenantId && b.EmployeeId == employeeId);
            if (from.HasValue)
                query = query.Where(b => b.StartTime >= from.Value);
            if (to.HasValue)
                query = query.Where(b => b.EndTime <= to.Value);

            query = sort.ToLower() == "desc" ? query.OrderByDescending(b => b.StartTime) : query.OrderBy(b => b.StartTime);

            var skip = (page - 1) * pageSize;
            return await query.Skip(skip).Take(pageSize).Include(b => b.Status).ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _dbContext
                .Bookings
                .Include(b => b.User)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Booking>> GetByUserAsync(Guid tenantId, Guid userId, DateTime? from, DateTime? to, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var query = _dbContext.Bookings.AsNoTracking().Where(b => b.TenantId == tenantId && b.UserId == userId);
            if (from.HasValue)
                query = query.Where(b => b.StartTime >= from.Value);
            if (to.HasValue)
                query = query.Where(b => b.EndTime <= to.Value);

            query = sort.ToLower() == "desc" ? query.OrderByDescending(b => b.StartTime) : query.OrderBy(b => b.StartTime);

            var skip = (page - 1) * pageSize;
            return await query.Skip(skip).Take(pageSize).Include(b => b.Status).ToListAsync();
        }

        public async Task<bool> HasConflictAsync(Guid tenantId, Guid serviceId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            var query = _dbContext.Bookings.AsQueryable()
                .Where(b => b.TenantId == tenantId && b.ServiceId == serviceId);

            if (excludeBookingId.HasValue)
                query = query.Where(b => b.Id != excludeBookingId.Value);

            var conflictExists = await query.AnyAsync(b => (b.StartTime < endTime) && (b.EndTime > startTime));
            return conflictExists;

        }

        public async Task UpdateAsync()
        {
            
          await _dbContext.SaveChangesAsync();
            
        }
    }
}
