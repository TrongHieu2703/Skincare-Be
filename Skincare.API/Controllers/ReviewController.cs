using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // Lấy tất cả review theo productId
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(new { message = "Fetched reviews successfully.", data = reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching reviews.", error = ex.Message });
            }
        }

        // Lấy review theo id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                return Ok(new { message = "Fetched review successfully.", data = review });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "REVIEW_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the review.", error = ex.Message });
            }
        }

        // Tạo review
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data.", errors = ModelState });
            }

            try
            {
                // Lấy userId từ token JWT
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Gán userId từ token vào createReviewDto, bỏ qua CustomerId từ client
                createReviewDto.CustomerId = userId;
                
                var review = await _reviewService.CreateReviewAsync(createReviewDto);
                return CreatedAtAction(nameof(GetReviewById),
                                      new { id = review.Id },
                                      new { message = "Review created successfully.", data = review });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "NOT_FOUND" });
            }
            catch (BusinessLogicException blex)
            {
                return BadRequest(new { message = blex.Message, errorCode = "BUSINESS_LOGIC_ERROR" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the review.", error = ex.Message });
            }
        }

        // Cập nhật review
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data.", errors = ModelState });
            }

            try
            {
                // Lấy userId từ token JWT
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }
                
                // Kiểm tra xem review có thuộc về user hiện tại không
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review.CustomerId != userId)
                {
                    return Forbid();
                }
                
                var updatedReview = await _reviewService.UpdateReviewAsync(id, updateReviewDto);
                return Ok(new { message = "Review updated successfully.", data = updatedReview });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "REVIEW_NOT_FOUND" });
            }
            catch (BusinessLogicException blex)
            {
                return BadRequest(new { message = blex.Message, errorCode = "BUSINESS_LOGIC_ERROR" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the review.", error = ex.Message });
            }
        }

        // Xóa review
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                // Lấy userId từ token JWT
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }
                
                // Kiểm tra xem review có thuộc về user hiện tại không
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review.CustomerId != userId)
                {
                    return Forbid();
                }
                
                await _reviewService.DeleteReviewAsync(id);
                return NoContent();
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "REVIEW_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the review.", error = ex.Message });
            }
        }

        // Explicit route to get average rating
        [HttpGet]
        [Route("product/{productId}/rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductAverageRating(int productId)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingForProductAsync(productId);
                return Ok(new { message = "Fetched average rating successfully.", data = averageRating });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating average rating.", error = ex.Message });
            }
        }

        // Alternative endpoint with a simpler route
        [HttpGet("rating/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRating(int productId)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingForProductAsync(productId);
                return Ok(new { message = "Fetched average rating successfully.", data = averageRating });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating average rating.", error = ex.Message });
            }
        }

        // Get both reviews and average rating in a single call
        [HttpGet("product/{productId}/reviews-with-rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsWithRating(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                var averageRating = await _reviewService.GetAverageRatingForProductAsync(productId);
                
                return Ok(new { 
                    message = "Fetched reviews and rating successfully.", 
                    data = new {
                        reviews = reviews,
                        averageRating = averageRating,
                        totalReviews = reviews.Count()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching reviews and rating.", error = ex.Message });
            }
        }

        // Add a new endpoint to check if a review exists for a specific order item
        [HttpGet("order-item/{orderItemId}")]
        public async Task<IActionResult> GetReviewByOrderItem(int orderItemId)
        {
            try
            {
                // Get current user ID from claims
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var customerId))
                {
                    return Unauthorized(new { message = "User not authenticated or invalid user ID", exists = false });
                }

                var review = await _reviewService.GetReviewByOrderItemAsync(orderItemId, customerId);
                
                if (review == null)
                {
                    return Ok(new { message = "No review found for this order item and customer.", exists = false });
                }
                
                return Ok(new { message = "Review found.", data = review, exists = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the review.", error = ex.Message });
            }
        }
    }
}
