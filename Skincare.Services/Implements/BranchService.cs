using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ILogger<BranchService> _logger;

        public BranchService(IBranchRepository branchRepository, ILogger<BranchService> logger)
        {
            _branchRepository = branchRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<BranchDto>> GetAllBranchesAsync()
        {
            try
            {
                var branches = await _branchRepository.GetAllBranchesAsync();
                return branches.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllBranchesAsync");
                throw;
            }
        }

        public async Task<BranchDto> GetBranchByIdAsync(int id)
        {
            try
            {
                var branch = await _branchRepository.GetBranchByIdAsync(id);
                if (branch == null)
                {
                    throw new NotFoundException($"Branch with ID {id} not found.");
                }
                return MapToDto(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetBranchByIdAsync for ID {id}");
                throw;
            }
        }

        private BranchDto MapToDto(Branch branch)
        {
            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name
            };
        }
    }
} 