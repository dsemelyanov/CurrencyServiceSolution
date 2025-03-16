using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Data;

namespace MigrationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MigrationController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyMigrations()
        {
            try
            {
                Console.WriteLine("Applying migrations...");
                await _dbContext.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully.");
                return Ok("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error applying migrations: {ex.Message}");
            }
        }
    }
}
