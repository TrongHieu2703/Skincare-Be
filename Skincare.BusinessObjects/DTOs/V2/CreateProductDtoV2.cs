using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs.V2
{
    public class CreateProductDtoV2
    {
        [Required, MinLength(2)]
        public string Name { get; set; }

        [Required, MinLength(10)]
        public string Description { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public int ProductTypeId { get; set; }
        public int BranchId { get; set; }
        public string Status { get; set; } = "active";

        // New fields for V2
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Benefits { get; set; } = new List<string>();
        
        [MinLength(10)]
        public string UsageInstructions { get; set; }
        
        public string SkinTypeRecommendation { get; set; }
        public bool IsOrganic { get; set; }
        public bool IsCrueltyFree { get; set; }
        
        [Required]
        public string Size { get; set; }
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Images { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
} 