using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class ProductTypeRepository : IProductTypeRepository
    {
        private readonly SWP391Context _context;

        public ProductTypeRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductType>> GetAllAsync()
        {
            return await _context.ProductTypes.ToListAsync();
        }

        public async Task<ProductType> GetByIdAsync(int id)
        {
            return await _context.ProductTypes.FindAsync(id);
        }
    }
} 