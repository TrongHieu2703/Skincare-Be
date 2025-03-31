using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Skincare.Repositories.Implements
{
    public class ProductRepository : IProductRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(SWP391Context context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllProductsWithPaginationAsync(int pageNumber, int pageSize)
        {
            try
            {
                // Count without any conditions to verify the actual total count
                var rawCount = await _context.Products.CountAsync();
                _logger.LogInformation($"Raw products count (without filters): {rawCount}");
                
                var totalCount = await _context.Products.CountAsync();
                _logger.LogInformation($"Total products count used for pagination: {totalCount}");
                
                var products = await _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductSkinTypes)
                    .Include(p => p.Reviews)
                    .OrderBy(p => p.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {products.Count} products for page {pageNumber} (out of {totalCount} total)");
                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProductsWithPaginationAsync");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                .Include(p => p.Reviews)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                    .ThenInclude(pst => pst.SkinType)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                .Include(p => p.Reviews)
                .Where(p => p.ProductTypeId == productTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                    .ThenInclude(pst => pst.SkinType)
                .Include(p => p.Reviews)
                .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                    .ThenInclude(pst => pst.SkinType)
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.ProductType.Name == category);

            if (inStock.HasValue)
                query = query.Where(p => p.Inventories.Sum(i => i.Quantity) > 0 == inStock.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null)
                return null;

            _logger.LogInformation($"Updating product {product.Id}");
            _logger.LogInformation($"Current values: Name={existing.Name}, Price={existing.Price}, IsAvailable={existing.IsAvailable}, Stock={existing.Stock}, Quantity={existing.Quantity}");
            _logger.LogInformation($"New values: Name={product.Name}, Price={product.Price}, IsAvailable={product.IsAvailable}, Stock={product.Stock}, Quantity={product.Quantity}");
            
            // Log if stock is 0 and product is being set as unavailable
            if (product.Stock <= 0 && product.IsAvailable == false)
            {
                _logger.LogInformation($"Product {product.Id} is being set as unavailable because stock is {product.Stock}");
            }
            else if (product.Stock > 0 && product.IsAvailable == true)
            {
                _logger.LogInformation($"Product {product.Id} is being set as available because stock is {product.Stock} > 0");
            }

            // Detach the existing entity to avoid tracking conflicts
            _context.Entry(existing).State = EntityState.Detached;
            
            // Attach the updated entity and mark it as modified
            _context.Products.Attach(product);
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated product {product.Id}");
                
                // Verify changes were saved by fetching the product again
                var updated = await _context.Products.FindAsync(product.Id);
                _logger.LogInformation($"Verification - Updated product: Name={updated.Name}, Price={updated.Price}, IsAvailable={updated.IsAvailable}, Stock={updated.Stock}, Quantity={updated.Quantity}");
                
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {product.Id}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                // Tìm sản phẩm theo id
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    // 1. Xoá các bản ghi ProductSkinType liên quan đến sản phẩm này
                    var relatedSkinTypes = _context.ProductSkinTypes.Where(pst => pst.ProductId == id);
                    if (relatedSkinTypes.Any())
                    {
                        _context.ProductSkinTypes.RemoveRange(relatedSkinTypes);
                        // Ghi log nếu cần
                    }

                    // 2. Xoá các bản ghi Inventory liên quan đến sản phẩm này
                    var relatedInventories = _context.Inventories.Where(inv => inv.ProductId == id);
                    if (relatedInventories.Any())
                    {
                        _context.Inventories.RemoveRange(relatedInventories);
                        // Ghi log nếu cần
                    }
                    
                    // Nếu còn các bảng khác phụ thuộc đến Product, cần xử lý tương tự.

                    // 3. Xoá sản phẩm
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log lỗi và ném lại exception
                throw new Exception($"Error deleting product with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Product>> GetProductsBySkinTypeAsync(int skinTypeId)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
                    .ThenInclude(pst => pst.SkinType)
                .Where(p => p.ProductSkinTypes.Any(pst => pst.SkinTypeId == skinTypeId))
                .ToListAsync();
        }

        public async Task<int> GetTotalInventoryQuantityAsync(int productId)
        {
            try
            {
                var totalQuantity = await _context.Inventories
                    .Where(i => i.ProductId == productId)
                    .SumAsync(i => i.Quantity ?? 0);
                    
                _logger.LogInformation($"Total inventory quantity for product {productId}: {totalQuantity}");
                return totalQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total inventory quantity for product {productId}");
                throw;
            }
        }

        public async Task<IQueryable<Product>> GetFilteredProductsQueryAsync(
            int? skinTypeId, 
            int? productTypeId, 
            int? branchId, 
            decimal? minPrice, 
            decimal? maxPrice)
        {
            try
            {
                // Start with a base query that includes all the necessary related data
                var query = _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductSkinTypes)
                        .ThenInclude(pst => pst.SkinType)
                    .Include(p => p.Reviews) // Include reviews for rating calculation
                    .AsQueryable();

                // Apply skin type filter
                if (skinTypeId.HasValue)
                {
                    query = query.Where(p => p.ProductSkinTypes.Any(pst => pst.SkinTypeId == skinTypeId.Value));
                }

                // Apply product type filter
                if (productTypeId.HasValue)
                {
                    query = query.Where(p => p.ProductTypeId == productTypeId.Value);
                }

                // Apply branch/brand filter
                if (branchId.HasValue)
                {
                    query = query.Where(p => p.ProductBrandId == branchId.Value);
                }

                // Apply price range filter
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }
                
                _logger.LogInformation("Generated filtered products query");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFilteredProductsQueryAsync");
                throw;
            }
        }

        public async Task<int> GetCountFromQueryAsync(IQueryable<Product> query)
        {
            try
            {
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCountFromQueryAsync");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetPaginatedProductsFromQueryAsync(
            IQueryable<Product> query, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                return await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedProductsFromQueryAsync");
                throw;
            }
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsByTypeWithPaginationAsync(
            int productTypeId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                // Create a query for products with the specified product type
                var query = _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductSkinTypes)
                        .ThenInclude(pst => pst.SkinType)
                    .Include(p => p.Reviews)
                    .Where(p => p.ProductTypeId == productTypeId);
                
                // Get the total count
                int totalCount = await query.CountAsync();
                
                // Apply pagination and get results
                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByTypeWithPaginationAsync for type {productTypeId}");
                throw;
            }
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsByBranchWithPaginationAsync(
            int branchId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                // Create a query for products with the specified branch (brand)
                var query = _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductSkinTypes)
                        .ThenInclude(pst => pst.SkinType)
                    .Include(p => p.Reviews)
                    .Where(p => p.ProductBrandId == branchId);
                
                // Get the total count
                int totalCount = await query.CountAsync();
                
                // Apply pagination and get results
                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsByBranchWithPaginationAsync for branch {branchId}");
                throw;
            }
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsBySkinTypeWithPaginationAsync(
            int skinTypeId, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                // Create a query for products associated with the specified skin type
                var query = _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductSkinTypes)
                        .ThenInclude(pst => pst.SkinType)
                    .Include(p => p.Reviews)
                    .Where(p => p.ProductSkinTypes.Any(pst => pst.SkinTypeId == skinTypeId));
                
                // Get the total count
                int totalCount = await query.CountAsync();
                
                // Apply pagination and get results
                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProductsBySkinTypeWithPaginationAsync for skin type {skinTypeId}");
                throw;
            }
        }
    }
}
