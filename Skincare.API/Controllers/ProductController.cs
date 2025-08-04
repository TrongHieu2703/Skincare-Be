using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Services.Interfaces;
using Skincare.Services.Implements;
using System.Text.Json;
using Skincare.API.Attributes;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "name",
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                
                var response = ApiResponse<IEnumerable<ProductDto>>.PaginatedResult(
                    products, 
                    totalItems, 
                    pageNumber, 
                    pageSize, 
                    "Products retrieved successfully"
                );

                // Add deprecation metadata for V1
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                var errorResponse = ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Failed to retrieve products",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    var errorResponse = ApiResponse<ProductDto>.ErrorResult(
                        "Product not found",
                        new List<ApiError> { ApiError.BusinessError("PRODUCT_NOT_FOUND", "Product with specified ID does not exist") }
                    );
                    return NotFound(errorResponse);
                }

                var response = ApiResponse<ProductDto>.SuccessResult(product, "Product retrieved successfully");
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
                var errorResponse = ApiResponse<ProductDto>.ErrorResult(
                    "Failed to retrieve product",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);
                var response = ApiResponse<ProductDto>.SuccessResult(product, "Product created successfully");
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, response);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => ApiError.ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                var errorResponse = ApiResponse<ProductDto>.ErrorResult("Validation failed", errors);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                var errorResponse = ApiResponse<ProductDto>.ErrorResult(
                    "Failed to create product",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                if (product == null)
                {
                    var errorResponse = ApiResponse<ProductDto>.ErrorResult(
                        "Product not found",
                        new List<ApiError> { ApiError.BusinessError("PRODUCT_NOT_FOUND", "Product with specified ID does not exist") }
                    );
                    return NotFound(errorResponse);
                }

                var response = ApiResponse<ProductDto>.SuccessResult(product, "Product updated successfully");
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => ApiError.ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                var errorResponse = ApiResponse<ProductDto>.ErrorResult("Validation failed", errors);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                var errorResponse = ApiResponse<ProductDto>.ErrorResult(
                    "Failed to update product",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    var errorResponse = ApiResponse<bool>.ErrorResult(
                        "Product not found",
                        new List<ApiError> { ApiError.BusinessError("PRODUCT_NOT_FOUND", "Product with specified ID does not exist") }
                    );
                    return NotFound(errorResponse);
                }

                var response = ApiResponse<bool>.SuccessResult(true, "Product deleted successfully");
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                var errorResponse = ApiResponse<bool>.ErrorResult(
                    "Failed to delete product",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    var errorResponse = ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                        "Search keyword is required",
                        new List<ApiError> { ApiError.ValidationError("keyword", "Search keyword cannot be empty") }
                    );
                    return BadRequest(errorResponse);
                }

                var (products, totalPages, totalItems) = await _productService.SearchProductsAsync(keyword, pageNumber, pageSize);
                
                var response = ApiResponse<IEnumerable<ProductDto>>.PaginatedResult(
                    products, 
                    totalItems, 
                    pageNumber, 
                    pageSize, 
                    "Search completed successfully"
                );
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with keyword: {Keyword}", keyword);
                var errorResponse = ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Failed to search products",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("advanced-search")]
        public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedSearchDto searchDto)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.AdvancedSearchAsync(searchDto);
                
                var response = ApiResponse<IEnumerable<ProductDto>>.PaginatedResult(
                    products, 
                    totalItems, 
                    searchDto.PageNumber, 
                    searchDto.PageSize, 
                    "Advanced search completed successfully"
                );
                response.Metadata.Version = "1.0";
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();
                
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => ApiError.ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                var errorResponse = ApiResponse<IEnumerable<ProductDto>>.ErrorResult("Validation failed", errors);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
                var errorResponse = ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Failed to perform advanced search",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // GET: api/Product/product-type/5
        [HttpGet("product-type/{productTypeId}")]
        public async Task<IActionResult> GetProductsByType(int productTypeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetProductsByTypeWithPaginationAsync(productTypeId, pageNumber, pageSize);
                
                var response = new { 
                    message = "Products by type retrieved successfully", 
                    data = products,
                    pagination = new {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalPages = totalPages,
                        totalItems = totalItems
                    },
                    meta = new {
                        version = "1.0",
                        deprecated = true,
                        deprecationDate = "2024-12-31",
                        migrationGuide = "/api/docs/migration-v2"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByType for type {productTypeId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/branch/5
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetProductsByBranch(int branchId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetProductsByBranchWithPaginationAsync(branchId, pageNumber, pageSize);
                
                var response = new { 
                    message = "Products by branch retrieved successfully", 
                    data = products,
                    pagination = new {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalPages = totalPages,
                        totalItems = totalItems
                    },
                    meta = new {
                        version = "1.0",
                        deprecated = true,
                        deprecationDate = "2024-12-31",
                        migrationGuide = "/api/docs/migration-v2"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByBranch for branch {branchId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/skin-type/5
        [HttpGet("skin-type/{skinTypeId}")]
        public async Task<IActionResult> GetProductsBySkinType(int skinTypeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetProductsBySkinTypeWithPaginationAsync(skinTypeId, pageNumber, pageSize);
                
                var response = new { 
                    message = "Products by skin type retrieved successfully", 
                    data = products,
                    pagination = new {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalPages = totalPages,
                        totalItems = totalItems
                    },
                    meta = new {
                        version = "1.0",
                        deprecated = true,
                        deprecationDate = "2024-12-31",
                        migrationGuide = "/api/docs/migration-v2"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinType for skin type {skinTypeId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            var response = new { 
                message = "Product API is working", 
                timestamp = DateTime.UtcNow,
                version = "1.0",
                deprecated = true,
                deprecationDate = "2024-12-31",
                migrationGuide = "/api/docs/migration-v2"
            };
            return Ok(response);
        }

        // POST: api/Product/compare
        [HttpPost("compare")]
        public async Task<IActionResult> CompareProducts([FromBody] CompareRequestDto compareRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid request data", errors = ModelState });
                }

                var products = await _productService.CompareProductsAsync(compareRequestDto);
                
                var response = new { 
                    message = "Products comparison completed successfully", 
                    data = products,
                    comparisonInfo = new {
                        productIds = compareRequestDto.ProductIds,
                        productsCount = products.Count()
                    },
                    meta = new {
                        version = "1.0",
                        deprecated = true,
                        deprecationDate = "2024-12-31",
                        migrationGuide = "/api/docs/migration-v2"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompareProducts: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }

    internal class _fileService
    {
    }

    public static class MemoryCacheExtensions
    {
        private static readonly Lazy<object> _keys = new Lazy<object>(() => { return new object(); });
        private static readonly Dictionary<object, HashSet<string>> _keysCollection = new Dictionary<object, HashSet<string>>();

        public static HashSet<string> GetKeys<T>(this IMemoryCache memoryCache)
        {
            return _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
        }

        public static void AddKey(this IMemoryCache memoryCache, string key)
        {
            var keys = memoryCache.GetKeys<object>();
            keys.Add(key);
        }

        public static void RemoveKey(this IMemoryCache memoryCache, string key)
        {
            var keys = memoryCache.GetKeys<object>();
            keys.Remove(key);
        }

        private static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dict.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = valueFactory(key);
            dict[key] = value;
            return value;
        }
    }
}

