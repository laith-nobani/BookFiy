using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using IDatabase = StackExchange.Redis.IDatabase;

namespace BookFiy.Application.Services
{
    public class RedisService
    {
        private readonly IDatabase _database;
        public RedisService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SetValueAsync<T>(string key, T value, TimeSpan expiry)
        {
            if (value == null)
                return;

            var serializedValue = System.Text.Json.JsonSerializer.Serialize(value);
            


            await _database.StringSetAsync(key, serializedValue, expiry);
        }


        public async Task<T?> GetValueAsync<T>(string key)
        {
            var serializedValue = await _database.StringGetAsync(key);

            if (serializedValue.IsNullOrEmpty)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(
                    serializedValue.ToString(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return default;
            }
        }
        public async Task RemoveValueAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        // 

        public async Task<bool> TryLockAsync(string key, TimeSpan expiry)
        {
            return await _database.LockTakeAsync(key, Environment.MachineName, expiry);
        }

        public async Task UnlockAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
        public async Task<bool> SafeUnlockAsync(string key, string value)
        {
            var currentValue = await _database.StringGetAsync(key);

            if (currentValue == value)
            {
                return await _database.KeyDeleteAsync(key);
            }

            return false;
        }




    }

}
