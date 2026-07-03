using BookFiy.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Data.Configs
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.Notes).HasMaxLength(500);
            builder.Property(b => b.StatusId).IsRequired();
            builder.Property(b => b.UserId).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.UpdatedAt).IsRequired();
            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany<BookingAudit>()
                .WithOne()
                .HasForeignKey(a => a.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.User)
                   .WithMany()
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Status)
                   .WithMany()
                   .HasForeignKey(b => b.StatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
