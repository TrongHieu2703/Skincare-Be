using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.DTOs
{
   public class UpdateOrderDto
{
    public int? VoucherId { get; set; }
    public bool? IsPrepaid { get; set; }
    public string Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    public bool UpdateTransactionStatus { get; set; } = false;
}



}
