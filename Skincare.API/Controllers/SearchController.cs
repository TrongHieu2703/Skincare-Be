using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpPost("full-text")]
        public async Task<IActionResult> FullTextSearch([FromBody] FullTextSearchDto searchDto, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchDto.Query))
                {
                    var errorResponse = ApiResponse<FullTextSearchResultDto>.ErrorResult(
                        "Search query is required",
                        new List<ApiError> { ApiError.ValidationError("query", "Search query cannot be empty") }
                    );
                    return BadRequest(errorResponse);
                }

                var result = await _searchService.FullTextSearchAsync(searchDto, pageNumber, pageSize);
                
                var response = ApiResponse<FullTextSearchResultDto>.SuccessResult(result, "Full-text search completed successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                response.Metadata.ResponseTimeMs = result.Took;
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in full-text search");
                var errorResponse = ApiResponse<FullTextSearchResultDto>.ErrorResult(
                    "Failed to perform full-text search",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    var errorResponse = ApiResponse<SearchSuggestionsDto>.ErrorResult(
                        "Search query is required",
                        new List<ApiError> { ApiError.ValidationError("query", "Search query cannot be empty") }
                    );
                    return BadRequest(errorResponse);
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(query, limit);
                
                var response = ApiResponse<SearchSuggestionsDto>.SuccessResult(suggestions, "Search suggestions retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions");
                var errorResponse = ApiResponse<SearchSuggestionsDto>.ErrorResult(
                    "Failed to get search suggestions",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetAutocompleteSuggestions([FromQuery] string query, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    var errorResponse = ApiResponse<List<string>>.ErrorResult(
                        "Search query is required",
                        new List<ApiError> { ApiError.ValidationError("query", "Search query cannot be empty") }
                    );
                    return BadRequest(errorResponse);
                }

                var suggestions = await _searchService.GetAutocompleteSuggestionsAsync(query, limit);
                
                var response = ApiResponse<List<string>>.SuccessResult(suggestions, "Autocomplete suggestions retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete suggestions");
                var errorResponse = ApiResponse<List<string>>.ErrorResult(
                    "Failed to get autocomplete suggestions",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("smart-suggestions")]
        public async Task<IActionResult> GetSmartSuggestions([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    var errorResponse = ApiResponse<List<SearchSuggestionDto>>.ErrorResult(
                        "Search query is required",
                        new List<ApiError> { ApiError.ValidationError("query", "Search query cannot be empty") }
                    );
                    return BadRequest(errorResponse);
                }

                var suggestions = await _searchService.GetSmartSuggestionsAsync(query, limit);
                
                var response = ApiResponse<List<SearchSuggestionDto>>.SuccessResult(suggestions, "Smart suggestions retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting smart suggestions");
                var errorResponse = ApiResponse<List<SearchSuggestionDto>>.ErrorResult(
                    "Failed to get smart suggestions",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingSearches([FromQuery] int limit = 10)
        {
            try
            {
                var trendingSearches = await _searchService.GetTrendingSearchesAsync(limit);
                
                var response = ApiResponse<List<TrendingSearchDto>>.SuccessResult(trendingSearches, "Trending searches retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending searches");
                var errorResponse = ApiResponse<List<TrendingSearchDto>>.ErrorResult(
                    "Failed to get trending searches",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("popular-products")]
        public async Task<IActionResult> GetPopularProducts([FromQuery] int limit = 10)
        {
            try
            {
                var popularProducts = await _searchService.GetPopularProductsAsync(limit);
                
                var response = ApiResponse<List<PopularProductDto>>.SuccessResult(popularProducts, "Popular products retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular products");
                var errorResponse = ApiResponse<List<PopularProductDto>>.ErrorResult(
                    "Failed to get popular products",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSearchAnalytics([FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow.Date;
                var analytics = await _searchService.GetSearchAnalyticsAsync(targetDate);
                
                var response = ApiResponse<SearchAnalyticsDto>.SuccessResult(analytics, "Search analytics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search analytics");
                var errorResponse = ApiResponse<SearchAnalyticsDto>.ErrorResult(
                    "Failed to get search analytics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("metrics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSearchMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    var errorResponse = ApiResponse<SearchMetricsDto>.ErrorResult(
                        "Invalid date range",
                        new List<ApiError> { ApiError.ValidationError("dateRange", "Start date must be before end date") }
                    );
                    return BadRequest(errorResponse);
                }

                var metrics = await _searchService.GetSearchMetricsAsync(startDate, endDate);
                
                var response = ApiResponse<SearchMetricsDto>.SuccessResult(metrics, "Search metrics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search metrics");
                var errorResponse = ApiResponse<SearchMetricsDto>.ErrorResult(
                    "Failed to get search metrics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("index-product")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IndexProduct([FromBody] ProductDto product)
        {
            try
            {
                var result = await _searchService.IndexProductAsync(product);
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to index product",
                        new List<ApiError> { ApiError.BusinessError("INDEX_FAILED", "Product indexing failed") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Product indexed successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing product");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to index product",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("rebuild-index")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RebuildSearchIndex()
        {
            try
            {
                var result = await _searchService.RebuildSearchIndexAsync();
                
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Failed to rebuild search index",
                        new List<ApiError> { ApiError.BusinessError("REBUILD_FAILED", "Search index rebuild failed") }
                    );
                    return StatusCode(500, errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Search index rebuilt successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding search index");
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to rebuild search index",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("index-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSearchIndexStats()
        {
            try
            {
                var stats = await _searchService.GetSearchIndexStatsAsync();
                
                var response = ApiResponse<Dictionary<string, object>>.SuccessResult(stats, "Search index stats retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search index stats");
                var errorResponse = ApiResponse<Dictionary<string, object>>.ErrorResult(
                    "Failed to get search index stats",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
} 