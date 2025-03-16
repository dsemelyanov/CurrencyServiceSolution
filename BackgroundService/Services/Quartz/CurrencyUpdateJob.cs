using Quartz;
using System.Xml.Linq;
using System;
using SharedModels.Data;
using SharedModels.Models;
using BackgroundService.Services.HttpClients.CbrSender;
using System.Globalization;

namespace BackgroundService.Services.Quartz
{
    public class CurrencyUpdateJob : IJob
    {
        private readonly ICbrSender _cbrSender;
        private readonly IServiceScopeFactory _scopeFactory;

        public CurrencyUpdateJob(ICbrSender cbrSender, IServiceScopeFactory scopeFactory)
        {
            _cbrSender = cbrSender;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                // Получаем токен отмены из контекста
                var cancellationToken = context.CancellationToken;

                //var xmlDoc = await _cbrSender.GetCbr(cancellationToken);
                var xmlDoc = await _cbrSender.GetCbrLoad(cancellationToken);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                foreach (var valute in xmlDoc.Descendants("Valute"))
                {
                    var name = valute.Element("CharCode")?.Value;
                    var rateStr = valute.Element("Value")?.Value.Replace(",", ".");
                    if (decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
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
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Currency update job was cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CurrencyUpdateJob: {ex.Message}");
                throw;
            }
        }
    }
}
