using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using Skincare.BusinessObjects.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Skincare.Repositories.Implements
{
    public class ProductRepository : IProductRepository
    {
        private readonly SWP391Context _context;

        public ProductRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
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
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId)
        {
            return await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductSkinTypes)
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

            // Cập nhật các trường
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Image = product.Image;
            existing.IsAvailable = product.IsAvailable;
            existing.ProductTypeId = product.ProductTypeId;
            existing.ProductBrandId = product.ProductBrandId;

            _context.Products.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
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



    }
}
