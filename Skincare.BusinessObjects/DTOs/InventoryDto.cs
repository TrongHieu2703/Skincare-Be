using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int? Quantity { get; set; }

        // Tuỳ vào logic, bạn có thể muốn hiển thị thêm tên Branch, tên Sản phẩm
        public string BranchName { get; set; }
        public string ProductName { get; set; }
    }
}
