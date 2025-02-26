using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QAController : ControllerBase
    {
        private readonly IFaqService _faqService;

        public QAController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFaqs()
        {
            try
            {
                var faqs = await _faqService.GetAllFaqsAsync();
                return Ok(new { Message = "FAQs fetched successfully", Data = faqs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching FAQs.", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFaqById(int id)
        {
            try
            {
                var faq = await _faqService.GetFaqByIdAsync(id);
                if (faq == null) return NotFound(new { Message = "FAQ not found." });
                return Ok(new { Message = "FAQ fetched successfully", Data = faq });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the FAQ.", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFaq([FromBody] FaqDTO faq)
        {
            try
            {
                var createdFaq = await _faqService.CreateFaqAsync(faq);
                return CreatedAtAction(nameof(GetFaqById), new { id = createdFaq.Id }, new { Message = "FAQ created successfully", Data = createdFaq });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the FAQ.", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFaq(int id, [FromBody] FaqDTO faq)
        {
            if (id != faq.Id) return BadRequest(new { Message = "ID mismatch." });

            try
            {
                await _faqService.UpdateFaqAsync(faq);
                return Ok(new { Message = "FAQ updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the FAQ.", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            try
            {
                await _faqService.DeleteFaqAsync(id);
                return Ok(new { Message = "FAQ deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the FAQ.", Error = ex.Message });
            }
        }
    }
}
