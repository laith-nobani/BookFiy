using BookFiy.Domain.Constants;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Data.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public Guid? CurrentTenantId { get; set; }

        public DbSet<Tenant> tenants { get; set; }
        public DbSet<Employee> employees { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingAudit> BookingAudits { get; set; }

        public DbSet<BookingStatus> BookingStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new Configs.TenantConfiguration());
            modelBuilder.ApplyConfiguration(new Configs.EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new Configs.OtpConfiguration());
            modelBuilder.ApplyConfiguration(new Configs.ServiceConfiguration());
            modelBuilder.ApplyConfiguration(new Configs.BookingAuditConfiguration());
            modelBuilder.ApplyConfiguration(new Configs.BookingConfiguration());
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshToken");
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId);
            modelBuilder.Entity<BookingStatus>().HasData(
     new BookingStatus
     {
         Id = 1,
         Name = "Pending",
     },
     new BookingStatus
     {
         Id = 2,
         Name = "Confirmed",
     },
     new BookingStatus
     {
         Id = 3,
         Name = "Cancelled",
     },
     new BookingStatus
     {
         Id = 4,
         Name = "Completed",
     },
     new BookingStatus
     {
         Id = 5,
         Name = "No Show",
         });

        }
    }
}
