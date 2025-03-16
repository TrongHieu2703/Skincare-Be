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
        private readonly GoogleDriveService _googleDriveService;
        // Thêm biến để kiểm soát số lượng request
        private static readonly Dictionary<string, DateTime> _lastUploadTime = new Dictionary<string, DateTime>();
        private static readonly SemaphoreSlim _uploadSemaphore = new SemaphoreSlim(5, 5); // Giới hạn 5 upload đồng thời

        public ProductService(
            IProductRepository productRepository, 
            ILogger<ProductService> logger,
            GoogleDriveService googleDriveService)
        {
            _productRepository = productRepository;
            _logger = logger;
            _googleDriveService = googleDriveService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize);
                return products.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProductsAsync");
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
                    ProductBrandId = createProductDto.ProductBrandId
                };

                var created = await _productRepository.CreateProductAsync(newProduct);
                return MapToDto(created);
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

                existing.Name = updateProductDto.Name ?? existing.Name;
                existing.Description = updateProductDto.Description ?? existing.Description;
                existing.Price = updateProductDto.Price ?? existing.Price;
                existing.Image = updateProductDto.Image ?? existing.Image;
                existing.IsAvailable = updateProductDto.IsAvailable ?? existing.IsAvailable;
                existing.ProductTypeId = updateProductDto.ProductTypeId ?? existing.ProductTypeId;
                existing.ProductBrandId = updateProductDto.ProductBrandId ?? existing.ProductBrandId;

                var updated = await _productRepository.UpdateProductAsync(existing);
                return MapToDto(updated);
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
                // Validate image
                if (image == null || image.Length == 0)
                {
                    throw new ArgumentException("Không có file ảnh được tải lên");
                }

                // Kiểm tra rate limit
                await CheckRateLimit("create");

                // Allowed image types
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(image.ContentType.ToLower()))
                {
                    throw new ArgumentException("Loại file không hợp lệ. Chỉ chấp nhận JPG, PNG và GIF.");
                }

                // Kiểm tra kích thước tối đa (8MB)
                if (image.Length > 8 * 1024 * 1024)
                {
                    throw new ArgumentException("Kích thước file quá lớn. Tối đa 8MB.");
                }

                // Sử dụng semaphore để giới hạn số lượng upload đồng thời
                await _uploadSemaphore.WaitAsync();
                try
                {
                    // Upload to Google Drive
                    var (fileId, _, _, fileUrl) = await _googleDriveService.UploadFile(image);
                    
                    // Create product with image URL
                    var newProduct = new Product
                    {
                        Name = createProductDto.Name,
                        Description = createProductDto.Description,
                        Price = createProductDto.Price,
                        Image = fileUrl, // Set image URL from Google Drive
                        IsAvailable = createProductDto.IsAvailable,
                        ProductTypeId = createProductDto.ProductTypeId,
                        ProductBrandId = createProductDto.ProductBrandId
                    };

                    var created = await _productRepository.CreateProductAsync(newProduct);
                    return MapToDto(created);
                }
                finally
                {
                    _uploadSemaphore.Release();
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
                // Get existing product
                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                {
                    throw new NotFoundException($"Không tìm thấy sản phẩm với ID {id}");
                }

                // If we have a new image
                if (image != null && image.Length > 0)
                {
                    // Kiểm tra rate limit
                    await CheckRateLimit("update");

                    // Allowed image types
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(image.ContentType.ToLower()))
                    {
                        throw new ArgumentException("Loại file không hợp lệ. Chỉ chấp nhận JPG, PNG và GIF.");
                    }

                    // Kiểm tra kích thước tối đa (8MB)
                    if (image.Length > 8 * 1024 * 1024)
                    {
                        throw new ArgumentException("Kích thước file quá lớn. Tối đa 8MB.");
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existing.Image) && 
                        (existing.Image.Contains("drive.google.com") || 
                         existing.Image.Contains("thumbnail?id=") || 
                         existing.Image.Contains("uc?id=")))
                    {
                        await DeleteProductImage(existing.Image);
                    }

                    // Sử dụng semaphore để giới hạn số lượng upload đồng thời
                    await _uploadSemaphore.WaitAsync();
                    try
                    {
                        // Upload new image
                        var (fileId, _, _, fileUrl) = await _googleDriveService.UploadFile(image);
                        existing.Image = fileUrl;
                    }
                    finally
                    {
                        _uploadSemaphore.Release();
                    }
                }

                // Update other fields
                existing.Name = updateProductDto.Name ?? existing.Name;
                existing.Description = updateProductDto.Description ?? existing.Description;
                existing.Price = updateProductDto.Price ?? existing.Price;
                existing.IsAvailable = updateProductDto.IsAvailable ?? existing.IsAvailable;
                existing.ProductTypeId = updateProductDto.ProductTypeId ?? existing.ProductTypeId;
                existing.ProductBrandId = updateProductDto.ProductBrandId ?? existing.ProductBrandId;

                var updated = await _productRepository.UpdateProductAsync(existing);
                return MapToDto(updated);
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
                // Extract file ID from the URL
                var fileId = string.Empty;
                
                if (imageUrl.Contains("thumbnail?id="))
                {
                    fileId = imageUrl.Split("thumbnail?id=")[1].Split("&")[0];
                }
                else if (imageUrl.Contains("uc?id="))
                {
                    fileId = imageUrl.Split("uc?id=")[1].Split("&")[0];
                }
                else if (imageUrl.Contains("/d/"))
                {
                    fileId = imageUrl.Split("/d/")[1].Split("/")[0];
                }

                if (!string.IsNullOrEmpty(fileId))
                {
                    await _googleDriveService.DeleteFile(fileId);
                    _logger.LogInformation($"Đã xóa ảnh cũ của sản phẩm: {fileId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete old product image: {ex.Message}");
                // Log but continue with product update
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

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Image = product.Image,
                IsAvailable = product.IsAvailable,
                ProductTypeName = product.ProductType?.Name,
                ProductBrandName = product.ProductBrand?.Name,
                SkinTypes = product.ProductSkinTypes?
                    .Where(pst => pst.SkinType != null)
                    .Select(pst => pst.SkinType.Name)
                    .ToList() ?? new List<string>()
            };
        }
    }
}
