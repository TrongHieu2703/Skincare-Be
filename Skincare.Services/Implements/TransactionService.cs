using Skincare.BusinessObjects.DTOs;
using Skincare.Repositories.Implements;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly Skincare.Repositories.Interfaces.ITransactionRepository _transactionRepository;


        public TransactionService(Skincare.Repositories.Interfaces.ITransactionRepository transactionRepository)

        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByOrderIdAsync(int orderId)
        {
            return await _transactionRepository.GetTransactionsByOrderIdAsync(orderId);
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
        {
            return await _transactionRepository.CreateTransactionAsync(createTransactionDto);
        }

        public async Task<TransactionDto> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto)
        {
            return await _transactionRepository.UpdateTransactionAsync(id, updateTransactionDto);
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await _transactionRepository.DeleteTransactionAsync(id);
        }
    }
}