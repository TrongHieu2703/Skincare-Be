using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class BranchRepository : IBranchRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<BranchRepository> _logger;

        public BranchRepository(SWP391Context context, ILogger<BranchRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesAsync()
        {
            try
            {
                return await _context.Branches.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllBranchesAsync");
                throw;
            }
        }

        public async Task<Branch> GetBranchByIdAsync(int id)
        {
            try
            {
                return await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetBranchByIdAsync for ID {id}");
                throw;
            }
        }
    }
} 