using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_.,()]+$", ErrorMessage = "Product name contains invalid characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Product description must be between 10 and 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price must have maximum 2 decimal places")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Product type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid product type ID")]
        public int ProductTypeId { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid branch ID")]
        public int BranchId { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression(@"^(active|inactive|discontinued)$", ErrorMessage = "Status must be active, inactive, or discontinued")]
        public string Status { get; set; } = "active";

        // Additional validation fields
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int? StockQuantity { get; set; }

        [StringLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string? Size { get; set; }

        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        public string? Brand { get; set; }

        [StringLength(500, ErrorMessage = "Ingredients cannot exceed 500 characters")]
        public string? Ingredients { get; set; }

        [StringLength(200, ErrorMessage = "Usage instructions cannot exceed 200 characters")]
        public string? UsageInstructions { get; set; }

        [StringLength(100, ErrorMessage = "Skin type recommendation cannot exceed 100 characters")]
        public string? SkinTypeRecommendation { get; set; }

        public bool? IsOrganic { get; set; }
        public bool? IsCrueltyFree { get; set; }
    }
}
