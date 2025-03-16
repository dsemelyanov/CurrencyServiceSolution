using Microsoft.Extensions.Options;
using SharedModels.Settings;
using StackExchange.Redis;
using System.Text.Json;

namespace CacheService.Services.CacherService
{
    public class CacherService : ICacherService
    {
        private readonly StackExchange.Redis.IDatabase _redisDb;
        private readonly JwtSettings _jwtSettings;

        public CacherService(IConnectionMultiplexer redis, IOptions<JwtSettings> jwtSettings)
        {
            _redisDb = redis.GetDatabase();
            _jwtSettings = jwtSettings.Value;
        }

        public async Task SetCurrencyAsync(string key, decimal value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _redisDb.StringSetAsync(key, jsonValue, TimeSpan.FromMinutes(_jwtSettings.TokenExpiryMinutes));
        }

        public async Task<decimal?> GetCurrencyAsync(string key)
        {
            var jsonValue = await _redisDb.StringGetAsync(key);
            return jsonValue.HasValue ? JsonSerializer.Deserialize<decimal>(jsonValue) : null;
        }
    }
}
