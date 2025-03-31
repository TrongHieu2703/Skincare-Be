using System.Collections.Generic;
using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface ISkinTypeRepository
    {
        Task<IEnumerable<SkinType>> GetAllAsync();
        Task<SkinType> GetByIdAsync(int id);
    }
} 