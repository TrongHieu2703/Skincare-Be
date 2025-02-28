using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
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
            var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetByTypeAsync(int productTypeId)
        {
            var products = await _productRepository.GetByTypeAsync(productTypeId);
            return products.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
        {
            var products = await _productRepository.SearchProductsAsync(keyword);
            return products.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            var products = await _productRepository.FilterProductsAsync(category, inStock, minPrice, maxPrice);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
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

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null)
                return null;

            existing.Name = updateProductDto.Name ?? existing.Name;
            existing.Description = updateProductDto.Description ?? existing.Description;
            existing.Price = updateProductDto.Price ?? existing.Price;
            existing.Image = updateProductDto.Image ?? existing.Image;
            existing.IsAvailable = updateProductDto.IsAvailable ?? existing.IsAvailable;
            existing.ProductTypeId = updateProductDto.ProductTypeId ?? existing.ProductTypeId;
            existing.ProductBrandId = updateProductDto.ProductBrandId ?? existing.ProductBrandId;

            var updated = await _productRepository.UpdateProductAsync(existing);
            return updated != null ? MapToDto(updated) : null;
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteProductAsync(id);
        }

        // So sánh sản phẩm: Lấy danh sách sản phẩm theo danh sách ID đưa vào.
        public async Task<IEnumerable<ProductDto>> CompareProductsAsync(CompareRequestDto compareRequestDto)
        {
            var result = new List<ProductDto>();
            foreach (var productId in compareRequestDto.ProductIds)
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product != null)
                    result.Add(MapToDto(product));
            }
            return result;
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
