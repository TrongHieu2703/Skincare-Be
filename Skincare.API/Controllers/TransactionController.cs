using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
              var transactions = await _transactionService.GetTransactionsByOrderIdAsync(orderId);
              return Ok(transactions);
         }

         [HttpPost]
         public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
         {
              if (createTransactionDto == null)
                  return BadRequest("Transaction data is null");
              if (!ModelState.IsValid)
                  return BadRequest(ModelState);
              var transaction = await _transactionService.CreateTransactionAsync(createTransactionDto);
              return Created("", transaction);
         }

         [HttpPut("{id}")]
         public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateTransactionDto)
         {
              if (updateTransactionDto == null)
                  return BadRequest("Update data is null");
              var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, updateTransactionDto);
              return Ok(updatedTransaction);
         }

         [HttpDelete("{id}")]
         public async Task<IActionResult> DeleteTransaction(int id)
         {
              await _transactionService.DeleteTransactionAsync(id);
              return NoContent();
         }
    }
}
