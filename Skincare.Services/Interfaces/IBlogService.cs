using Skincare.BusinessObjects.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IBlogService
    {
        Task<IEnumerable<BlogDTO>> GetAllBlogsAsync();
        Task<BlogDTO> GetBlogByIdAsync(int id);
        Task<BlogDTO> CreateBlogAsync(BlogDTO blog);
        Task UpdateBlogAsync(BlogDTO blog);
        Task DeleteBlogAsync(int id);
    }
}
