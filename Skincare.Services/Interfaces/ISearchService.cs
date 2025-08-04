using Skincare.BusinessObjects.DTOs;

namespace Skincare.Services.Interfaces
{
    public interface ISearchService
    {
        // Full-text search
        Task<FullTextSearchResultDto> FullTextSearchAsync(FullTextSearchDto searchDto, int pageNumber = 1, int pageSize = 10);
        
        // Search suggestions and autocomplete
        Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string query, int limit = 10);
        Task<List<string>> GetAutocompleteSuggestionsAsync(string query, int limit = 5);
        Task<List<SearchSuggestionDto>> GetSmartSuggestionsAsync(string query, int limit = 10);
        
        // Search analytics
        Task<SearchAnalyticsDto> GetSearchAnalyticsAsync(DateTime date);
        Task<SearchMetricsDto> GetSearchMetricsAsync(DateTime startDate, DateTime endDate);
        Task<List<TrendingSearchDto>> GetTrendingSearchesAsync(int limit = 10);
        Task<List<PopularProductDto>> GetPopularProductsAsync(int limit = 10);
        
        // Search indexing
        Task<bool> IndexProductAsync(ProductDto product);
        Task<bool> IndexAllProductsAsync();
        Task<bool> DeleteProductIndexAsync(int productId);
        Task<bool> UpdateProductIndexAsync(ProductDto product);
        
        // Search optimization
        Task<bool> OptimizeSearchIndexAsync();
        Task<Dictionary<string, object>> GetSearchIndexStatsAsync();
        Task<bool> RebuildSearchIndexAsync();
        
        // Search monitoring
        Task LogSearchQueryAsync(string query, string userId, bool hasResults, double responseTime);
        Task<List<string>> GetZeroResultQueriesAsync(int limit = 20);
        Task<double> GetAverageSearchTimeAsync(DateTime startDate, DateTime endDate);
    }
} 