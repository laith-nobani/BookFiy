using System;

namespace BookFiy.Application.Dtos.Booking
{
    public class UpdateBookingDto
    {
        public DateTime? StartTime { get; set; }
        public int? StatusId { get; set; }
        public string? Notes { get; set; }
    }
}
