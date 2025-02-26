using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using Skincare.BusinessObjects.DTOs;

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
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductType) // Include ProductType
                .Include(p => p.ProductBrand) // Include ProductBrand
                .Include(p => p.ProductSkinTypes) // Include ProductSkinTypes
                    .ThenInclude(pst => pst.SkinType) // Include SkinType
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
                .Include(p => p.ProductType) // Include ProductType
                .Include(p => p.ProductBrand) // Include ProductBrand
                .Include(p => p.ProductSkinTypes) // Include ProductSkinTypes
                    .ThenInclude(pst => pst.SkinType) // Include SkinType
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

        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
        {
            return await _context.Products
                .Include(p => p.ProductType) // Include ProductType
                .Include(p => p.ProductBrand) // Include ProductBrand
                .Include(p => p.ProductSkinTypes) // Include ProductSkinTypes
                    .ThenInclude(pst => pst.SkinType) // Include SkinType
                .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId)
        {
            return await _context.Products
                .Where(p => p.ProductTypeId == productTypeId)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                ProductTypeId = createProductDto.ProductTypeId,
                IsAvailable = createProductDto.IsAvailable
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }

            product.Name = updateProductDto.Name ?? product.Name;
            product.Description = updateProductDto.Description ?? product.Description;
            product.Price = updateProductDto.Price ?? product.Price;
            product.ProductTypeId = updateProductDto.ProductTypeId ?? product.ProductTypeId;
            product.IsAvailable = updateProductDto.IsAvailable ?? product.IsAvailable;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
