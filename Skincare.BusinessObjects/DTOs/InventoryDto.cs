namespace Skincare.BusinessObjects.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int? Quantity { get; set; }
        public int Stock { get; set; }
        public string BranchName { get; set; }
        public string ProductName { get; set; }
    }
}
