using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        // Remove Required attribute to allow image upload via IFormFile
        public string? Image { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
        public int ProductTypeId { get; set; }
        [Required]
        public int ProductBrandId { get; set; }
        // Inventory information
        public int? Quantity { get; set; }
        public int? Stock { get; set; }
        public int? BranchId { get; set; }
        // Selected skin types
        public List<int> SkinTypeIds { get; set; } = new List<int>();
    }
}
