using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;
        private readonly ILogger<BranchController> _logger;

        public BranchController(
            IBranchService branchService,
            ILogger<BranchController> logger)
        {
            _branchService = branchService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBranches()
        {
            try
            {
                var branches = await _branchService.GetAllBranchesAsync();
                return Ok(new { message = "Branches retrieved successfully", data = branches });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving branches");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranchById(int id)
        {
            try
            {
                var branch = await _branchService.GetBranchByIdAsync(id);
                if (branch == null)
                {
                    return NotFound(new { message = "Branch not found", errorCode = "BRANCH_NOT_FOUND" });
                }
                return Ok(new { message = "Branch retrieved successfully", data = branch });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving branch with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
} 