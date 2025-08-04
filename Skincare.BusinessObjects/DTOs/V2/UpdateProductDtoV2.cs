using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs.V2
{
    public class UpdateProductDtoV2
    {
        [MinLength(2)]
        public string Name { get; set; }

        [MinLength(10)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        public int? ProductTypeId { get; set; }
        public int? BranchId { get; set; }
        public string Status { get; set; }

        // New fields for V2
        public List<string> Ingredients { get; set; }
        public List<string> Benefits { get; set; }
        
        [MinLength(10)]
        public string UsageInstructions { get; set; }
        
        public string SkinTypeRecommendation { get; set; }
        public bool? IsOrganic { get; set; }
        public bool? IsCrueltyFree { get; set; }
        
        public string Size { get; set; }
        
        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }
        
        public List<string> Tags { get; set; }
        public List<string> Images { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
} 