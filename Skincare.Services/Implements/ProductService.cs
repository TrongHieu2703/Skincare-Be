using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using Skincare.Services.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Text.Json; // Added for JsonSerializer
using Microsoft.EntityFrameworkCore; // Added for CountAsync and Skip/Take

namespace Skincare.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        private readonly IFileService _fileService;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductSkinTypeRepository _productSkinTypeRepository;
        private readonly ICacheService _cacheService;
        // Thêm biến để kiểm soát số lượng request
        private static readonly Dictionary<string, DateTime> _lastUploadTime = new Dictionary<string, DateTime>();
        private static readonly SemaphoreSlim _uploadSemaphore = new SemaphoreSlim(5, 5); // Giới hạn 5 upload đồng thời

        public ProductService(
            IProductRepository productRepository, 
            ILogger<ProductService> logger,
            IFileService fileService,
            IInventoryRepository inventoryRepository,
            IProductSkinTypeRepository productSkinTypeRepository,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _logger = logger;
            _fileService = fileService;
            _inventoryRepository = inventoryRepository;
            _productSkinTypeRepository = productSkinTypeRepository;
            _cacheService = cacheService;
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var cacheKey = $"{CacheKeys.ProductList}:{pageNumber}:{pageSize}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    _logger.LogInformation($"Getting all products: page {pageNumber}, size {pageSize}");
                    var (products, totalCount) = await _productRepository.GetAllProductsWithPaginationAsync(pageNumber, pageSize);
                    _logger.LogInformation($"Retrieved {products.Count()} products from repository. Total count: {totalCount}");
                    
                    // Convert entities to DTOs with additional error checking
                    var dtos = new List<ProductDto>();
                    foreach (var product in products)
                    {
                        try
                        {
                            dtos.Add(MapToDto(product));
                        }
                        catch (Exception mapEx)
                        {
                            _logger.LogError(mapEx, $"Error mapping product {product.Id}: {mapEx.Message}");
                            // Continue to next product instead of failing the entire request
                        }
                    }

                    // Fix calculation issue - use decimal or double for math, cast to int at the end
                    decimal calculatedPages = (decimal)totalCount / pageSize;
                    int totalPages = (int)Math.Ceiling(calculatedPages);
                    
                    _logger.LogInformation($"Pagination calculation: {totalCount} items / {pageSize} per page = {calculatedPages} pages, rounded up to {totalPages} pages");
                    
                    return (dtos, totalPages, totalCount);
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProductsAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ProductDetail, id);
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var product = await _productRepository.GetProductByIdAsync(id);
                    if (product == null)
                    {
                        // Quăng NotFoundException nếu không tìm thấy
                        throw new NotFoundException($"Product with ID {id} not found.");
                    }
                    return MapToDto(product);
                }, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductByIdAsync for ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetByTypeAsync(int productTypeId)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ProductByCategory, productTypeId);
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var products = await _productRepository.GetProductsByTypeAsync(productTypeId);
                    return products.Select(MapToDto).ToList();
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetByTypeAsync for productTypeId {productTypeId}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.ProductSearch, keyword.ToLower());
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var products = await _productRepository.SearchProductsAsync(keyword);
                    return products.Select(MapToDto).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchProductsAsync for keyword: {keyword}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var cacheKey = $"products:filter:{category}:{inStock}:{minPrice}:{maxPrice}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var products = await _productRepository.FilterProductsAsync(category, inStock, minPrice, maxPrice);
                    return products.Select(MapToDto).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterProductsAsync");
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productRepository.CreateProductAsync(createProductDto);
                
                // Invalidate related caches
                await InvalidateProductCaches();
                
                return MapToDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProductAsync");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productRepository.UpdateProductAsync(id, updateProductDto);
                
                // Invalidate specific product cache and related caches
                await _cacheService.RemoveAsync(string.Format(CacheKeys.ProductDetail, id));
                await InvalidateProductCaches();
                
                return MapToDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateProductAsync for ID {id}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                await _productRepository.DeleteProductAsync(id);
                
                // Invalidate specific product cache and related caches
                await _cacheService.RemoveAsync(string.Format(CacheKeys.ProductDetail, id));
                await InvalidateProductCaches();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteProductAsync for ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> CompareProductsAsync(CompareRequestDto compareRequestDto)
        {
            try
            {
                var cacheKey = $"products:compare:{string.Join(":", compareRequestDto.ProductIds.OrderBy(x => x))}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var products = await _productRepository.CompareProductsAsync(compareRequestDto);
                    return products.Select(MapToDto).ToList();
                }, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompareProductsAsync");
                throw;
            }
        }

        public async Task<ProductDto> CreateProductWithImageAsync(CreateProductDto createProductDto, IFormFile image)
        {
            try
            {
                await CheckRateLimit("upload");
                
                using (await _uploadSemaphore.WaitAsync())
                {
                    var product = await _productRepository.CreateProductWithImageAsync(createProductDto, image);
                    
                    // Invalidate related caches
                    await InvalidateProductCaches();
                    
                    return MapToDto(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProductWithImageAsync");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductWithImageAsync(int id, UpdateProductDto updateProductDto, IFormFile image)
        {
            try
            {
                await CheckRateLimit("upload");
                
                using (await _uploadSemaphore.WaitAsync())
                {
                    var product = await _productRepository.UpdateProductWithImageAsync(id, updateProductDto, image);
                    
                    // Invalidate specific product cache and related caches
                    await _cacheService.RemoveAsync(string.Format(CacheKeys.ProductDetail, id));
                    await InvalidateProductCaches();
                    
                    return MapToDto(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateProductWithImageAsync for ID {id}");
                throw;
            }
        }

        private async Task DeleteProductImage(string imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await _fileService.DeleteFileAsync(imageUrl);
                    _logger.LogInformation($"Deleted product image: {imageUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product image: {imageUrl}");
                // Don't throw here as it's not critical for the main operation
            }
        }

        private async Task CheckRateLimit(string operation)
        {
            var key = $"rate_limit:{operation}";
            var now = DateTime.UtcNow;
            
            lock (_lastUploadTime)
            {
                if (_lastUploadTime.ContainsKey(key))
                {
                    var timeSinceLastUpload = now - _lastUploadTime[key];
                    if (timeSinceLastUpload.TotalSeconds < 1) // 1 second minimum between uploads
                    {
                        throw new InvalidOperationException($"Rate limit exceeded for {operation}. Please wait before trying again.");
                    }
                }
                _lastUploadTime[key] = now;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsBySkinTypeAsync(int skinTypeId)
        {
            try
            {
                var cacheKey = $"products:skintype:{skinTypeId}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var products = await _productRepository.GetProductsBySkinTypeAsync(skinTypeId);
                    return products.Select(MapToDto).ToList();
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinTypeAsync for skinTypeId {skinTypeId}");
                throw;
            }
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                ProductTypeId = product.ProductTypeId,
                ProductTypeName = product.ProductType?.Name,
                BranchId = product.BranchId,
                BranchName = product.Branch?.Name,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsWithFiltersAsync(
            int pageNumber, 
            int pageSize, 
            int? skinTypeId = null, 
            int? productTypeId = null, 
            int? branchId = null,
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            decimal? minRating = null, 
            decimal? maxRating = null, 
            string sortBy = null)
        {
            try
            {
                var cacheKey = $"products:filtered:{pageNumber}:{pageSize}:{skinTypeId}:{productTypeId}:{branchId}:{minPrice}:{maxPrice}:{minRating}:{maxRating}:{sortBy}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var query = _productRepository.GetProductsQuery();

                    // Apply filters
                    if (skinTypeId.HasValue)
                    {
                        query = query.Where(p => p.ProductSkinTypes.Any(pst => pst.SkinTypeId == skinTypeId.Value));
                    }

                    if (productTypeId.HasValue)
                    {
                        query = query.Where(p => p.ProductTypeId == productTypeId.Value);
                    }

                    if (branchId.HasValue)
                    {
                        query = query.Where(p => p.BranchId == branchId.Value);
                    }

                    if (minPrice.HasValue)
                    {
                        query = query.Where(p => p.Price >= minPrice.Value);
                    }

                    if (maxPrice.HasValue)
                    {
                        query = query.Where(p => p.Price <= maxPrice.Value);
                    }

                    if (minRating.HasValue)
                    {
                        query = query.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) >= minRating.Value);
                    }

                    if (maxRating.HasValue)
                    {
                        query = query.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) <= maxRating.Value);
                    }

                    // Apply sorting
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        query = ApplySorting(query, sortBy);
                    }

                    var totalCount = await _productRepository.GetCountAsync(query);
                    var products = await _productRepository.GetPaginatedAsync(query, pageNumber, pageSize);

                    var dtos = products.Select(MapToDto).ToList();
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    return (dtos, totalPages, totalCount);
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductsWithFiltersAsync");
                throw;
            }
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "created" => query.OrderBy(p => p.CreatedAt),
                "created_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsByTypeWithPaginationAsync(
            int productTypeId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                var cacheKey = $"products:type:{productTypeId}:{pageNumber}:{pageSize}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var (products, totalCount) = await _productRepository.GetProductsByTypeWithPaginationAsync(productTypeId, pageNumber, pageSize);
                    var dtos = products.Select(MapToDto).ToList();
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    
                    return (dtos, totalPages, totalCount);
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByTypeWithPaginationAsync for productTypeId {productTypeId}");
                throw;
            }
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsByBranchWithPaginationAsync(
            int branchId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                var cacheKey = $"products:branch:{branchId}:{pageNumber}:{pageSize}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var (products, totalCount) = await _productRepository.GetProductsByBranchWithPaginationAsync(branchId, pageNumber, pageSize);
                    var dtos = products.Select(MapToDto).ToList();
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    
                    return (dtos, totalPages, totalCount);
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByBranchWithPaginationAsync for branchId {branchId}");
                throw;
            }
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsBySkinTypeWithPaginationAsync(
            int skinTypeId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                var cacheKey = $"products:skintype:{skinTypeId}:{pageNumber}:{pageSize}";
                
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var (products, totalCount) = await _productRepository.GetProductsBySkinTypeWithPaginationAsync(skinTypeId, pageNumber, pageSize);
                    var dtos = products.Select(MapToDto).ToList();
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    
                    return (dtos, totalPages, totalCount);
                }, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinTypeWithPaginationAsync for skinTypeId {skinTypeId}");
                throw;
            }
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> AdvancedSearchAsync(AdvancedSearchDto searchDto)
        {
            try
            {
                _logger.LogInformation($"Advanced search with criteria: {JsonSerializer.Serialize(searchDto)}");

                // Build query based on search criteria
                var query = _productRepository.GetAllProductsQueryable();

                // Apply keyword search
                if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
                {
                    var keyword = searchDto.Keyword.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(keyword) || 
                                           p.Description.ToLower().Contains(keyword) ||
                                           p.Brand.ToLower().Contains(keyword));
                }

                // Apply category filter
                if (searchDto.CategoryIds != null && searchDto.CategoryIds.Any())
                {
                    query = query.Where(p => searchDto.CategoryIds.Contains(p.ProductTypeId));
                }

                // Apply brand filter
                if (searchDto.BrandIds != null && searchDto.BrandIds.Any())
                {
                    // Assuming BrandId is available in Product entity
                    // query = query.Where(p => searchDto.BrandIds.Contains(p.BrandId));
                }

                // Apply skin type filter
                if (searchDto.SkinTypeIds != null && searchDto.SkinTypeIds.Any())
                {
                    query = query.Where(p => p.ProductSkinTypes.Any(pst => searchDto.SkinTypeIds.Contains(pst.SkinTypeId)));
                }

                // Apply price range filter
                if (searchDto.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
                }

                if (searchDto.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
                }

                // Apply rating filter
                if (searchDto.MinRating.HasValue)
                {
                    query = query.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) >= searchDto.MinRating.Value);
                }

                // Apply stock filter
                if (searchDto.InStock.HasValue && searchDto.InStock.Value)
                {
                    query = query.Where(p => p.Inventories.Any(i => i.Quantity > 0));
                }

                // Apply organic filter
                if (searchDto.IsOrganic.HasValue)
                {
                    // Assuming IsOrganic property exists
                    // query = query.Where(p => p.IsOrganic == searchDto.IsOrganic.Value);
                }

                // Apply cruelty-free filter
                if (searchDto.IsCrueltyFree.HasValue)
                {
                    // Assuming IsCrueltyFree property exists
                    // query = query.Where(p => p.IsCrueltyFree == searchDto.IsCrueltyFree.Value);
                }

                // Apply size filter
                if (!string.IsNullOrWhiteSpace(searchDto.Size))
                {
                    // Assuming Size property exists
                    // query = query.Where(p => p.Size == searchDto.Size);
                }

                // Apply brand filter
                if (!string.IsNullOrWhiteSpace(searchDto.Brand))
                {
                    // Assuming Brand property exists
                    // query = query.Where(p => p.Brand == searchDto.Brand);
                }

                // Apply sorting
                query = ApplyAdvancedSorting(query, searchDto.SortBy, searchDto.SortOrder);

                // Get total count before pagination
                var totalItems = await query.CountAsync();

                // Apply pagination
                var products = await query
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var dtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    try
                    {
                        dtos.Add(MapToDto(product));
                    }
                    catch (Exception mapEx)
                    {
                        _logger.LogError(mapEx, $"Error mapping product {product.Id}: {mapEx.Message}");
                    }
                }

                // Calculate total pages
                decimal calculatedPages = (decimal)totalItems / searchDto.PageSize;
                int totalPages = (int)Math.Ceiling(calculatedPages);

                _logger.LogInformation($"Advanced search completed: {dtos.Count} products found, {totalPages} pages");

                return (dtos, totalPages, totalItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdvancedSearchAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private IQueryable<Product> ApplyAdvancedSorting(IQueryable<Product> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "rating" => isDescending 
                    ? query.OrderByDescending(p => p.Reviews.Average(r => r.Rating))
                    : query.OrderBy(p => p.Reviews.Average(r => r.Rating)),
                "created" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                "popularity" => isDescending 
                    ? query.OrderByDescending(p => p.Reviews.Count)
                    : query.OrderBy(p => p.Reviews.Count),
                _ => query.OrderBy(p => p.Name) // Default sorting
            };
        }

        private async Task InvalidateProductCaches()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern);
                _logger.LogInformation("Invalidated all product caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating product caches");
            }
        }
    }
}
