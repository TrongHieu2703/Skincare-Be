using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IBranchRepository
    {
        Task<IEnumerable<Branch>> GetAllBranchesAsync();
        Task<Branch> GetBranchByIdAsync(int id);
    }
} 