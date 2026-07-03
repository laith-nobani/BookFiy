using BookFiy.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Data.Configs
{
    public class OtpConfiguration: IEntityTypeConfiguration<Otp>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Otp> builder)
        {
            builder.ToTable("Otp");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(o => o.CodeHash)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(o => o.ExpiresAt)
                .IsRequired();
            builder.Property(o => o.IsUsed)
                .IsRequired();
            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
