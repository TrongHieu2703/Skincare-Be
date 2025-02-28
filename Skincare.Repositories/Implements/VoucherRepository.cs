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
                    MinOrderValue = v.MinOrderValue,
                    Value = v.Value,
                    MaxDiscountValue = v.MaxDiscountValue,
                    StartedAt = v.StartedAt,
                    ExpiredAt = v.ExpiredAt,
                    IsInfinity = v.IsInfinity,
                    Quantity = v.Quantity,
                    PointCost = v.PointCost
                })
                .ToListAsync();
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            return voucher == null ? null : new VoucherDto
            {
                VoucherId = voucher.Id,
                Name = voucher.Name,
                Code = voucher.Code,
                IsPercent = voucher.IsPercent,
                MinOrderValue = voucher.MinOrderValue,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt,
                IsInfinity = voucher.IsInfinity,
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
                MinOrderValue = voucher.MinOrderValue,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt,
                IsInfinity = voucher.IsInfinity,
                Quantity = voucher.Quantity,
                PointCost = voucher.PointCost
            };
        }

        public async Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return null;

            voucher.Name = updateVoucherDto.Name ?? voucher.Name;
            voucher.Code = updateVoucherDto.Code ?? voucher.Code;
            voucher.IsPercent = updateVoucherDto.IsPercent ?? voucher.IsPercent;
            voucher.MinOrderValue = updateVoucherDto.MinOrderValue ?? voucher.MinOrderValue;
            voucher.Value = updateVoucherDto.Value ?? voucher.Value;
            voucher.MaxDiscountValue = updateVoucherDto.MaxDiscountValue ?? voucher.MaxDiscountValue;
            voucher.StartedAt = updateVoucherDto.StartedAt ?? voucher.StartedAt;
            voucher.ExpiredAt = updateVoucherDto.ExpiredAt ?? voucher.ExpiredAt;
            voucher.IsInfinity = updateVoucherDto.IsInfinity ?? voucher.IsInfinity;
            voucher.Quantity = updateVoucherDto.Quantity ?? voucher.Quantity;
            voucher.PointCost = updateVoucherDto.PointCost ?? voucher.PointCost;

            await _context.SaveChangesAsync();

            return new VoucherDto
            {
                VoucherId = voucher.Id,
                Name = voucher.Name,
                Code = voucher.Code,
                IsPercent = voucher.IsPercent,
                MinOrderValue = voucher.MinOrderValue,
                Value = voucher.Value,
                MaxDiscountValue = voucher.MaxDiscountValue,
                StartedAt = voucher.StartedAt,
                ExpiredAt = voucher.ExpiredAt,
                IsInfinity = voucher.IsInfinity,
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
