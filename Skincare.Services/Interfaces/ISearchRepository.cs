using Skincare.BusinessObjects.DTOs;

namespace Skincare.Repositories.Interfaces
{
    public interface ISearchRepository
    {
        // Search query logging
        Task LogSearchQueryAsync(string query, string userId, bool hasResults, double responseTime, DateTime timestamp);
        Task<List<SearchQueryLog>> GetSearchQueriesAsync(DateTime startDate, DateTime endDate, int limit = 100);
        
        // Search analytics
        Task<SearchAnalyticsDto> GetSearchAnalyticsAsync(DateTime date);
        Task<SearchMetricsDto> GetSearchMetricsAsync(DateTime startDate, DateTime endDate);
        Task<List<TrendingSearchDto>> GetTrendingSearchesAsync(int limit = 10);
        Task<List<PopularProductDto>> GetPopularProductsAsync(int limit = 10);
        
        // Search suggestions
        Task<List<string>> GetPopularSearchesAsync(int limit = 10);
        Task<List<string>> GetRecentSearchesAsync(string userId, int limit = 10);
        Task<List<string>> GetRelatedSearchesAsync(string query, int limit = 5);
        
        // Search optimization
        Task<List<string>> GetZeroResultQueriesAsync(int limit = 20);
        Task<double> GetAverageSearchTimeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalSearchesAsync(DateTime startDate, DateTime endDate);
        Task<int> GetUniqueSearchesAsync(DateTime startDate, DateTime endDate);
        
        // Search indexing
        Task<bool> UpdateSearchIndexAsync(int productId, string productName, string description, string brand);
        Task<bool> DeleteSearchIndexAsync(int productId);
    }

    public class SearchQueryLog
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public string UserId { get; set; }
        public bool HasResults { get; set; }
        public double ResponseTime { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
    }
} 