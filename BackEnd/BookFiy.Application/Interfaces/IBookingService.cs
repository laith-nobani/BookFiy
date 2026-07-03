using BookFiy.Application.Dtos.Booking;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IBookingService
    {
        public Task<bool> CreateBookingAsync(Guid TenantId,CreateBookingDto bookingDto);
        public Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
        public Task<List<BookingDto>> GetBookingsByEmployeeAsync(Guid employeeId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc");
        public Task UpdateBookingAsync(Guid bookingId, BookFiy.Application.Dtos.Booking.UpdateBookingDto dto);
        public Task<List<BookingDto>> GetBookingsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc");
        public Task<List<BookingDto>> GetBookingsByDateAsync(DateOnly date);
        public Task DeleteBookingAsync(Guid bookingId);

    }
}
