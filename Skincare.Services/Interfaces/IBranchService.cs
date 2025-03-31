using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IBranchService
    {
        Task<IEnumerable<BranchDto>> GetAllBranchesAsync();
        Task<BranchDto> GetBranchByIdAsync(int id);
    }
} 