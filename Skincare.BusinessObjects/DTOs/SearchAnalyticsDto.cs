using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class SearchAnalyticsDto
    {
        public DateTime Date { get; set; }
        public int TotalSearches { get; set; }
        public int UniqueSearches { get; set; }
        public int SuccessfulSearches { get; set; }
        public double AverageSearchTime { get; set; }
        public List<TrendingSearchDto> TrendingSearches { get; set; } = new List<TrendingSearchDto>();
        public List<PopularProductDto> PopularProducts { get; set; } = new List<PopularProductDto>();
        public List<SearchCategoryDto> SearchCategories { get; set; } = new List<SearchCategoryDto>();
        public List<SearchTimeDto> SearchTimes { get; set; } = new List<SearchTimeDto>();
    }

    public class TrendingSearchDto
    {
        public string Keyword { get; set; }
        public int SearchCount { get; set; }
        public double GrowthRate { get; set; }
        public DateTime LastSearched { get; set; }
        public List<string> RelatedKeywords { get; set; } = new List<string>();
    }

    public class PopularProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ViewCount { get; set; }
        public int SearchCount { get; set; }
        public int PurchaseCount { get; set; }
        public double ConversionRate { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class SearchCategoryDto
    {
        public string CategoryName { get; set; }
        public int SearchCount { get; set; }
        public int ProductCount { get; set; }
        public double AverageRating { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class SearchTimeDto
    {
        public int Hour { get; set; }
        public int SearchCount { get; set; }
        public double AverageSearchTime { get; set; }
    }

    public class SearchMetricsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSearches { get; set; }
        public int UniqueUsers { get; set; }
        public double AverageSearchTime { get; set; }
        public double SearchSuccessRate { get; set; }
        public List<string> TopKeywords { get; set; } = new List<string>();
        public List<string> ZeroResultKeywords { get; set; } = new List<string>();
    }
} 