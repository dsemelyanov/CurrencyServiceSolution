using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedModels.Data;
using SharedModels.Models;
using SharedModels.Settings;
using StackExchange.Redis;
using System.Text.Json;

namespace FinanceService.Services.FinanceService
{
    public class FinanceService : IFinanceService
    {
        private readonly IDatabase _redisDb;
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public FinanceService(
            IConnectionMultiplexer redis
            , AppDbContext dbContext
            , IOptions<JwtSettings> jwtSettings
            )
        {
            _redisDb = redis.GetDatabase();
            _dbContext = dbContext;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<decimal> GetExchangeRateAsync(string currencyName)
        {
            var cachedRate = await _redisDb.StringGetAsync(currencyName);
            if (cachedRate.HasValue)
            {
                return JsonSerializer.Deserialize<decimal>(cachedRate);
            }

            var currency = await _dbContext.Currencies.FirstOrDefaultAsync(c => c.Name == currencyName);
            if (currency != null)
            {
                await _redisDb.StringSetAsync(currencyName, JsonSerializer.Serialize(currency.Rate), TimeSpan.FromMinutes(_jwtSettings.TokenExpiryMinutes));
                return currency.Rate;
            }
            return 0m; // Или выбросить исключение, если валюта не найдена
        }

        public async Task UpdateExchangeRateAsync(string currencyName, decimal rate)
        {
            var currency = await _dbContext.Currencies.FirstOrDefaultAsync(c => c.Name == currencyName);
            if (currency != null)
            {
                currency.Rate = rate;
                await _dbContext.SaveChangesAsync();
                await _redisDb.StringSetAsync(currencyName, JsonSerializer.Serialize(rate), TimeSpan.FromMinutes(_jwtSettings.TokenExpiryMinutes));
            }
        }

        // Получение списка валют из интересов пользователя
        public async Task<List<Currency>> GetUserInterestsAsync(string username)
        {
            var user = await _dbContext.Users
                .Include(u => u.Interests)
                .ThenInclude(i => i.Currency)
                .FirstOrDefaultAsync(u => u.Name == username);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user.Interests.Select(i => i.Currency).ToList();
        }

        // Добавление валюты в список интересов пользователя
        public async Task AddCurrencyToInterestsAsync(string username, string currencyName)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Name == username);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var currency = await _dbContext.Currencies
                .FirstOrDefaultAsync(c => c.Name == currencyName);

            if (currency == null)
            {
                throw new Exception("Currency not found.");
            }

            // Проверяем, есть ли уже эта валюта в интересах пользователя
            var existingInterest = await _dbContext.Interests
                .FirstOrDefaultAsync(i => i.UserId == user.Id && i.CurrencyId == currency.Id);

            if (existingInterest != null)
            {
                throw new Exception("Currency already in user's interests.");
            }

            var interest = new Interest
            {
                UserId = user.Id,
                CurrencyId = currency.Id
            };

            _dbContext.Interests.Add(interest);
            await _dbContext.SaveChangesAsync();
        }
    }
}
