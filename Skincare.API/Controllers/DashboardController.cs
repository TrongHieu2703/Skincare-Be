using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        /// <param name="request">Dashboard request parameters including time range</param>
        /// <returns>All dashboard statistics in a single response</returns>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats([FromQuery] DashboardRequestDto request)
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync(request);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving dashboard statistics");
            }
        }

        /// <summary>
        /// Get product-related statistics only
        /// </summary>
        /// <param name="startDate">Optional start date for time-based statistics</param>
        /// <param name="endDate">Optional end date for time-based statistics</param>
        /// <returns>Product statistics</returns>
        [HttpGet("products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductStatsDto>> GetProductStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var stats = await _dashboardService.GetProductStatsAsync(start, end);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving product statistics");
            }
        }

        /// <summary>
        /// Get customer-related statistics only
        /// </summary>
        /// <param name="startDate">Optional start date for time-based statistics</param>
        /// <param name="endDate">Optional end date for time-based statistics</param>
        /// <returns>Customer statistics</returns>
        [HttpGet("customers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerStatsDto>> GetCustomerStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var stats = await _dashboardService.GetCustomerStatsAsync(start, end);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving customer statistics");
            }
        }

        /// <summary>
        /// Get order-related statistics only
        /// </summary>
        /// <param name="startDate">Optional start date for time-based statistics</param>
        /// <param name="endDate">Optional end date for time-based statistics</param>
        /// <param name="interval">Interval for time-based charts (hour, day, month, year)</param>
        /// <returns>Order statistics</returns>
        [HttpGet("orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderStatsDto>> GetOrderStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string interval = "day")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var stats = await _dashboardService.GetOrderStatsAsync(start, end, interval);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving order statistics");
            }
        }

        /// <summary>
        /// Get revenue-related statistics only
        /// </summary>
        /// <param name="startDate">Optional start date for time-based statistics</param>
        /// <param name="endDate">Optional end date for time-based statistics</param>
        /// <param name="interval">Interval for time-based charts (hour, day, month, year)</param>
        /// <returns>Revenue statistics</returns>
        [HttpGet("revenue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RevenueStatsDto>> GetRevenueStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string interval = "day")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var stats = await _dashboardService.GetRevenueStatsAsync(start, end, interval);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving revenue statistics");
            }
        }

        /// <summary>
        /// Get best selling products
        /// </summary>
        /// <param name="startDate">Optional start date for time-based statistics</param>
        /// <param name="endDate">Optional end date for time-based statistics</param>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of best selling products</returns>
        [HttpGet("bestsellers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TopProductDto[]>> GetBestSellingProducts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 5)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var products = await _dashboardService.GetBestSellingProductsAsync(start, end, limit);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best selling products");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving best selling products");
            }
        }

        /// <summary>
        /// Get top rated products
        /// </summary>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of top rated products</returns>
        [HttpGet("toprated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TopProductDto[]>> GetTopRatedProducts([FromQuery] int limit = 5)
        {
            try
            {
                var products = await _dashboardService.GetTopRatedProductsAsync(limit);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top rated products");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving top rated products");
            }
        }
    }
} 