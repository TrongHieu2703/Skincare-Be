using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class ProductSkinTypeRepository : IProductSkinTypeRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<ProductSkinTypeRepository> _logger;

        public ProductSkinTypeRepository(SWP391Context context, ILogger<ProductSkinTypeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductSkinType>> GetAllAsync()
        {
            try
            {
                return await _context.ProductSkinTypes
                    .Include(pst => pst.Product)
                    .Include(pst => pst.SkinType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all product skin types");
                throw;
            }
        }

        public async Task<ProductSkinType> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ProductSkinTypes
                    .Include(pst => pst.Product)
                    .Include(pst => pst.SkinType)
                    .FirstOrDefaultAsync(pst => pst.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting product skin type with ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductSkinType>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _context.ProductSkinTypes
                    .Include(pst => pst.SkinType)
                    .Where(pst => pst.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting product skin types for product ID {productId}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductSkinType>> GetBySkinTypeIdAsync(int skinTypeId)
        {
            try
            {
                return await _context.ProductSkinTypes
                    .Include(pst => pst.Product)
                    .Where(pst => pst.SkinTypeId == skinTypeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting product skin types for skin type ID {skinTypeId}");
                throw;
            }
        }

        public async Task<ProductSkinType> CreateAsync(ProductSkinType productSkinType)
        {
            try
            {
                _context.ProductSkinTypes.Add(productSkinType);
                await _context.SaveChangesAsync();
                return productSkinType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product skin type");
                throw;
            }
        }

        public async Task<ProductSkinType> UpdateAsync(ProductSkinType productSkinType)
        {
            try
            {
                _context.Entry(productSkinType).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return productSkinType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating product skin type with ID {productSkinType.Id}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var productSkinType = await _context.ProductSkinTypes.FindAsync(id);
                if (productSkinType != null)
                {
                    _context.ProductSkinTypes.Remove(productSkinType);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting product skin type with ID {id}");
                throw;
            }
        }

        public async Task DeleteByProductIdAsync(int productId)
        {
            try
            {
                var productSkinTypes = await _context.ProductSkinTypes
                    .Where(pst => pst.ProductId == productId)
                    .ToListAsync();

                if (productSkinTypes.Any())
                {
                    _context.ProductSkinTypes.RemoveRange(productSkinTypes);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting product skin types for product ID {productId}");
                throw;
            }
        }
    }
} 