using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.IRepositories
{
    
        public interface IBookingStatusRepository
        {
            Task<List<BookingStatus>> GetAllAsync();

            Task<BookingStatus?> GetByIdAsync(int id);

            Task<BookingStatus?> GetByNameAsync(string name);

            Task<bool> ExistsAsync(int id);
        }
    
}
