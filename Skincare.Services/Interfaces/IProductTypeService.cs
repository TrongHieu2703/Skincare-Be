using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IProductTypeService
    {
        Task<IEnumerable<ProductTypeDto>> GetAllProductTypesAsync();
        Task<ProductTypeDto?> GetProductTypeByIdAsync(int id);
    }
} 