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
            return Ok(bookings);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }
            return Ok(booking);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingByUserID(Guid userId, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20, string sort = "asc")
        {
            var booking = await _bookingService.GetBookingsByUserAsync(userId, from, to, page, pageSize, sort);
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }
            return Ok(booking);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var tenantId = dto.TenantId;
                var booking = await _bookingService.CreateBookingAsync(tenantId, dto);
                if (!booking)
                {
                    return BadRequest("Failed to create booking.");
                }

                return Ok("Booking created successfully.");
            }
            catch 
            {
                return NotFound("Failed to create booking.");
            }
        }

        [HttpDelete]

        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            try
            {
                await _bookingService.DeleteBookingAsync(bookingId);
                return Ok("Booking deleted successfully.");

            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found.");
            }

        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(Guid bookingId, [FromBody] UpdateBookingDto dto)
        {
            try
            {
                await _bookingService.UpdateBookingAsync(bookingId, dto);
                return Ok("Booking updated successfully.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found.");
            }


        }
    }
}
