using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedModels.Data;
using SharedModels.Models;
using SharedModels.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models.UserModels;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public AuthController(AppDbContext dbContext, IOptions<JwtSettings> jwtSettings)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Name == model.Username))
            {
                return BadRequest("Username already exists.");
            }

            var user = new User
            {
                Name = model.Username,
                Password = model.Password
                //Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Name == model.Username
                && u.Password == model.Password
                //&& BCrypt.Net.BCrypt.Verify(model.Password, user.Password)
            );
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // В stateless JWT логаут обычно реализуется на клиенте (удаление токена)
            // Здесь можно добавить логику, например, добавление токена в чёрный список (если используется)
            return Ok("Logged out successfully.");
        }

        private string GenerateJwtToken(User user)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            if (keyBytes.Length < 32)
            {
                throw new ArgumentException("SecretKey must be at least 32 bytes after UTF-8 encoding. Current length: " + keyBytes.Length + " bytes.");
            }

            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
