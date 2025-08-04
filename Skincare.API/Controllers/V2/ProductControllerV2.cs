using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.DTOs.V2;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Services.Interfaces;
using Skincare.API.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.API.Controllers.V2
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiVersion("2.0")]
    public class ProductControllerV2 : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductControllerV2> _logger;

        public ProductControllerV2(IProductService productService, ILogger<ProductControllerV2> logger)
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
                
                // Convert to V2 DTOs
                var productsV2 = new List<ProductDtoV2>();
                foreach (var product in products)
                {
                    productsV2.Add(ConvertToV2(product));
                }

                var response = new
                {
                    data = productsV2,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalPages,
                        totalItems,
                        hasNextPage = pageNumber < totalPages,
                        hasPreviousPage = pageNumber > 1
                    },
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow,
                        sortBy,
                        includeInactive
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products V2");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                var productV2 = ConvertToV2(product);

                var response = new
                {
                    data = productV2,
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow,
                        cacheStatus = "hit"
                    }
                };

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message, version = "2.0" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product V2 with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int? skinTypeId = null,
            [FromQuery] int? productTypeId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isOrganic = null,
            [FromQuery] bool? isCrueltyFree = null,
            [FromQuery] string size = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetProductsWithFiltersAsync(
                    pageNumber, pageSize, skinTypeId, productTypeId, null, minPrice, maxPrice, null, null, "name");

                // Apply V2-specific filters
                var filteredProducts = new List<ProductDto>();
                foreach (var product in products)
                {
                    var productV2 = ConvertToV2(product);
                    
                    // Apply V2 filters
                    if (isOrganic.HasValue && productV2.IsOrganic != isOrganic.Value) continue;
                    if (isCrueltyFree.HasValue && productV2.IsCrueltyFree != isCrueltyFree.Value) continue;
                    if (!string.IsNullOrEmpty(size) && productV2.Size != size) continue;
                    
                    filteredProducts.Add(product);
                }

                var productsV2 = filteredProducts.ConvertAll(ConvertToV2);

                var response = new
                {
                    data = productsV2,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalPages,
                        totalItems: filteredProducts.Count,
                        hasNextPage = pageNumber < totalPages,
                        hasPreviousPage = pageNumber > 1
                    },
                    filters = new
                    {
                        keyword,
                        skinTypeId,
                        productTypeId,
                        minPrice,
                        maxPrice,
                        isOrganic,
                        isCrueltyFree,
                        size
                    },
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products V2");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDtoV2 createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data", errors = ModelState, version = "2.0" });

                // Convert V2 DTO to V1 DTO for service
                var createProductDtoV1 = new CreateProductDto
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    ProductTypeId = createProductDto.ProductTypeId,
                    Status = createProductDto.Status
                };

                var product = await _productService.CreateProductAsync(createProductDtoV1);
                var productV2 = ConvertToV2(product);

                var response = new
                {
                    message = "Product created successfully",
                    data = productV2,
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow
                    }
                };

                return CreatedAtAction(nameof(GetProductById), new { id = productV2.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product V2");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message, version = "2.0" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDtoV2 updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data", errors = ModelState, version = "2.0" });

                // Convert V2 DTO to V1 DTO for service
                var updateProductDtoV1 = new UpdateProductDto
                {
                    Name = updateProductDto.Name,
                    Description = updateProductDto.Description,
                    Price = updateProductDto.Price,
                    ProductTypeId = updateProductDto.ProductTypeId,
                    Status = updateProductDto.Status
                };

                var product = await _productService.UpdateProductAsync(id, updateProductDtoV1);
                var productV2 = ConvertToV2(product);

                var response = new
                {
                    message = "Product updated successfully",
                    data = productV2,
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow
                    }
                };

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message, version = "2.0" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product V2 with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message, version = "2.0" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);

                var response = new
                {
                    message = "Product deleted successfully",
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow
                    }
                };

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message, version = "2.0" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product V2 with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message, version = "2.0" });
            }
        }

        [HttpGet("organic")]
        public async Task<IActionResult> GetOrganicProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                
                // Filter organic products (this would be better implemented in the service layer)
                var organicProducts = new List<ProductDtoV2>();
                foreach (var product in products)
                {
                    var productV2 = ConvertToV2(product);
                    if (productV2.IsOrganic)
                    {
                        organicProducts.Add(productV2);
                    }
                }

                var response = new
                {
                    data = organicProducts,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalPages,
                        totalItems: organicProducts.Count,
                        hasNextPage = pageNumber < totalPages,
                        hasPreviousPage = pageNumber > 1
                    },
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow,
                        filter = "organic"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organic products V2");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("cruelty-free")]
        public async Task<IActionResult> GetCrueltyFreeProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalPages, totalItems) = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                
                // Filter cruelty-free products
                var crueltyFreeProducts = new List<ProductDtoV2>();
                foreach (var product in products)
                {
                    var productV2 = ConvertToV2(product);
                    if (productV2.IsCrueltyFree)
                    {
                        crueltyFreeProducts.Add(productV2);
                    }
                }

                var response = new
                {
                    data = crueltyFreeProducts,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalPages,
                        totalItems: crueltyFreeProducts.Count,
                        hasNextPage = pageNumber < totalPages,
                        hasPreviousPage = pageNumber > 1
                    },
                    meta = new
                    {
                        version = "2.0",
                        timestamp = DateTime.UtcNow,
                        filter = "cruelty-free"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cruelty-free products V2");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        private ProductDtoV2 ConvertToV2(ProductDto product)
        {
            return new ProductDtoV2
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                ProductTypeId = product.ProductTypeId,
                ProductTypeName = product.ProductTypeName,
                BranchId = product.BranchId,
                BranchName = product.BranchName,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                
                // V2 specific fields (mock data for demonstration)
                Ingredients = new List<string> { "Water", "Glycerin", "Hyaluronic Acid" },
                Benefits = new List<string> { "Hydrating", "Anti-aging", "Brightening" },
                UsageInstructions = "Apply twice daily to clean skin",
                SkinTypeRecommendation = "All skin types",
                IsOrganic = product.Id % 2 == 0, // Mock: even IDs are organic
                IsCrueltyFree = product.Id % 3 == 0, // Mock: IDs divisible by 3 are cruelty-free
                Size = "50ml",
                StockQuantity = 100,
                AverageRating = 4.5m,
                ReviewsCount = 25,
                Tags = new List<string> { "skincare", "moisturizer", "anti-aging" },
                Images = new List<string> { product.ImageUrl },
                Metadata = new Dictionary<string, object>
                {
                    { "brand", "SkincareBrand" },
                    { "origin", "Korea" },
                    { "expiry_months", 24 }
                }
            };
        }
    }
} 