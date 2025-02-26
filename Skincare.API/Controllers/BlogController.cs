using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogsAsync();
                return Ok(new { Message = "Blogs fetched successfully", Data = blogs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching blogs.", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                if (blog == null) return NotFound(new { Message = "Blog not found." });
                return Ok(new { Message = "Blog fetched successfully", Data = blog });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the blog.", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] BlogDTO blog)
        {
            try
            {
                var createdBlog = await _blogService.CreateBlogAsync(blog);
                return CreatedAtAction(nameof(GetBlogById), new { id = createdBlog.Id }, new { Message = "Blog created successfully", Data = createdBlog });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the blog.", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogDTO blog)
        {
            if (id != blog.Id) return BadRequest(new { Message = "ID mismatch." });

            try
            {
                await _blogService.UpdateBlogAsync(blog);
                return Ok(new { Message = "Blog updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the blog.", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                await _blogService.DeleteBlogAsync(id);
                return Ok(new { Message = "Blog deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the blog.", Error = ex.Message });
            }
        }
    }
}
