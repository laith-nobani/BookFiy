using BookFiy.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFiy.Infrastructure.Data.Configs
{
    internal class BookingAuditConfiguration : IEntityTypeConfiguration<BookingAudit>
    {
        public void Configure(EntityTypeBuilder<BookingAudit> builder)
        {
            builder.ToTable("BookingAudit");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.EventType).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Data).HasColumnType("nvarchar(max)");
            builder.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.HasIndex(b => b.BookingId);
            builder.HasIndex(b => b.TenantId);
        }
    }
}
