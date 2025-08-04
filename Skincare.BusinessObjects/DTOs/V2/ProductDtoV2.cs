using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs.V2
{
    public class ProductDtoV2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // New fields for V2
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Benefits { get; set; } = new List<string>();
        public string UsageInstructions { get; set; }
        public string SkinTypeRecommendation { get; set; }
        public bool IsOrganic { get; set; }
        public bool IsCrueltyFree { get; set; }
        public string Size { get; set; }
        public int StockQuantity { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Images { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
} 