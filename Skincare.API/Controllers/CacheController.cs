using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.Services.Interfaces;
using Skincare.Services.Constants;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpPost("clear-all")]
        public async Task<IActionResult> ClearAllCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync("*");
                _logger.LogInformation("All cache cleared by admin");
                
                return Ok(new { message = "All cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache");
                return StatusCode(500, new { message = "Error clearing cache", details = ex.Message });
            }
        }

        [HttpPost("clear-products")]
        public async Task<IActionResult> ClearProductCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern);
                _logger.LogInformation("Product cache cleared by admin");
                
                return Ok(new { message = "Product cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing product cache");
                return StatusCode(500, new { message = "Error clearing product cache", details = ex.Message });
            }
        }

        [HttpPost("clear-categories")]
        public async Task<IActionResult> ClearCategoryCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.CategoryPattern);
                _logger.LogInformation("Category cache cleared by admin");
                
                return Ok(new { message = "Category cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing category cache");
                return StatusCode(500, new { message = "Error clearing category cache", details = ex.Message });
            }
        }

        [HttpPost("clear-users")]
        public async Task<IActionResult> ClearUserCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.UserPattern);
                _logger.LogInformation("User cache cleared by admin");
                
                return Ok(new { message = "User cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user cache");
                return StatusCode(500, new { message = "Error clearing user cache", details = ex.Message });
            }
        }

        [HttpPost("clear-orders")]
        public async Task<IActionResult> ClearOrderCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.OrderPattern);
                _logger.LogInformation("Order cache cleared by admin");
                
                return Ok(new { message = "Order cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing order cache");
                return StatusCode(500, new { message = "Error clearing order cache", details = ex.Message });
            }
        }

        [HttpPost("clear-reviews")]
        public async Task<IActionResult> ClearReviewCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.ReviewPattern);
                _logger.LogInformation("Review cache cleared by admin");
                
                return Ok(new { message = "Review cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing review cache");
                return StatusCode(500, new { message = "Error clearing review cache", details = ex.Message });
            }
        }

        [HttpPost("clear-dashboard")]
        public async Task<IActionResult> ClearDashboardCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.DashboardPattern);
                _logger.LogInformation("Dashboard cache cleared by admin");
                
                return Ok(new { message = "Dashboard cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing dashboard cache");
                return StatusCode(500, new { message = "Error clearing dashboard cache", details = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetCacheStats()
        {
            try
            {
                // This is a basic implementation - in a real scenario, you might want to get more detailed stats
                var stats = new
                {
                    message = "Cache statistics",
                    note = "Redis statistics would be available here in a production environment",
                    cacheService = "Redis",
                    status = "Active"
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache stats");
                return StatusCode(500, new { message = "Error getting cache stats", details = ex.Message });
            }
        }
    }
} 