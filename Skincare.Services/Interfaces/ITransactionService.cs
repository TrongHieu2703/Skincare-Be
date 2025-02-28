using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsByOrderIdAsync(int orderId);
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto);
        Task<TransactionDto> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto);
        Task DeleteTransactionAsync(int id);
    }
}
