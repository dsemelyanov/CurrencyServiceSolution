using CacheService.Services.CacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CacheService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Защищаем контроллер JWT-аутентификацией
    public class CacheController : ControllerBase
    {
        private readonly ICacherService _cacheService;

        public CacheController(ICacherService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("{currencyName}")]
        public async Task<ActionResult<decimal?>> GetCurrency(string currencyName)
        {
            var rate = await _cacheService.GetCurrencyAsync(currencyName);
            if (rate.HasValue)
            {
                return Ok(rate.Value);
            }
            return NotFound();
        }

        [HttpPost("{currencyName}/{rate}")]
        public async Task<IActionResult> SetCurrency(string currencyName, decimal rate)
        {
            await _cacheService.SetCurrencyAsync(currencyName, rate);
            return Ok();
        }
    }
}
