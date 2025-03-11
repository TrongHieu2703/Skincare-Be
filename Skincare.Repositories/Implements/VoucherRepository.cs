using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly SWP391Context _context;

        public VoucherRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync()
        {
            return await _context.Vouchers
                .Select(v => new VoucherDto
                {
                    VoucherId = v.Id,
                    Name = v.Name,
                    Code = v.Code,
                    IsPercent = v.IsPercent, 
                    MinOrderValue = v.MinOrderValue ?? 0m,
                    Value = v.Value,
                    MaxDiscountValue = v.MaxDiscountValue ?? 0m,
                    StartedAt = v.StartedAt,
                    ExpiredAt = v.ExpiredAt ?? default(DateTime),
                    IsInfinity = v.IsInfinity ?? false,
                    Quantity = v.Quantity,
                    PointCost = v.PointCost
                })
                .ToListAsync();
        }

        public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                return null;
            return new VoucherDto
            {
                VoucherId = voucher.Id,
                Name = voucher.Name,
                Code = voucher.Code,
                IsPercent = voucher.IsPercent,
                MinOrderValue = voucher.MinOrderValue ?? 0m,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue ?? 0m,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt ?? default(DateTime),
                IsInfinity = voucher.IsInfinity ?? false,
                Quantity = voucher.Quantity,
                PointCost = voucher.PointCost
            };
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto)
        {
            var voucher = new Voucher
            {
                Name = createVoucherDto.Name,
                Code = createVoucherDto.Code,
                IsPercent = createVoucherDto.IsPercent,
                MinOrderValue = createVoucherDto.MinOrderValue,
                Value = createVoucherDto.Value,
                MaxDiscountValue = createVoucherDto.MaxDiscountValue,
                StartedAt = createVoucherDto.StartedAt,
                ExpiredAt = createVoucherDto.ExpiredAt,
                IsInfinity = createVoucherDto.IsInfinity,
                Quantity = createVoucherDto.Quantity,
                PointCost = createVoucherDto.PointCost
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return new VoucherDto
            {
                VoucherId = voucher.Id,
                Name = voucher.Name,
                Code = voucher.Code,
                IsPercent = voucher.IsPercent, 
                MinOrderValue = voucher.MinOrderValue ?? 0m,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue ?? 0m,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt ?? default(DateTime),
                IsInfinity = voucher.IsInfinity ?? false,
                Quantity = voucher.Quantity,
                PointCost = voucher.PointCost
            };
        }

        public async Task<VoucherDto?> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                return null;

            voucher.Name = updateVoucherDto.Name ?? voucher.Name;
            voucher.Code = updateVoucherDto.Code ?? voucher.Code;
            voucher.IsPercent = updateVoucherDto.IsPercent.HasValue ? updateVoucherDto.IsPercent.Value : voucher.IsPercent;
            voucher.MinOrderValue = updateVoucherDto.MinOrderValue.HasValue ? updateVoucherDto.MinOrderValue.Value : voucher.MinOrderValue;
            voucher.Value = updateVoucherDto.Value.HasValue ? updateVoucherDto.Value.Value : voucher.Value;
            voucher.MaxDiscountValue = updateVoucherDto.MaxDiscountValue.HasValue ? updateVoucherDto.MaxDiscountValue.Value : voucher.MaxDiscountValue;
            voucher.StartedAt = updateVoucherDto.StartedAt.HasValue ? updateVoucherDto.StartedAt.Value : voucher.StartedAt;
            voucher.ExpiredAt = updateVoucherDto.ExpiredAt.HasValue ? updateVoucherDto.ExpiredAt.Value : voucher.ExpiredAt;
            voucher.IsInfinity = updateVoucherDto.IsInfinity.HasValue ? updateVoucherDto.IsInfinity.Value : voucher.IsInfinity;
            voucher.Quantity = updateVoucherDto.Quantity.HasValue ? updateVoucherDto.Quantity.Value : voucher.Quantity;
            voucher.PointCost = updateVoucherDto.PointCost.HasValue ? updateVoucherDto.PointCost.Value : voucher.PointCost;

            await _context.SaveChangesAsync();

            return new VoucherDto
            {
                VoucherId = voucher.Id,
                Name = voucher.Name,
                Code = voucher.Code,
                IsPercent = voucher.IsPercent, 
                MinOrderValue = voucher.MinOrderValue ?? 0m,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue ?? 0m,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt ?? default(DateTime),
                IsInfinity = voucher.IsInfinity ?? false,
                Quantity = voucher.Quantity,
                PointCost = voucher.PointCost
            };
        }

        public async Task DeleteVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
            }
        }
    }
}
