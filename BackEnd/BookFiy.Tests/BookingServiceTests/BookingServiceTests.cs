using BookFiy.Application.Dtos.Booking;
using BookFiy.Application.Interfaces;
using BookFiy.Application.Services;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BookFiy.Tests.BookingServiceTests
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IBookingStatusRepository> _statusRepo = new();
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IEmployeeRepository> _employeeRepo = new();
        private readonly Mock<ITenantProvider> _tenantProvider = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly BookingService _service;

        public BookingServiceTests()
        {
            _userManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);

            _service = new BookingService(
                _tenantProvider.Object,
                _bookingRepo.Object,
                _statusRepo.Object,
                _serviceRepo.Object,
                _employeeRepo.Object,
                _dbContext,
                null!,
                _userManager.Object
            );
        }



        [Fact]
        public async Task UpdateBooking_ShouldUpdateStatus()
        {
            var tenantId = Guid.NewGuid();

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StatusId = 1
            };

            _tenantProvider.Setup(x => x.TenantId).Returns(tenantId);

            _bookingRepo.Setup(x => x.GetByIdAsync(booking.Id))
                .ReturnsAsync(booking);

            var dto = new UpdateBookingDto
            {
                StatusId = 2
            };

            await _service.UpdateBookingAsync(booking.Id, dto);

            Assert.Equal(2, booking.StatusId);
        }

        [Fact]
        public async Task UpdateBooking_ShouldUpdateNotes()
        {
            var tenantId = Guid.NewGuid();

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Notes = "Old Notes"
            };

            _tenantProvider.Setup(x => x.TenantId).Returns(tenantId);

            _bookingRepo.Setup(x => x.GetByIdAsync(booking.Id))
                .ReturnsAsync(booking);

            var dto = new UpdateBookingDto
            {
                Notes = "New Notes"
            };

            await _service.UpdateBookingAsync(booking.Id, dto);

            Assert.Equal("New Notes", booking.Notes);
            _bookingRepo.Verify(x => x.UpdateAsync(), Times.Once);
        }
        [Fact]
        public async Task UpdateBooking_ShouldUpdateTime_WhenNoConflict()
        {
            var tenantId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EmployeeId = employeeId,
                ServiceId = serviceId,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(30)
            };

            var newStart = DateTime.UtcNow.AddDays(1);

            _tenantProvider.Setup(x => x.TenantId).Returns(tenantId);

            _bookingRepo.Setup(x => x.GetByIdAsync(booking.Id))
                .ReturnsAsync(booking);

            _serviceRepo.Setup(x => x.GetByIdAsync(serviceId, tenantId))
                .ReturnsAsync(new Service
                {
                    DurationMinutes = 60
                });

            _bookingRepo.Setup(x =>
                x.HasConflictAsync(
                    tenantId,
                    employeeId,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    booking.Id))
                .ReturnsAsync(false);

            await _service.UpdateBookingAsync(
                booking.Id,
                new UpdateBookingDto
                {
                    StartTime = newStart
                });

            Assert.Equal(newStart, booking.StartTime);
            Assert.Equal(newStart.AddMinutes(60), booking.EndTime);
        }

        [Fact]
        public async Task UpdateBooking_ShouldFail_WhenNotFound()
        {
            _bookingRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Booking?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.UpdateBookingAsync(Guid.NewGuid(), new UpdateBookingDto()));
        }
        [Fact]
        public async Task UpdateBooking_ShouldThrow_WhenTimeConflictExists()
        {
            var tenantId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EmployeeId = employeeId,
                ServiceId = serviceId
            };

            _tenantProvider.Setup(x => x.TenantId).Returns(tenantId);

            _bookingRepo.Setup(x => x.GetByIdAsync(booking.Id))
                .ReturnsAsync(booking);

            _serviceRepo.Setup(x => x.GetByIdAsync(serviceId, tenantId))
                .ReturnsAsync(new Service
                {
                    DurationMinutes = 30
                });

            _bookingRepo.Setup(x =>
                x.HasConflictAsync(
                    tenantId,
                    employeeId,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    booking.Id))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateBookingAsync(
                    booking.Id,
                    new UpdateBookingDto
                    {
                        StartTime = DateTime.UtcNow
                    }));
        }

        [Fact]
        public async Task DeleteBooking_ShouldSucceed()
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid()
            };

            _bookingRepo.Setup(x => x.GetByIdAsync(booking.Id))
                .ReturnsAsync(booking);

            _statusRepo.Setup(x => x.GetByNameAsync("Cancelled"))
                .ReturnsAsync(new BookingStatus { Id = 2 });

            await _service.DeleteBookingAsync(booking.Id);

            _bookingRepo.Verify(x => x.UpdateAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteBooking_ShouldFail_WhenNotFound()
        {
            _bookingRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Booking?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteBookingAsync(Guid.NewGuid()));
        }
    }
}