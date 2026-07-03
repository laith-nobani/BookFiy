using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Booking
{
    public class BookingDto 
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
