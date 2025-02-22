using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all reviews");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound();

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching review with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching reviews for product ID {productId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            if (review == null)
                return BadRequest("Review is null");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdReview = await _reviewService.AddReviewAsync(review);
                return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, createdReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting review with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}