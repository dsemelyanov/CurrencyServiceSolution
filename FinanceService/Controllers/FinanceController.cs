using FinanceService.Services.FinanceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        private readonly ILogger<FinanceController> _logger;

        public FinanceController(
            IFinanceService financeService
            , ILogger<FinanceController> logger
            )
        {
            _financeService = financeService;
            _logger = logger;
        }

        [HttpGet("{currencyName}")]
        public async Task<ActionResult<decimal>> GetExchangeRate(string currencyName)
        {
            var rate = await _financeService.GetExchangeRateAsync(currencyName);
            if (rate > 0)
            {
                return Ok(rate);
            }
            return NotFound();
        }

        [HttpPost("{currencyName}/{rate}")]
        public async Task<IActionResult> UpdateExchangeRate(string currencyName, decimal rate)
        {
            await _financeService.UpdateExchangeRateAsync(currencyName, rate);
            return Ok();
        }

        // Получение списка валют из интересов пользователя
        [HttpGet("interests")]
        public async Task<ActionResult<List<Currency>>> GetUserInterests()
        {
            _logger.LogInformation("Processing GetUserInterests request");
            try
            {
                // Логируем все claims
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
                }

                var username = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username not found in token");
                    return Unauthorized("Username not found in token.");
                }

                var interests = await _financeService.GetUserInterestsAsync(username);
                return Ok(interests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Добавление валюты в список интересов пользователя
        [HttpPost("interests/{currencyName}")]
        public async Task<IActionResult> AddCurrencyToInterests(string currencyName)
        {
            try
            {
                var username = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized("Username not found in token.");
                }

                await _financeService.AddCurrencyToInterestsAsync(username, currencyName);
                return Ok("Currency added to interests.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
