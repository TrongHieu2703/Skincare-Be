using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsByOrderIdAsync(int orderId);
         Task<TransactionDto> GetTransactionByIdAsync(int id); 
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto);
        Task<TransactionDto> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto);
        Task DeleteTransactionAsync(int id);
    }
}
