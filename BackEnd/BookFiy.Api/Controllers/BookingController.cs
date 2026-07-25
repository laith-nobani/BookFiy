using BookFiy.Api.Extensions;
using BookFiy.Application.Dtos.Booking;
using BookFiy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookFiy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("empoloyee/{employeeId}")]
        public async Task<IActionResult> GetBookings(Guid employeeId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var bookings = await _bookingService.GetBookingsByEmployeeAsync(employeeId, from, to, page, pageSize, sort);
            return Ok(bookings.Data);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var res = await _bookingService.GetBookingByIdAsync(bookingId);
            if (!res.IsSuccess)
            {
                return NotFound(res.Message);
            }
            return Ok(res.Data);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingByUserID(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var res = await _bookingService.GetBookingsByUserAsync(userId, from, to, page, pageSize, sort);
            if (!res.IsSuccess)
            {
                return NotFound(res.Message);
            }
            return Ok(res.Data);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            return (await _bookingService.CreateBookingAsync(dto.TenantId, dto))
            .ToActionResult();
        }

        [HttpDelete]

        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            return (await _bookingService.DeleteBookingAsync(bookingId))
                .ToActionResult();

        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(Guid bookingId, [FromBody] UpdateBookingDto dto)
        {
            return (await _bookingService.UpdateBookingAsync(bookingId,dto))
                .ToActionResult();
        }
    }
}
