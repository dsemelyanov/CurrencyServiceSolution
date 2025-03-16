using System.Xml.Linq;
using System;
using SharedModels.Data;
using SharedModels.Models;

namespace BackgroundService.Services.Background
{
    public class CurrencyUpdateService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;

        public CurrencyUpdateService(HttpClient httpClient, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateCurrencies();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Обновление раз в сутки
            }
        }

        private async Task UpdateCurrencies()
        {
            var xmlData = await _httpClient.GetStringAsync("http://www.cbr.ru/scripts/XML_daily.asp");
            var xmlDoc = XDocument.Parse(xmlData);
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var valute in xmlDoc.Descendants("Valute"))
            {
                var name = valute.Element("CharCode")?.Value;
                var rateStr = valute.Element("Value")?.Value.Replace(",", ".");
                if (decimal.TryParse(rateStr, out var rate))
                {
                    var currency = dbContext.Currencies.FirstOrDefault(c => c.Name == name);
                    if (currency == null)
                    {
                        dbContext.Currencies.Add(new Currency { Name = name, Rate = rate });
                    }
                    else
                    {
                        currency.Rate = rate;
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
