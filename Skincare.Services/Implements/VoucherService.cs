using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task<IEnumerable<VoucherDto>> GetAvailableVouchersAsync()
        {
            var allVouchers = await _voucherRepository.GetAllVouchersAsync();
            var now = DateTime.UtcNow;
            
            Console.WriteLine($"Current UTC time: {now}");
            
            var availableVouchers = allVouchers.Where(v => 
                (v.ExpiredAt > now) && 
                (v.StartedAt <= now) &&
                (v.IsInfinity || v.Quantity > 0)
            ).ToList();
            
            // Debug logging
            foreach (var voucher in allVouchers)
            {
                bool isExpired = voucher.ExpiredAt <= now;
                bool hasStarted = voucher.StartedAt <= now;
                bool hasQuantity = voucher.IsInfinity || voucher.Quantity > 0;
                bool isAvailable = !isExpired && hasStarted && hasQuantity;
                
                Console.WriteLine($"Voucher {voucher.Code} ({voucher.VoucherId}):");
                Console.WriteLine($"  StartedAt: {voucher.StartedAt}, HasStarted: {hasStarted}");
                Console.WriteLine($"  ExpiredAt: {voucher.ExpiredAt}, IsExpired: {isExpired}");
                Console.WriteLine($"  Quantity: {voucher.Quantity}, IsInfinity: {voucher.IsInfinity}, HasQuantity: {hasQuantity}");
                Console.WriteLine($"  IsAvailable: {isAvailable}");
            }
            
            return availableVouchers;
        }
    }
}
