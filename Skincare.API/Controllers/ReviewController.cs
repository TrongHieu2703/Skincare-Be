using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // Lấy tất cả review theo productId
    [HttpGet("product/{productId}")]
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
            var review = await _reviewService.CreateReviewAsync(createReviewDto);
            return CreatedAtAction(nameof(GetReviewById),
                                  new { id = review.Id },
                                  new { message = "Review created successfully.", data = review });
        }
        catch (NotFoundException nfex)
        {
            return NotFound(new { message = nfex.Message, errorCode = "NOT_FOUND" });
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
            var updatedReview = await _reviewService.UpdateReviewAsync(id, updateReviewDto);
            return Ok(new { message = "Review updated successfully.", data = updatedReview });
        }
        catch (NotFoundException nfex)
        {
            return NotFound(new { message = nfex.Message, errorCode = "REVIEW_NOT_FOUND" });
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
}
