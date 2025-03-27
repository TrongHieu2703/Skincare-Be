using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class SkinTypeRepository : ISkinTypeRepository
    {
        private readonly SWP391Context _context;

        public SkinTypeRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SkinType>> GetAllAsync()
        {
            return await _context.SkinTypes.ToListAsync();
        }

        public async Task<SkinType> GetByIdAsync(int id)
        {
            return await _context.SkinTypes.FindAsync(id);
        }
    }
} 