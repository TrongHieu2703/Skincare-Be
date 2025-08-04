using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class AdvancedSearchDto
    {
        [StringLength(100, ErrorMessage = "Search keyword cannot exceed 100 characters")]
        public string? Keyword { get; set; }

        public List<int>? CategoryIds { get; set; }
        public List<int>? BrandIds { get; set; }
        public List<int>? SkinTypeIds { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum price cannot be negative")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum price cannot be negative")]
        public decimal? MaxPrice { get; set; }

        [Range(1, 5, ErrorMessage = "Minimum rating must be between 1 and 5")]
        public decimal? MinRating { get; set; }

        public bool? InStock { get; set; }
        public bool? IsOrganic { get; set; }
        public bool? IsCrueltyFree { get; set; }

        [RegularExpression(@"^(name|price|rating|created|popularity)$", ErrorMessage = "Invalid sort field")]
        public string? SortBy { get; set; } = "name";

        [RegularExpression(@"^(asc|desc)$", ErrorMessage = "Sort order must be asc or desc")]
        public string? SortOrder { get; set; } = "asc";

        [Range(1, 100, ErrorMessage = "Page number must be between 1 and 100")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize { get; set; } = 10;

        public bool? HasDiscount { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsBestSeller { get; set; }

        [StringLength(50, ErrorMessage = "Size filter cannot exceed 50 characters")]
        public string? Size { get; set; }

        [StringLength(100, ErrorMessage = "Brand filter cannot exceed 100 characters")]
        public string? Brand { get; set; }
    }
} 