using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ISkinTypeService
    {
        Task<IEnumerable<SkinTypeDto>> GetAllSkinTypesAsync();
        Task<SkinTypeDto> GetSkinTypeByIdAsync(int id);
    }
} 