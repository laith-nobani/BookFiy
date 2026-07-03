using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.IRepositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);

        Task<List<Booking>> GetAllAsync(Guid tenantId);

        Task<List<Booking>> GetByEmployeeAsync(Guid tenantId, Guid employeeId);
        Task<List<Booking>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, DateTime? from, DateTime? to, int page = 1, int pageSize = 20, string sort = "asc");

        Task<List<Booking>> GetByUserAsync(Guid tenantId, Guid userId, DateTime? from, DateTime? to, int page = 1, int pageSize = 20, string sort = "asc");

        Task<List<Booking>> GetByDateAsync(Guid tenantId, DateOnly date);

        Task AddAsync(Booking booking);

        Task UpdateAsync();

        Task DeleteAsync(Booking booking);

        Task<bool> ExistsAsync(Guid id);

        Task<bool> HasConflictAsync(
            Guid tenantId,
            Guid employeeId,
            DateTime startTime,
            DateTime endTime,
            Guid? excludeBookingId = null);

    }
}
