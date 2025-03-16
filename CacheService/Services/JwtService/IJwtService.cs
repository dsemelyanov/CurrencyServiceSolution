using SharedModels.Models;

namespace CacheService.Services.JwtService
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
