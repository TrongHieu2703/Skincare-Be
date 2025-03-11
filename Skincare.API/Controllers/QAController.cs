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
                return Ok(new { message = "FAQs fetched successfully", data = faqs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching FAQs", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFaqById(int id)
        {
            try
            {
                var faq = await _faqService.GetFaqByIdAsync(id);
                return Ok(new { message = "FAQ fetched successfully", data = faq });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "FAQ_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the FAQ", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFaq([FromBody] FaqDTO faq)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });
            try
            {
                var createdFaq = await _faqService.CreateFaqAsync(faq);
                return CreatedAtAction(nameof(GetFaqById), new { id = createdFaq.Id }, new { message = "FAQ created successfully", data = createdFaq });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the FAQ", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFaq(int id, [FromBody] FaqDTO faq)
        {
            if (id != faq.Id)
                return BadRequest(new { message = "ID mismatch" });
            try
            {
                await _faqService.UpdateFaqAsync(faq);
                return Ok(new { message = "FAQ updated successfully" });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "FAQ_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the FAQ", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            try
            {
                await _faqService.DeleteFaqAsync(id);
                return Ok(new { message = "FAQ deleted successfully" });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "FAQ_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the FAQ", error = ex.Message });
            }
        }
    }
}
