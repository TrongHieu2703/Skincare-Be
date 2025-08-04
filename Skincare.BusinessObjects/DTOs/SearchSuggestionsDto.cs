using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class SearchSuggestionsDto
    {
        public List<string> Keywords { get; set; } = new List<string>();
        public List<ProductSuggestionDto> Products { get; set; } = new List<ProductSuggestionDto>();
        public List<CategorySuggestionDto> Categories { get; set; } = new List<CategorySuggestionDto>();
        public List<BrandSuggestionDto> Brands { get; set; } = new List<BrandSuggestionDto>();
        public List<string> PopularSearches { get; set; } = new List<string>();
        public List<string> TrendingSearches { get; set; } = new List<string>();
    }

    public class ProductSuggestionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? Rating { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public bool InStock { get; set; }
        public string HighlightedName { get; set; }
    }

    public class CategorySuggestionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public string HighlightedName { get; set; }
    }

    public class BrandSuggestionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int ProductCount { get; set; }
        public string HighlightedName { get; set; }
    }
} 