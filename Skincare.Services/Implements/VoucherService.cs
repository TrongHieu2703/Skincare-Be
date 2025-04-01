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
            
            // Get current date in local time without time component
            var now = DateTime.Now;
            var today = now.Date;  // This gives 00:00:00 for today
            
            Console.WriteLine($"Current local date and time: {now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Current local date without time: {today:yyyy-MM-dd}");
            
            var availableVouchers = allVouchers.Where(v => 
                // Check start date (voucher has started) - compare date portions only
                (v.StartedAt.Date <= today) && 
                // Check expiry date (voucher hasn't expired) - consider the entire day for expiry
                (v.IsInfinity || v.ExpiredAt.Date >= today) &&
                // Check quantity
                (v.IsInfinity || v.Quantity > 0)
            ).ToList();
            
            // Debug logging
            foreach (var voucher in allVouchers)
            {
                // Compare dates properly
                bool hasStarted = voucher.StartedAt.Date <= today;
                bool isExpired = !voucher.IsInfinity && voucher.ExpiredAt.Date < today;
                bool hasQuantity = voucher.IsInfinity || voucher.Quantity > 0;
                bool isShipping = !voucher.IsPercent && voucher.Value == 0;
                bool isAvailable = hasStarted && !isExpired && hasQuantity;
                
                Console.WriteLine($"Voucher {voucher.Code} ({voucher.VoucherId}):");
                Console.WriteLine($"  StartedAt: {voucher.StartedAt:yyyy-MM-dd}, HasStarted: {hasStarted}");
                Console.WriteLine($"  ExpiredAt: {voucher.ExpiredAt:yyyy-MM-dd}, IsExpired: {isExpired}");
                Console.WriteLine($"  Quantity: {voucher.Quantity}, IsInfinity: {voucher.IsInfinity}, HasQuantity: {hasQuantity}");
                Console.WriteLine($"  IsShipping: {isShipping}, IsPercent: {voucher.IsPercent}, Value: {voucher.Value}");
                Console.WriteLine($"  IsAvailable: {isAvailable}");
            }
            
            return availableVouchers;
        }
    }
}
