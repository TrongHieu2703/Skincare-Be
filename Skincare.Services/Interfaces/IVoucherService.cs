﻿using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDto>> GetAllVouchersAsync();
        Task<VoucherDto> GetVoucherByIdAsync(int id);
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto);
        Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto);
        Task DeleteVoucherAsync(int id);
    }
}
