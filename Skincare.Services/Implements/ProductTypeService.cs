using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly IProductTypeRepository _productTypeRepository;
        private readonly ILogger<ProductTypeService> _logger;

        public ProductTypeService(
            IProductTypeRepository productTypeRepository,
            ILogger<ProductTypeService> logger)
        {
            _productTypeRepository = productTypeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductTypeDto>> GetAllProductTypesAsync()
        {
            try
            {
                var productTypes = await _productTypeRepository.GetAllAsync();
                return productTypes.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all product types");
                throw;
            }
        }

        public async Task<ProductTypeDto?> GetProductTypeByIdAsync(int id)
        {
            try
            {
                var productType = await _productTypeRepository.GetByIdAsync(id);
                if (productType == null)
                {
                    return null;
                }
                return MapToDto(productType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching product type with ID {id}");
                throw;
            }
        }

        private ProductTypeDto MapToDto(ProductType productType)
        {
            return new ProductTypeDto
            {
                Id = productType.Id,
                Name = productType.Name
            };
        }
    }
} 