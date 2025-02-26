using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Logging;
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

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<IEnumerable<ProductDTO>> GetByTypeAsync(int productTypeId)
        {
            var products = await _productRepository.GetByTypeAsync(productTypeId);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = await _productRepository.CreateProductAsync(createProductDto);
            return MapToDto(product);
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _productRepository.UpdateProductAsync(id, updateProductDto);
            return product != null ? MapToDto(product) : null;
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteProductAsync(id);
        }

        private ProductDTO MapToDto(Product product)
        {
            return new ProductDTO
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
