namespace CacheService.Services.CacherService
{
    public interface ICacherService
    {
        Task SetCurrencyAsync(string key, decimal value);
        Task<decimal?> GetCurrencyAsync(string key);
    }
}
