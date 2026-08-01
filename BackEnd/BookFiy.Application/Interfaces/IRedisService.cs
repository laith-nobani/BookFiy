using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFiy.Application.Interfaces
{
    public interface IRedisService
    {
        Task SetAsync<T>(
           string key,
           T value,
           TimeSpan expiration);

        Task<T?> GetAsync<T>(
            string key);

        Task RemoveAsync(
            string key);

        Task<bool> ExistsAsync(
            string key);
    }
}
