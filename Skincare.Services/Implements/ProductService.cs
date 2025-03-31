using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Skincare.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        private readonly IFileService _fileService;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductSkinTypeRepository _productSkinTypeRepository;
        // Thêm biến để kiểm soát số lượng request
        private static readonly Dictionary<string, DateTime> _lastUploadTime = new Dictionary<string, DateTime>();
        private static readonly SemaphoreSlim _uploadSemaphore = new SemaphoreSlim(5, 5); // Giới hạn 5 upload đồng thời

        public ProductService(
            IProductRepository productRepository, 
            ILogger<ProductService> logger,
            IFileService fileService,
            IInventoryRepository inventoryRepository,
            IProductSkinTypeRepository productSkinTypeRepository)
        {
            _productRepository = productRepository;
            _logger = logger;
            _fileService = fileService;
            _inventoryRepository = inventoryRepository;
            _productSkinTypeRepository = productSkinTypeRepository;
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            try
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
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    // Quăng NotFoundException nếu không tìm thấy
                    throw new NotFoundException($"Product with ID {id} not found.");
                }
                return MapToDto(product);
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
                var products = await _productRepository.GetByTypeAsync(productTypeId);
                // Nếu không tìm thấy sản phẩm, có thể trả về danh sách rỗng
                return products.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetByTypeAsync for type {productTypeId}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
        {
            try
            {
                var products = await _productRepository.SearchProductsAsync(keyword);
                return products.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchProductsAsync with keyword {keyword}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var products = await _productRepository.FilterProductsAsync(category, inStock, minPrice, maxPrice);
                return products.Select(MapToDto).ToList();
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
                var newProduct = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Image = createProductDto.Image,
                    IsAvailable = createProductDto.IsAvailable,
                    ProductTypeId = createProductDto.ProductTypeId,
                    ProductBrandId = createProductDto.ProductBrandId,
                    Quantity = createProductDto.Quantity,
                    Stock = createProductDto.Stock ?? createProductDto.Quantity
                };

                // Create the product first
                var createdProduct = await _productRepository.CreateProductAsync(newProduct);
                
                // If skin type IDs are provided, create product-skin type relationships
                if (createProductDto.SkinTypeIds != null && createProductDto.SkinTypeIds.Any())
                {
                    foreach (var skinTypeId in createProductDto.SkinTypeIds)
                    {
                        var productSkinType = new ProductSkinType
                        {
                            ProductId = createdProduct.Id,
                            SkinTypeId = skinTypeId
                        };
                        
                        await _productSkinTypeRepository.CreateAsync(productSkinType);
                    }
                }
                
                // Fetch the complete product with relations for returning
                var completeProduct = await _productRepository.GetProductByIdAsync(createdProduct.Id);
                return MapToDto(completeProduct);
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
                // Kiểm tra sản phẩm có tồn tại không
                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                {
                    throw new NotFoundException($"Product with ID {id} not found for update.");
                }

                // Log trước khi cập nhật
                _logger.LogInformation($"Updating product {id} - Before update: Name={existing.Name}, Price={existing.Price}, IsAvailable={existing.IsAvailable}, Stock={existing.Stock}, Quantity={existing.Quantity}");
                
                // Validate stock can't be greater than quantity
                int newStock = updateProductDto.Stock ?? existing.Stock ?? 0;
                int newQuantity = updateProductDto.Quantity ?? existing.Quantity ?? 0;
                
                if (newStock > newQuantity)
                {
                    _logger.LogWarning($"Attempted to set stock ({newStock}) greater than quantity ({newQuantity}) for product {id}");
                    throw new InvalidOperationException("Stock cannot be greater than quantity");
                }
                
                // If stock is 0, product should not be available
                bool newIsAvailable = updateProductDto.IsAvailable ?? existing.IsAvailable;
                if (newStock <= 0)
                {
                    newIsAvailable = false;
                    _logger.LogInformation($"Setting isAvailable to false for product {id} because stock is {newStock}");
                }
                else if (newStock > 0 && !newIsAvailable)
                {
                    // If stock is greater than 0, product should be available
                    newIsAvailable = true;
                    _logger.LogInformation($"Setting isAvailable to true for product {id} because stock is {newStock} > 0");
                }
                
                // Ghi lại các giá trị đang được cập nhật
                if (updateProductDto.Name != null) _logger.LogInformation($"Updating Name from '{existing.Name}' to '{updateProductDto.Name}'");
                if (updateProductDto.Description != null) _logger.LogInformation($"Updating Description");
                if (updateProductDto.Price.HasValue) _logger.LogInformation($"Updating Price from {existing.Price} to {updateProductDto.Price}");
                if (updateProductDto.Image != null) _logger.LogInformation($"Updating Image from '{existing.Image}' to '{updateProductDto.Image}'");
                _logger.LogInformation($"Updating IsAvailable from {existing.IsAvailable} to {newIsAvailable}");
                if (updateProductDto.ProductTypeId.HasValue) _logger.LogInformation($"Updating ProductTypeId from {existing.ProductTypeId} to {updateProductDto.ProductTypeId}");
                if (updateProductDto.ProductBrandId.HasValue) _logger.LogInformation($"Updating ProductBrandId from {existing.ProductBrandId} to {updateProductDto.ProductBrandId}");
                if (updateProductDto.Quantity.HasValue) _logger.LogInformation($"Updating Quantity from {existing.Quantity} to {updateProductDto.Quantity}");
                if (updateProductDto.Stock.HasValue) _logger.LogInformation($"Updating Stock from {existing.Stock} to {updateProductDto.Stock}");

                existing.Name = updateProductDto.Name ?? existing.Name;
                existing.Description = updateProductDto.Description ?? existing.Description;
                existing.Price = updateProductDto.Price ?? existing.Price;
                // Only update image if a new one is explicitly provided
                if (updateProductDto.Image != null)
                {
                    existing.Image = updateProductDto.Image;
                }
                existing.IsAvailable = newIsAvailable;
                existing.ProductTypeId = updateProductDto.ProductTypeId ?? existing.ProductTypeId;
                existing.ProductBrandId = updateProductDto.ProductBrandId ?? existing.ProductBrandId;
                existing.Quantity = newQuantity;
                existing.Stock = newStock;

                var updated = await _productRepository.UpdateProductAsync(existing);
                
                // Log sau khi cập nhật
                _logger.LogInformation($"Product {id} updated - After update: Name={updated.Name}, Price={updated.Price}, IsAvailable={updated.IsAvailable}, Stock={updated.Stock}, Quantity={updated.Quantity}");
                
                // Update skin types if provided
                if (updateProductDto.SkinTypeIds != null)
                {
                    // Get current skin types
                    var currentSkinTypes = await _productSkinTypeRepository.GetByProductIdAsync(id);
                    
                    // Delete all existing associations
                    foreach (var pst in currentSkinTypes)
                    {
                        await _productSkinTypeRepository.DeleteAsync(pst.Id);
                    }
                    
                    // Create new ones
                    foreach (var skinTypeId in updateProductDto.SkinTypeIds)
                    {
                        var productSkinType = new ProductSkinType
                        {
                            ProductId = id,
                            SkinTypeId = skinTypeId
                        };
                        
                        await _productSkinTypeRepository.CreateAsync(productSkinType);
                    }
                    
                    _logger.LogInformation($"Updated skin types for product {id}");
                }
                
                // Fetch the complete updated product with relations
                var completeProduct = await _productRepository.GetProductByIdAsync(id);
                return MapToDto(completeProduct);
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
                // Kiểm tra sản phẩm có tồn tại không
                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                {
                    throw new NotFoundException($"Product with ID {id} not found for delete.");
                }

                await _productRepository.DeleteProductAsync(id);
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
                var result = new List<ProductDto>();
                foreach (var productId in compareRequestDto.ProductIds)
                {
                    var product = await _productRepository.GetProductByIdAsync(productId);
                    if (product != null)
                        result.Add(MapToDto(product));
                    // Nếu muốn, bạn có thể quăng NotFoundException nếu 1 productId không tồn tại
                }
                return result;
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
                var newProduct = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    IsAvailable = createProductDto.IsAvailable,
                    ProductTypeId = createProductDto.ProductTypeId,
                    ProductBrandId = createProductDto.ProductBrandId,
                    Quantity = createProductDto.Quantity,
                    Stock = createProductDto.Stock ?? createProductDto.Quantity
                };

                // Handle image upload
                if (image != null && image.Length > 0)
                {
                    string imageUrl = await _fileService.SaveFileAsync(image, "product-images");
                    newProduct.Image = imageUrl;
                }

                var createdProduct = await _productRepository.CreateProductAsync(newProduct);
                
                // Add skin type associations if provided
                if (createProductDto.SkinTypeIds != null && createProductDto.SkinTypeIds.Any())
                {
                    foreach (var skinTypeId in createProductDto.SkinTypeIds)
                    {
                        var productSkinType = new ProductSkinType
                        {
                            ProductId = createdProduct.Id,
                            SkinTypeId = skinTypeId
                        };
                        
                        await _productSkinTypeRepository.CreateAsync(productSkinType);
                    }
                }
                
                // Fetch the complete product with all relationships loaded
                var completeProduct = await _productRepository.GetProductByIdAsync(createdProduct.Id);
                return MapToDto(completeProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with image");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductWithImageAsync(int id, UpdateProductDto updateProductDto, IFormFile image)
        {
            try
            {
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    throw new NotFoundException($"Product with ID {id} not found.");
                }

                _logger.LogInformation($"Updating product {id}. Current image path: {existingProduct.Image}");

                // Update basic information EXCEPT image
                existingProduct.Name = updateProductDto.Name ?? existingProduct.Name;
                existingProduct.Description = updateProductDto.Description ?? existingProduct.Description;
                existingProduct.Price = updateProductDto.Price ?? existingProduct.Price;
                existingProduct.IsAvailable = updateProductDto.IsAvailable ?? existingProduct.IsAvailable;
                existingProduct.ProductTypeId = updateProductDto.ProductTypeId ?? existingProduct.ProductTypeId;
                existingProduct.ProductBrandId = updateProductDto.ProductBrandId ?? existingProduct.ProductBrandId;
                
                // Add quantity and stock updates
                if (updateProductDto.Quantity.HasValue)
                {
                    existingProduct.Quantity = updateProductDto.Quantity;
                    _logger.LogInformation($"Updating quantity to {updateProductDto.Quantity} for product {id}");
                }
                
                if (updateProductDto.Stock.HasValue)
                {
                    existingProduct.Stock = updateProductDto.Stock;
                    _logger.LogInformation($"Updating stock to {updateProductDto.Stock} for product {id}");
                }

                // Handle image update
                if (image != null && image.Length > 0)
                {
                    _logger.LogInformation($"Processing new image upload for product {id}. File name: {image.FileName}");
                    
                    // Save new image first
                    string newImageUrl = await _fileService.SaveFileAsync(image, "product-images");
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        _logger.LogInformation($"New image saved successfully: {newImageUrl}");
                        
                        // Store old image path for deletion
                        string oldImagePath = existingProduct.Image;
                        _logger.LogInformation($"Old image path: {oldImagePath}");
                        
                        // Update product with new image path
                        existingProduct.Image = newImageUrl;
                        _logger.LogInformation($"Updated product image path to: {existingProduct.Image}");

                        // Delete old image after successful save and path update
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            _logger.LogInformation($"Attempting to delete old image: {oldImagePath}");
                            if (_fileService.DeleteFile(oldImagePath.TrimStart('/')))
                            {
                                _logger.LogInformation($"Successfully deleted old image: {oldImagePath}");
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to delete old image: {oldImagePath}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to save new image. Keeping existing image path.");
                    }
                }
                else
                {
                    _logger.LogInformation($"No new image file provided for product {id}. Keeping existing image path: {existingProduct.Image}");
                }

                // Update the product in database
                var updatedProduct = await _productRepository.UpdateProductAsync(existingProduct);
                _logger.LogInformation($"Product {id} updated in database with final image path: {updatedProduct.Image}");
                
                // Update skin types if provided
                if (updateProductDto.SkinTypeIds != null)
                {
                    // Get current skin types
                    var currentSkinTypes = await _productSkinTypeRepository.GetByProductIdAsync(id);
                    
                    // Delete all existing associations
                    foreach (var pst in currentSkinTypes)
                    {
                        await _productSkinTypeRepository.DeleteAsync(pst.Id);
                    }
                    
                    // Create new ones
                    foreach (var skinTypeId in updateProductDto.SkinTypeIds)
                    {
                        var productSkinType = new ProductSkinType
                        {
                            ProductId = id,
                            SkinTypeId = skinTypeId
                        };
                        
                        await _productSkinTypeRepository.CreateAsync(productSkinType);
                    }
                }
                
                // Fetch the complete updated product with all relations
                var completeProduct = await _productRepository.GetProductByIdAsync(id);
                return MapToDto(completeProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID {id}: {ex.Message}");
                throw;
            }
        }

        private async Task DeleteProductImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return;
                }
                
                // Handle based on URL pattern
                if (imageUrl.StartsWith("/product-images/"))
                {
                    // Local storage image
                    _fileService.DeleteFile(imageUrl);
                    _logger.LogInformation($"Deleted local image: {imageUrl}");
                }
                else if (imageUrl.Contains("drive.google.com"))
                {
                    // Skip for now to avoid dependency on GoogleDriveService
                    _logger.LogInformation($"Skipped deletion of Google Drive image: {imageUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product image: {ex.Message}");
                // Don't throw here to avoid disrupting the main flow
            }
        }

        // Phương thức kiểm tra rate limit
        private async Task CheckRateLimit(string operation)
        {
            string key = $"{operation}";
            
            lock (_lastUploadTime)
            {
                if (_lastUploadTime.ContainsKey(key))
                {
                    // Kiểm tra thời gian giữa các lần upload (ít nhất 1 giây)
                    TimeSpan timeSinceLastUpload = DateTime.UtcNow - _lastUploadTime[key];
                    if (timeSinceLastUpload.TotalSeconds < 1)
                    {
                        throw new InvalidOperationException("Thao tác quá nhanh. Vui lòng thử lại sau.");
                    }
                    _lastUploadTime[key] = DateTime.UtcNow;
                }
                else
                {
                    _lastUploadTime[key] = DateTime.UtcNow;
                }
            }
            
            // Thêm delay nhỏ để đảm bảo không có quá nhiều request cùng lúc
            await Task.Delay(100);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsBySkinTypeAsync(int skinTypeId)
        {
            try
            {
                var products = await _productRepository.GetProductsBySkinTypeAsync(skinTypeId);
                return products.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinTypeAsync for skin type {skinTypeId}");
                throw;
            }
        }

        private ProductDto MapToDto(Product product)
        {
            if (product == null)
            {
                _logger.LogWarning("Attempted to map null product to DTO");
                throw new ArgumentNullException(nameof(product));
            }
            
            try
            {
                string imageUrl = product.Image;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // Đảm bảo đường dẫn ảnh bắt đầu bằng "/"
                    if (!imageUrl.StartsWith("/"))
                    {
                        imageUrl = "/" + imageUrl;
                    }
                }

                // Calculate average rating and reviews count
                decimal? averageRating = null;
                int reviewsCount = 0;
                
                if (product.Reviews != null && product.Reviews.Any())
                {
                    reviewsCount = product.Reviews.Count;
                    var validRatings = product.Reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value);
                    
                    if (validRatings.Any())
                    {
                        averageRating = Math.Round((decimal)validRatings.Average(), 1);
                    }
                }

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name ?? "",
                    Description = product.Description ?? "",
                    Price = product.Price,
                    Image = imageUrl ?? "",
                    IsAvailable = product.IsAvailable,
                    ProductTypeId = product.ProductTypeId,
                    ProductTypeName = product.ProductType?.Name ?? "Unknown Type",
                    ProductBrandId = product.ProductBrandId,
                    ProductBrandName = product.ProductBrand?.Name ?? "Unknown Brand",
                    Quantity = product.Quantity,
                    Stock = product.Stock,
                    InventoryId = product.Inventory?.Id,
                    BranchId = product.Inventory?.BranchId,
                    SkinTypes = product.ProductSkinTypes?
                        .Where(pst => pst.SkinType != null)
                        .Select(pst => pst.SkinType.Name)
                        .ToList() ?? new List<string>(),
                    SkinTypeIds = product.ProductSkinTypes?
                        .Select(pst => pst.SkinTypeId)
                        .ToList() ?? new List<int>(),
                    AverageRating = averageRating,
                    ReviewsCount = reviewsCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error mapping product {product.Id} to DTO: {ex.Message}");
                throw;
            }
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
                _logger.LogInformation($"Filtering products with: skinTypeId={skinTypeId}, productTypeId={productTypeId}, " +
                    $"branchId={branchId}, priceRange={minPrice}-{maxPrice}, ratingRange={minRating}-{maxRating}, sortBy={sortBy}");
                
                // Get initial query from repository
                var query = await _productRepository.GetFilteredProductsQueryAsync(
                    skinTypeId, productTypeId, branchId, minPrice, maxPrice);
                
                 // Kiểm tra nếu sắp xếp theo rating
                bool isSortingByRating = !string.IsNullOrEmpty(sortBy) && 
                    (sortBy.ToLower() == "rating_asc" || sortBy.ToLower() == "rating_desc");
                
                string nonRatingSortBy = isSortingByRating ? null : sortBy;
                
                // Apply sorting nếu không phải sắp xếp theo rating
                if (!string.IsNullOrEmpty(nonRatingSortBy))
                {
                    query = ApplySorting(query, nonRatingSortBy);
                }
                
                // Get total count for pagination
                int totalCount = await _productRepository.GetCountFromQueryAsync(query);
                
                // Apply pagination
                var paginatedProducts = await _productRepository.GetPaginatedProductsFromQueryAsync(
                    query, pageNumber, pageSize);
                
                // Map to DTOs
                var productDtos = paginatedProducts.Select(p => MapToDto(p)).ToList();
                
                // Declare pagination variables
                decimal calculatedPages;
                int totalPages;
                
                // Xử lý sắp xếp theo rating sau khi chuyển đổi thành DTO
                if (isSortingByRating)
                {
                    if (sortBy.ToLower() == "rating_asc")
                    {
                        productDtos = productDtos
                            .OrderBy(p => p.AverageRating ?? 0)
                            .ToList();
                    }
                    else // rating_desc
                    {
                        productDtos = productDtos
                            .OrderByDescending(p => p.AverageRating ?? 0)
                            .ToList();
                    }
                }
                
                // Filter by rating if specified (this is done after mapping since rating is calculated in the DTO)
                if (minRating.HasValue || maxRating.HasValue)
                {
                    _logger.LogInformation($"Filtering by rating range: {minRating} - {maxRating}");
                    productDtos = productDtos
                        .Where(p => (!minRating.HasValue || (p.AverageRating.HasValue && p.AverageRating >= minRating)) &&
                                   (!maxRating.HasValue || (p.AverageRating.HasValue && p.AverageRating <= maxRating)))
                        .ToList();
                    
                    // Recalculate pagination info for filtered results
                    totalCount = productDtos.Count;
                    calculatedPages = (decimal)totalCount / pageSize;
                    totalPages = (int)Math.Ceiling(calculatedPages);
                    
                    // Apply pagination again after rating filter
                    productDtos = productDtos
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    // Calculate pagination info for non-rating filtered results
                    calculatedPages = (decimal)totalCount / pageSize;
                    totalPages = (int)Math.Ceiling(calculatedPages);
                }
                
                _logger.LogInformation($"Retrieved {productDtos.Count} filtered products. " +
                    $"Total: {totalCount}, Pages: {totalPages}");
                
                return (productDtos, totalPages, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductsWithFiltersAsync");
                throw;
            }
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy)
        {
            switch (sortBy.ToLower())
            {
                case "price_asc":
                    return query.OrderBy(p => p.Price);
                case "price_desc":
                    return query.OrderByDescending(p => p.Price);
                case "name_asc":
                    return query.OrderBy(p => p.Name);
                case "name_desc":
                    return query.OrderByDescending(p => p.Name);
                default:
                    return query;
            }
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsByTypeWithPaginationAsync(
            int productTypeId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                _logger.LogInformation($"Getting products by type {productTypeId} with pagination: page {pageNumber}, size {pageSize}");
                
                var (products, totalCount) = await _productRepository.GetProductsByTypeWithPaginationAsync(
                    productTypeId, pageNumber, pageSize);
                
                // Fix method group conversion
                var productDtos = products.Select(p => MapToDto(p)).ToList();
                
                decimal calculatedPages = (decimal)totalCount / pageSize;
                int totalPages = (int)Math.Ceiling(calculatedPages);
                
                _logger.LogInformation($"Retrieved {productDtos.Count} products by type {productTypeId}. " +
                    $"Total: {totalCount}, Pages: {totalPages}");
                
                return (productDtos, totalPages, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByTypeWithPaginationAsync for type {productTypeId}");
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
                _logger.LogInformation($"Getting products by branch {branchId} with pagination: page {pageNumber}, size {pageSize}");
                
                var (products, totalCount) = await _productRepository.GetProductsByBranchWithPaginationAsync(
                    branchId, pageNumber, pageSize);
                
                // Fix method group conversion
                var productDtos = products.Select(p => MapToDto(p)).ToList();
                
                decimal calculatedPages = (decimal)totalCount / pageSize;
                int totalPages = (int)Math.Ceiling(calculatedPages);
                
                _logger.LogInformation($"Retrieved {productDtos.Count} products by branch {branchId}. " +
                    $"Total: {totalCount}, Pages: {totalPages}");
                
                return (productDtos, totalPages, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByBranchWithPaginationAsync for branch {branchId}");
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
                _logger.LogInformation($"Getting products by skin type {skinTypeId} with pagination: page {pageNumber}, size {pageSize}");
                
                var (products, totalCount) = await _productRepository.GetProductsBySkinTypeWithPaginationAsync(
                    skinTypeId, pageNumber, pageSize);
                
                // Fix method group conversion
                var productDtos = products.Select(p => MapToDto(p)).ToList();
                
                decimal calculatedPages = (decimal)totalCount / pageSize;
                int totalPages = (int)Math.Ceiling(calculatedPages);
                
                _logger.LogInformation($"Retrieved {productDtos.Count} products by skin type {skinTypeId}. " +
                    $"Total: {totalCount}, Pages: {totalPages}");
                
                return (productDtos, totalPages, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinTypeWithPaginationAsync for skin type {skinTypeId}");
                throw;
            }
        }
    }
}
