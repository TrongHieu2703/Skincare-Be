// Inventory.cs
#nullable disable
using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.Entities
{
    public partial class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int? Quantity { get; set; }
        public int Stock { get; set; }  // Số lượng tối đa có thể lưu kho hoặc mục đích khác tùy theo logic

        public virtual Branch Branch { get; set; }
        public virtual Product Product { get; set; }
        // Nếu có quan hệ nhiều sản phẩm, bạn có thể giữ Collection, tùy vào thiết kế.
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
