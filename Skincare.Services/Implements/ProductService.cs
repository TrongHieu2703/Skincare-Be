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

namespace Skincare.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
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
