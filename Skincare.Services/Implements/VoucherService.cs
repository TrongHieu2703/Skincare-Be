using Skincare.BusinessObjects.DTOs;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
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
            return await _voucherRepository.GetVoucherByIdAsync(id);
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto)
        {
            return await _voucherRepository.CreateVoucherAsync(createVoucherDto);
        }

        public async Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto)
        {
            return await _voucherRepository.UpdateVoucherAsync(id, updateVoucherDto);
        }

        public async Task DeleteVoucherAsync(int id)
        {
            await _voucherRepository.DeleteVoucherAsync(id);
        }
    }
}
