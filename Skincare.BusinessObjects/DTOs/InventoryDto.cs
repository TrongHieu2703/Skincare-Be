using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class InventoryDto
    {
        public int InventoryId { get; set; }  
        public int ProductId { get; set; }
        public int BranchId { get; set; }     
        public int? Quantity { get; set; }
    }
}
