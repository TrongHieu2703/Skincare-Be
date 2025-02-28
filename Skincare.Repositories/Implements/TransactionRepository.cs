using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly SWP391Context _context;

        public TransactionRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByOrderIdAsync(int orderId)
        {
            return await _context.Transactions
                .Where(t => t.OrderId == orderId)
                .Select(t => new TransactionDto
                {
                    TransactionId = t.TransactionId,
                    OrderId = t.OrderId,
                    PaymentMethod = t.PaymentMethod,
                    Amount = t.Amount,
                    Status = t.Status,
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
        {
            var transaction = new Transaction
            {
                OrderId = createTransactionDto.OrderId,
                PaymentMethod = createTransactionDto.PaymentMethod,
                Amount = createTransactionDto.Amount,
                Status = createTransactionDto.Status,
                CreatedDate = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                OrderId = transaction.OrderId,
                PaymentMethod = transaction.PaymentMethod,
                Amount = transaction.Amount,
                Status = transaction.Status,
                CreatedDate = transaction.CreatedDate
            };
        }

        public async Task<TransactionDto> UpdateTransactionAsync(int id, UpdateTransactionDto updateTransactionDto)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id);
            if (transaction == null) return null;

            transaction.PaymentMethod = updateTransactionDto.PaymentMethod;
            transaction.Amount = updateTransactionDto.Amount;
            transaction.Status = updateTransactionDto.Status;

            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                OrderId = transaction.OrderId,
                PaymentMethod = transaction.PaymentMethod,
                Amount = transaction.Amount,
                Status = transaction.Status,
                CreatedDate = transaction.CreatedDate
            };
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
