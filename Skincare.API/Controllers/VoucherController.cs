using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVouchers()
        {
            try
            {
                var vouchers = await _voucherService.GetAllVouchersAsync();
                return Ok(new { message = "Fetched vouchers successfully", data = vouchers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVoucherById(int id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                return Ok(new { message = "Fetched voucher successfully", data = voucher });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "VOUCHER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto createVoucherDto)
        {
            if (createVoucherDto == null)
                return BadRequest(new { message = "Voucher data is null" });
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            try
            {
                var voucher = await _voucherService.CreateVoucherAsync(createVoucherDto);
                return CreatedAtAction(nameof(GetVoucherById), new { id = voucher.VoucherId }, new { message = "Voucher created successfully", data = voucher });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherDto updateVoucherDto)
        {
            if (updateVoucherDto == null)
                return BadRequest(new { message = "Update data is null" });
            try
            {
                var updatedVoucher = await _voucherService.UpdateVoucherAsync(id, updateVoucherDto);
                return Ok(new { message = "Voucher updated successfully", data = updatedVoucher });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "VOUCHER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            try
            {
                await _voucherService.DeleteVoucherAsync(id);
                return NoContent();
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "VOUCHER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
