using BookFiy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.Entites
{
    public class Booking : BaseEntity
    {
        public Guid TenantId { get; set; }

        public Guid ServiceId { get; set; }
        public Service Service { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int StatusId { get; set; }
        public BookingStatus Status{ get; set; }
        
        public string? Notes { get; set; }
    }


}
