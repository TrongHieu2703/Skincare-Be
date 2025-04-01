using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/upload")]
    [Consumes("multipart/form-data")]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IFileService fileService, ILogger<UploadController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }


        [HttpPost("product-image")]

        public async Task<IActionResult> UploadProductImage([FromForm] FileUploadRequest request)
        {
            return await UploadFile(request.File, "product-images");
        }


        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] FileUploadRequest request)
        {
            return await UploadFile(request.File, "avatar-images");
        }

        private async Task<IActionResult> UploadFile(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided or file is empty");

            try
            {
                var filePath = await _fileService.SaveFileAsync(file, folder);
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest("File upload failed");

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "Internal server error");
            }
        }

        // ✅ API XOÁ FILE
        [HttpDelete]
        public IActionResult DeleteFile([FromQuery] string filePath)
        {
            if (_fileService.DeleteFile(filePath))
                return Ok("File deleted successfully");

            return NotFound("File not found");
        }
    }
}
