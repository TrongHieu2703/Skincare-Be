using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetTransactionsByOrderId(int orderId)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsByOrderIdAsync(orderId);
                return Ok(new { message = "Fetched transactions successfully", data = transactions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching transactions", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            if (createTransactionDto == null)
                return BadRequest(new { message = "Transaction data is null" });
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });

            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(createTransactionDto);
                return CreatedAtAction(nameof(GetTransactionsByOrderId), new { orderId = transaction.OrderId }, new { message = "Transaction created successfully", data = transaction });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the transaction", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateTransactionDto)
        {
            if (updateTransactionDto == null)
                return BadRequest(new { message = "Update data is null" });
            try
            {
                var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, updateTransactionDto);
                return Ok(new { message = "Transaction updated successfully", data = updatedTransaction });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "TRANSACTION_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the transaction", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
                return NoContent();
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "TRANSACTION_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the transaction", error = ex.Message });
            }
        }
    }
}
