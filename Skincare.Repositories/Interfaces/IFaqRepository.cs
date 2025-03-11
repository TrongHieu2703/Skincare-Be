using Skincare.BusinessObjects.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IFaqRepository
    {
        Task<IEnumerable<FaqDTO>> GetAllFaqsAsync();
        Task<FaqDTO> GetFaqByIdAsync(int id);
        Task<FaqDTO> CreateFaqAsync(FaqDTO faq);
        Task UpdateFaqAsync(FaqDTO faq);
        Task DeleteFaqAsync(int id);
    }
}
