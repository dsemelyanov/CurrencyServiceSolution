
using BackgroundService.Settings;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BackgroundService.Services.HttpClients.CbrSender
{
    public class CbrSender : ICbrSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _currencyApiUrl;

        public CbrSender(
            HttpClient httpClient
            , IOptions<ApiSettings> apiSettings
            )
        {
            _httpClient = httpClient;
            _currencyApiUrl = apiSettings.Value.CurrencyApiUrl ?? throw new ArgumentNullException(nameof(_currencyApiUrl));
        }

        // Метод с ручным управлением кодировкой
        public async Task<XDocument?> GetCbr(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_currencyApiUrl, cancellationToken);

            response.EnsureSuccessStatusCode(); // Проверяем, что статус-код 2xx

            // Читаем содержимое как массив байтов
            var contentBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            // Декодируем содержимое из Windows-1251 в строку
            var encoding = Encoding.GetEncoding("windows-1251");
            var xmlData = encoding.GetString(contentBytes);
            var xmlDoc = XDocument.Parse(xmlData);

            return xmlDoc;
        }

        // Метод для прямой загрузки XML из потока ответа
        public async Task<XDocument?> GetCbrLoad(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_currencyApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var xmlDoc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

            return xmlDoc;
        }
    }
}
