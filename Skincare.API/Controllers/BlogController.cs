using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

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
                return Ok(new { message = "Blogs fetched successfully", data = blogs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching blogs.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                return Ok(new { message = "Blog fetched successfully", data = blog });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "BLOG_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the blog.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] BlogDTO blog)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data.", errors = ModelState });
            try
            {
                var createdBlog = await _blogService.CreateBlogAsync(blog);
                return CreatedAtAction(nameof(GetBlogById), new { id = createdBlog.Id }, new { message = "Blog created successfully", data = createdBlog });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the blog.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogDTO blog)
        {
            if (id != blog.Id)
                return BadRequest(new { message = "ID mismatch." });

            try
            {
                await _blogService.UpdateBlogAsync(blog);
                return Ok(new { message = "Blog updated successfully" });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "BLOG_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the blog.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                await _blogService.DeleteBlogAsync(id);
                return Ok(new { message = "Blog deleted successfully" });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "BLOG_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the blog.", error = ex.Message });
            }
        }
    }
}
