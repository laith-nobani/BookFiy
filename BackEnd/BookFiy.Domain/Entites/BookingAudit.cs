using System;

namespace BookFiy.Domain.Entites
{
    public class BookingAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookingId { get; set; }
        public Guid TenantId { get; set; }
        public string EventType { get; set; } = string.Empty; 
        public string Data { get; set; } = string.Empty;
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
