using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // Cache 5 phút

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger,
            IMemoryCache cache)
        {
            _productService = productService;
            _logger = logger;
            _cache = cache;
        }

        // GET: api/Product
        [HttpGet]
        [ResponseCache(VaryByQueryKeys = new[] { "pageNumber", "pageSize" }, Duration = 300)] // 300 giây = 5 phút
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                string cacheKey = $"Products_Page{pageNumber}_Size{pageSize}";
                
                // Kiểm tra dữ liệu trong cache
                if (_cache.TryGetValue(cacheKey, out object cachedResponse))
                {
                    _logger.LogInformation($"Returning cached products data for page {pageNumber}");
                    return Ok(cachedResponse);
                }
                
                // Nếu không có trong cache, lấy từ database
                var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                var response = new { message = "Products retrieved successfully", data = products };
                
                // Lưu vào cache
                _cache.Set(cacheKey, response, _cacheDuration);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        [ResponseCache(VaryByHeader = "Authorization", VaryByQueryKeys = new[] { "id" }, Duration = 300)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                string cacheKey = $"Product_{id}";
                
                // Kiểm tra dữ liệu trong cache
                if (_cache.TryGetValue(cacheKey, out object cachedResponse))
                {
                    _logger.LogInformation($"Returning cached product data for ID {id}");
                    return Ok(cachedResponse);
                }
                
                // Nếu không có trong cache, lấy từ database
                var product = await _productService.GetProductByIdAsync(id);
                var response = new { message = "Product retrieved successfully", data = product };
                
                // Lưu vào cache
                _cache.Set(cacheKey, response, _cacheDuration);
                
                return Ok(response);
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found");
                return NotFound(new { message = nfex.Message, errorCode = "PRODUCT_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/search?keyword=abc
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Ok(new { message = "No search term provided", data = new List<ProductDto>() });
                }
                
                string cacheKey = $"ProductSearch_{keyword}";
                
                // Kiểm tra dữ liệu trong cache
                if (_cache.TryGetValue(cacheKey, out object cachedResponse))
                {
                    _logger.LogInformation($"Returning cached search results for '{keyword}'");
                    return Ok(cachedResponse);
                }
                
                var products = await _productService.SearchProductsAsync(keyword);
                var response = new { message = "Products searched successfully", data = products };
                
                // Lưu vào cache với thời gian ngắn hơn
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(1));
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with keyword {keyword}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);
                
                // Xóa cache khi có thay đổi dữ liệu
                ClearProductCache();
                
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                    new { message = "Product created successfully", data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                
                // Xóa cache khi có thay đổi dữ liệu
                ClearProductCache();
                _cache.Remove($"Product_{id}");
                
                return Ok(new { message = "Product updated successfully", data = product });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found for update");
                return NotFound(new { message = nfex.Message, errorCode = "PRODUCT_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                
                // Xóa cache khi có thay đổi dữ liệu
                ClearProductCache();
                _cache.Remove($"Product_{id}");
                
                return Ok(new { message = $"Product with ID {id} deleted successfully" });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found for delete");
                return NotFound(new { message = nfex.Message, errorCode = "PRODUCT_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/Product/with-image
        [HttpPost("with-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductWithImage([FromForm] string name, [FromForm] string description,
            [FromForm] decimal price, [FromForm] bool isAvailable, [FromForm] int productTypeId, 
            [FromForm] int productBrandId, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "No image uploaded" });

                var createProductDto = new CreateProductDto
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    IsAvailable = isAvailable,
                    ProductTypeId = productTypeId,
                    ProductBrandId = productBrandId
                };

                var product = await _productService.CreateProductWithImageAsync(createProductDto, image);
                
                // Xóa cache khi có thay đổi dữ liệu
                ClearProductCache();
                
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                    new { message = "Product created successfully with image", data = product });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Invalid argument for product creation");
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with image");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // PUT: api/Product/5/with-image
        [HttpPut("{id}/with-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductWithImage(int id, [FromForm] string name, 
            [FromForm] string description, [FromForm] decimal? price, [FromForm] bool? isAvailable, 
            [FromForm] int? productTypeId, [FromForm] int? productBrandId, IFormFile image)
        {
            try
            {
                var updateProductDto = new UpdateProductDto
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    IsAvailable = isAvailable,
                    ProductTypeId = productTypeId,
                    ProductBrandId = productBrandId
                };

                var product = await _productService.UpdateProductWithImageAsync(id, updateProductDto, image);
                
                // Xóa cache khi có thay đổi dữ liệu
                ClearProductCache();
                _cache.Remove($"Product_{id}");
                
                return Ok(new { message = "Product updated successfully with image", data = product });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found for update with image");
                return NotFound(new { message = nfex.Message, errorCode = "PRODUCT_NOT_FOUND" });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Invalid argument for product update");
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with image for id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        
        // Helper method to clear product cache
        private void ClearProductCache()
        {
            // Clear all product-related cache entries
            // Thực tế nên dùng pattern hoặc tag để quản lý cache hiệu quả hơn
            foreach (var key in _cache.GetKeys<string>())
            {
                if (key.StartsWith("Product_") || key.StartsWith("Products_"))
                {
                    _cache.Remove(key);
                }
            }
        }
    }
    
    // Extension method to get all keys from IMemoryCache
    public static class MemoryCacheExtensions
    {
        private static readonly Lazy<object> _keys = new Lazy<object>(() => { return new object(); });
        private static readonly Dictionary<object, HashSet<string>> _keysCollection = new Dictionary<object, HashSet<string>>();
 
        public static HashSet<string> GetKeys<T>(this IMemoryCache memoryCache)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            return keys;
        }
 
        public static void AddKey(this IMemoryCache memoryCache, string key)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            keys.Add(key);
        }
 
        public static void RemoveKey(this IMemoryCache memoryCache, string key)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            keys.Remove(key);
        }
        
        private static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dict[key] = value;
            }
            return value;
        }
    }
}

