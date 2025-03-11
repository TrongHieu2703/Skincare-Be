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
        public string Image { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
        public int ProductTypeId { get; set; }
        [Required]
        public int ProductBrandId { get; set; }
    }
}
