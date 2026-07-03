using BookFiy.Domain.Entites;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Repositories
{
    public class BookingStatusRepository : IBookingStatusRepository
    {
        private readonly AppDbContext _dbContext;

        public BookingStatusRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> ExistsAsync(int id)
        {
             var exists = await _dbContext.Set<BookingStatus>().AnyAsync(bs => bs.Id == id);
            return exists;
        }

        public async Task<List<BookingStatus>> GetAllAsync()
        {
            var bookingStatuses = await _dbContext
                .Set<BookingStatus>()
                .AsNoTracking()
                .ToListAsync();
            return bookingStatuses;

        }

        public async Task<BookingStatus?> GetByIdAsync(int id)
        {
             var bookingStatus = await _dbContext.Set<BookingStatus>().FirstOrDefaultAsync(bs => bs.Id == id);
            return bookingStatus;
        }

        public async Task<BookingStatus?> GetByNameAsync(string name)
        {
            var bookingStatus = await _dbContext.Set<BookingStatus>().FirstOrDefaultAsync(bs => bs.Name == name);
            return bookingStatus;
        }
    }
}
