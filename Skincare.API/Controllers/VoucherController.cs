using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVouchers() => Ok(await _voucherService.GetAllVouchersAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVoucherById(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            return voucher != null ? Ok(voucher) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
        {
            var voucher = await _voucherService.CreateVoucherAsync(dto);
            return CreatedAtAction(nameof(GetVoucherById), new { id = voucher.VoucherId }, voucher);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherDto dto)
        {
            var updatedVoucher = await _voucherService.UpdateVoucherAsync(id, dto);
            return Ok(updatedVoucher);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            await _voucherService.DeleteVoucherAsync(id);
            return NoContent();
        }
    }
}
