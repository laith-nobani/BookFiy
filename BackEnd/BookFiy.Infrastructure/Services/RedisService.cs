using BookFiy.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using IDatabase = StackExchange.Redis.IDatabase;

namespace BookFiy.Application.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;
        private const string Prefix = "bookfiy:";

        public RedisService(IConnectionMultiplexer redis)
        {
            _database =redis.GetDatabase();
        }

        private static readonly JsonSerializerOptions JsonOptions =
           new()
           {
               PropertyNameCaseInsensitive = true
           };

        public async Task<bool> ExistsAsync(string key)
        {
            
            return await _database.KeyExistsAsync(Prefix + key);

        }

        public async Task<T?> GetAsync<T>(string key)
        {

           return await _database.StringGetAsync(Prefix + key) is RedisValue value && value.HasValue
                ? JsonSerializer.Deserialize<T>(value)
                : default;
        }

        public Task RemoveAsync(string key)
        {
          return _database.KeyDeleteAsync(Prefix + key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            if (value == null)
                return;

            var json = JsonSerializer.Serialize(value,JsonOptions);

            await _database.StringSetAsync(Prefix + key, json, expiration);
        }
 
    }

}
