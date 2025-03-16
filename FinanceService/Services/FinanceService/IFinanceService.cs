using SharedModels.Models;

namespace FinanceService.Services.FinanceService
{
    public interface IFinanceService
    {
        Task<decimal> GetExchangeRateAsync(string currencyName);
        Task UpdateExchangeRateAsync(string currencyName, decimal rate);
        Task<List<Currency>> GetUserInterestsAsync(string username);
        Task AddCurrencyToInterestsAsync(string username, string currencyName);
    }
}
