using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ILogger<BlogService> _logger;

        public BlogService(IBlogRepository blogRepository, ILogger<BlogService> logger)
        {
            _blogRepository = blogRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<BlogDTO>> GetAllBlogsAsync()
        {
            try
            {
                return await _blogRepository.GetAllBlogsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching blogs");
                throw new ApplicationException("An error occurred while fetching blogs.");
            }
        }

        public async Task<BlogDTO> GetBlogByIdAsync(int id)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(id);
                if (blog == null)
                    throw new NotFoundException("Blog not found.");
                return blog;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching blog with ID {id}");
                throw new ApplicationException("An error occurred while fetching the blog.");
            }
        }

        public async Task<BlogDTO> CreateBlogAsync(BlogDTO blog)
        {
            try
            {
                return await _blogRepository.CreateBlogAsync(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                throw new ApplicationException("An error occurred while creating the blog.");
            }
        }

        public async Task UpdateBlogAsync(BlogDTO blog)
        {
            try
            {
                // Kiểm tra xem blog có tồn tại không
                var existing = await _blogRepository.GetBlogByIdAsync(blog.Id);
                if (existing == null)
                    throw new NotFoundException("Blog not found for update.");
                await _blogRepository.UpdateBlogAsync(blog);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog");
                throw new ApplicationException("An error occurred while updating the blog.");
            }
        }

        public async Task DeleteBlogAsync(int id)
        {
            try
            {
                var existing = await _blogRepository.GetBlogByIdAsync(id);
                if (existing == null)
                    throw new NotFoundException("Blog not found for deletion.");
                await _blogRepository.DeleteBlogAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog");
                throw new ApplicationException("An error occurred while deleting the blog.");
            }
        }
    }
}
