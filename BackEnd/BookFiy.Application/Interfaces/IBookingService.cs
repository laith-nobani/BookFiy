using BookFiy.Application.Comman;
using BookFiy.Application.Dtos.Booking;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IBookingService
    {
        public Task<Result<bool>> CreateBookingAsync(Guid TenantId,CreateBookingDto bookingDto);
        public Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId);
        public Task<Result<List<BookingDto>>> GetBookingsByEmployeeAsync(Guid employeeId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc");
        public Task<Result<bool>> UpdateBookingAsync(Guid bookingId,UpdateBookingDto dto);
        public Task<Result<List<BookingDto>>> GetBookingsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc");
        public Task<Result<List<BookingDto>>> GetBookingsByDateAsync(DateOnly date);
        public Task<Result<bool>> DeleteBookingAsync(Guid bookingId);

    }
}
