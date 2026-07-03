using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Booking
{
    public class CreateBookingDto
    {
        public Guid TenantId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }

    }
}
