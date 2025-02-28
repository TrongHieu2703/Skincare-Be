using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
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
              var vouchers = await _voucherService.GetAllVouchersAsync();
              return Ok(vouchers);
         }

         [HttpGet("{id}")]
         public async Task<IActionResult> GetVoucherById(int id)
         {
              var voucher = await _voucherService.GetVoucherByIdAsync(id);
              return voucher != null ? Ok(voucher) : NotFound();
         }

         [HttpPost]
         public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto createVoucherDto)
         {
              if (createVoucherDto == null)
                  return BadRequest("Voucher data is null");
              if (!ModelState.IsValid)
                  return BadRequest(ModelState);
              var voucher = await _voucherService.CreateVoucherAsync(createVoucherDto);
              return CreatedAtAction(nameof(GetVoucherById), new { id = voucher.VoucherId }, voucher);
         }

         [HttpPut("{id}")]
         public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherDto updateVoucherDto)
         {
              var updatedVoucher = await _voucherService.UpdateVoucherAsync(id, updateVoucherDto);
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
