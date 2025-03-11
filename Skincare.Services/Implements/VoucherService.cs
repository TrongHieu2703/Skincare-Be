using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync()
        {
            return await _voucherRepository.GetAllVouchersAsync();
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(int id)
        {
            var voucher = await _voucherRepository.GetVoucherByIdAsync(id);
            if (voucher == null)
                throw new NotFoundException($"Voucher with ID {id} not found.");
            return voucher;
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto)
        {
            // (Nếu cần kiểm tra trùng mã, có thể thêm logic ở đây)
            return await _voucherRepository.CreateVoucherAsync(createVoucherDto);
        }

        public async Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto)
        {
            var updatedVoucher = await _voucherRepository.UpdateVoucherAsync(id, updateVoucherDto);
            if (updatedVoucher == null)
                throw new NotFoundException($"Voucher with ID {id} not found for update.");
            return updatedVoucher;
        }

        public async Task DeleteVoucherAsync(int id)
        {
            var voucher = await _voucherRepository.GetVoucherByIdAsync(id);
            if (voucher == null)
                throw new NotFoundException($"Voucher with ID {id} not found for deletion.");
            await _voucherRepository.DeleteVoucherAsync(id);
        }
    }
}
