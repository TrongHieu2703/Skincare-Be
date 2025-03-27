using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public bool IsAvailable { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public int ProductBrandId { get; set; }
        public string ProductBrandName { get; set; }
        
        // Direct product quantity and stock
        public int? Quantity { get; set; }
        public int? Stock { get; set; }
        
        // Inventory information
        public int? InventoryId { get; set; }
        public int? BranchId { get; set; }
        
        // Skin types
        public List<string> SkinTypes { get; set; } = new List<string>();
        public List<int> SkinTypeIds { get; set; } = new List<int>();
    }
}
