using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByOrderIdAsync(int orderId)
        {
            return await _transactionRepository.GetTransactionsByOrderIdAsync(orderId);
        }

         public async Task<TransactionDto> GetTransactionByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
            if (transaction == null)
                throw new NotFoundException($"Transaction with ID {id} not found.");
            return transaction;
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
        {
            // Giả sử create luôn thành công nếu dữ liệu hợp lệ.
            return await _transactionRepository.CreateTransactionAsync(createTransactionDto);
        }

        public async Task<TransactionDto> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto)
        {
            var updatedTransaction = await _transactionRepository.UpdateTransactionAsync(id, updateTransactionDto);
            if (updatedTransaction == null)
            {
                throw new NotFoundException($"Transaction with ID {id} not found for update.");
            }
            return updatedTransaction;
        }

        public async Task DeleteTransactionAsync(int id)
        {
            // Giả sử repository có phương thức GetTransactionByIdAsync
            var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction with ID {id} not found for deletion.");
            }
            await _transactionRepository.DeleteTransactionAsync(id);
        }
    }
}
