namespace Skincare.BusinessObjects.DTOs
{
    public class CreateInventoryDto
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int? Quantity { get; set; }
        public int Stock { get; set; }
    }
}
