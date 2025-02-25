using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;

public class BlogRepository : IBlogRepository
{
    private readonly SWP391Context _context;
    private readonly ILogger<BlogRepository> _logger;

    public BlogRepository(SWP391Context context, ILogger<BlogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BlogDTO>> GetAllBlogsAsync()
    {
        try
        {
            return await _context.BlogPosts.Include(b => b.BlogCategory).Include(b => b.BlogOwner)
                .Select(b => new BlogDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    Img = b.Img,
                    BlogOwnerId = b.BlogOwnerId,
                    BlogCategoryId = b.BlogCategoryId,
                    UpdatedAt = b.UpdatedAt,
                    IsVisible = b.IsVisible
                }).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching blogs from database");
            throw;
        }
    }

    public async Task<BlogDTO> GetBlogByIdAsync(int id)
    {
        try
        {
            var blog = await _context.BlogPosts.Where(b => b.Id == id).Select(b => new BlogDTO
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                Img = b.Img,
                BlogOwnerId = b.BlogOwnerId,
                BlogCategoryId = b.BlogCategoryId,
                UpdatedAt = b.UpdatedAt,
                IsVisible = b.IsVisible
            }).FirstOrDefaultAsync();

            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            return blog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching blog with ID {id}");
            throw;
        }
    }

    public async Task<BlogDTO> CreateBlogAsync(BlogDTO blog)
    {
        try
        {
            var newBlog = new BlogPost
            {
                Title = blog.Title,
                Content = blog.Content,
                Img = blog.Img,
                BlogOwnerId = blog.BlogOwnerId,
                BlogCategoryId = blog.BlogCategoryId,
                UpdatedAt = DateTime.UtcNow,
                IsVisible = blog.IsVisible
            };

            _context.BlogPosts.Add(newBlog);
            await _context.SaveChangesAsync();

            blog.Id = newBlog.Id;
            return blog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog");
            throw;
        }
    }

    public async Task UpdateBlogAsync(BlogDTO blog)
    {
        try
        {
            var existingBlog = await _context.BlogPosts.FindAsync(blog.Id);
            if (existingBlog == null)
                throw new KeyNotFoundException($"Blog with ID {blog.Id} not found.");

            existingBlog.Title = blog.Title;
            existingBlog.Content = blog.Content;
            existingBlog.Img = blog.Img;
            existingBlog.BlogCategoryId = blog.BlogCategoryId;
            existingBlog.UpdatedAt = DateTime.UtcNow;
            existingBlog.IsVisible = blog.IsVisible;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog");
            throw;
        }
    }

    public async Task DeleteBlogAsync(int id)
    {
        try
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            _context.BlogPosts.Remove(blog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog");
            throw;
        }
    }
}
