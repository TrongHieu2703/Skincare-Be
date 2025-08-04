using Elasticsearch.Net;
using Nest;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Skincare.Services.Implements
{
    public class ElasticsearchService : ISearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _indexName = "skincare_products";

        public ElasticsearchService(ILogger<ElasticsearchService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var settings = new ConnectionSettings(new Uri(_configuration["Elasticsearch:Url"] ?? "http://localhost:9200"))
                .DefaultIndex(_indexName)
                .EnableDebugMode()
                .PrettyJson();

            _elasticClient = new ElasticClient(settings);
        }

        public async Task<FullTextSearchResultDto> FullTextSearchAsync(FullTextSearchDto searchDto, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"Performing full-text search: {searchDto.Query}");

                var searchRequest = new SearchRequest<ElasticsearchProductDocument>
                {
                    From = (pageNumber - 1) * pageSize,
                    Size = pageSize,
                    Query = BuildSearchQuery(searchDto),
                    Highlight = BuildHighlightDescriptor(searchDto),
                    Aggregations = BuildAggregations(),
                    Sort = BuildSortDescriptor()
                };

                var searchResponse = await _elasticClient.SearchAsync<ElasticsearchProductDocument>(searchRequest);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError($"Elasticsearch search failed: {searchResponse.DebugInformation}");
                    throw new Exception($"Search failed: {searchResponse.ServerError?.Error?.Reason}");
                }

                var result = new FullTextSearchResultDto
                {
                    TotalHits = (int)searchResponse.Total,
                    MaxScore = searchResponse.MaxScore ?? 0,
                    Took = searchResponse.Took,
                    Products = MapToFullTextProducts(searchResponse.Documents, searchResponse.Hits),
                    Suggestions = ExtractSuggestions(searchResponse),
                    Aggregations = ExtractAggregations(searchResponse.Aggregations)
                };

                _logger.LogInformation($"Search completed: {result.TotalHits} results found in {result.Took}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FullTextSearchAsync");
                throw;
            }
        }

        public async Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string query, int limit = 10)
        {
            try
            {
                var suggestions = new SearchSuggestionsDto();

                // Get keyword suggestions
                suggestions.Keywords = await GetAutocompleteSuggestionsAsync(query, limit);

                // Get product suggestions
                var productSearch = await FullTextSearchAsync(new FullTextSearchDto { Query = query }, 1, limit);
                suggestions.Products = productSearch.Products.Select(p => new ProductSuggestionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    Rating = p.Rating,
                    Category = p.Category,
                    Brand = p.Brand,
                    InStock = p.InStock,
                    HighlightedName = p.Highlights.ContainsKey("name") ? p.Highlights["name"].FirstOrDefault() : p.Name
                }).ToList();

                // Get popular searches
                suggestions.PopularSearches = await GetPopularSearchesAsync(limit);

                // Get trending searches
                suggestions.TrendingSearches = (await GetTrendingSearchesAsync(limit)).Select(t => t.Keyword).ToList();

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSearchSuggestionsAsync");
                throw;
            }
        }

        public async Task<List<string>> GetAutocompleteSuggestionsAsync(string query, int limit = 5)
        {
            try
            {
                var suggestRequest = new SearchRequest<ElasticsearchProductDocument>
                {
                    Size = 0,
                    Suggest = new SuggestContainer
                    {
                        ["product-suggestions"] = new SuggestBucket
                        {
                            Prefix = query,
                            Completion = new CompletionSuggester
                            {
                                Field = "suggest",
                                Size = limit,
                                SkipDuplicates = true
                            }
                        }
                    }
                };

                var response = await _elasticClient.SearchAsync<ElasticsearchProductDocument>(suggestRequest);

                if (!response.IsValid)
                {
                    _logger.LogError($"Autocomplete suggestion failed: {response.DebugInformation}");
                    return new List<string>();
                }

                var suggestions = new List<string>();
                if (response.Suggest.ContainsKey("product-suggestions"))
                {
                    foreach (var suggestion in response.Suggest["product-suggestions"])
                    {
                        suggestions.AddRange(suggestion.Options.Select(o => o.Text));
                    }
                }

                return suggestions.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAutocompleteSuggestionsAsync");
                return new List<string>();
            }
        }

        public async Task<List<SearchSuggestionDto>> GetSmartSuggestionsAsync(string query, int limit = 10)
        {
            try
            {
                var suggestions = new List<SearchSuggestionDto>();

                // Get basic autocomplete suggestions
                var autocompleteSuggestions = await GetAutocompleteSuggestionsAsync(query, limit);
                suggestions.AddRange(autocompleteSuggestions.Select(s => new SearchSuggestionDto
                {
                    Text = s,
                    Score = 1.0,
                    Frequency = 1
                }));

                // Get related terms (simplified implementation)
                var relatedTerms = await GetRelatedTermsAsync(query);
                suggestions.AddRange(relatedTerms.Select(t => new SearchSuggestionDto
                {
                    Text = t,
                    Score = 0.8,
                    Frequency = 1,
                    RelatedTerms = new List<string> { query }
                }));

                return suggestions.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSmartSuggestionsAsync");
                return new List<SearchSuggestionDto>();
            }
        }

        public async Task<bool> IndexProductAsync(ProductDto product)
        {
            try
            {
                var document = new ElasticsearchProductDocument
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.Image,
                    Brand = product.ProductBrandName,
                    Category = product.ProductTypeName,
                    Rating = product.AverageRating ?? 0,
                    ReviewCount = product.ReviewsCount ?? 0,
                    InStock = product.IsAvailable,
                    Ingredients = product.Description, // Simplified mapping
                    Suggest = new CompletionField
                    {
                        Input = new[] { product.Name, product.ProductBrandName, product.ProductTypeName }
                    }
                };

                var response = await _elasticClient.IndexDocumentAsync(document);

                if (!response.IsValid)
                {
                    _logger.LogError($"Failed to index product {product.Id}: {response.DebugInformation}");
                    return false;
                }

                _logger.LogInformation($"Successfully indexed product {product.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error indexing product {product.Id}");
                return false;
            }
        }

        public async Task<bool> IndexAllProductsAsync()
        {
            try
            {
                // This would typically be called from a background job
                // For now, we'll just return true as a placeholder
                _logger.LogInformation("Index all products operation requested");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexAllProductsAsync");
                return false;
            }
        }

        public async Task<bool> DeleteProductIndexAsync(int productId)
        {
            try
            {
                var response = await _elasticClient.DeleteAsync<ElasticsearchProductDocument>(productId);

                if (!response.IsValid)
                {
                    _logger.LogError($"Failed to delete product index {productId}: {response.DebugInformation}");
                    return false;
                }

                _logger.LogInformation($"Successfully deleted product index {productId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product index {productId}");
                return false;
            }
        }

        public async Task<bool> UpdateProductIndexAsync(ProductDto product)
        {
            return await IndexProductAsync(product);
        }

        public async Task<bool> OptimizeSearchIndexAsync()
        {
            try
            {
                var response = await _elasticClient.ForceMergeAsync(_indexName);
                return response.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing search index");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetSearchIndexStatsAsync()
        {
            try
            {
                var response = await _elasticClient.Indices.StatsAsync(_indexName);
                
                if (!response.IsValid)
                {
                    return new Dictionary<string, object>();
                }

                return new Dictionary<string, object>
                {
                    ["totalDocuments"] = response.Indices[_indexName].Total.Documents.Count,
                    ["totalSize"] = response.Indices[_indexName].Total.Store.SizeInBytes,
                    ["indexingRate"] = response.Indices[_indexName].Total.Indexing.IndexTotal
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search index stats");
                return new Dictionary<string, object>();
            }
        }

        public async Task<bool> RebuildSearchIndexAsync()
        {
            try
            {
                // This would typically involve creating a new index and reindexing all data
                _logger.LogInformation("Rebuild search index operation requested");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding search index");
                return false;
            }
        }

        public async Task LogSearchQueryAsync(string query, string userId, bool hasResults, double responseTime)
        {
            try
            {
                // This would typically log to a separate search analytics index
                _logger.LogInformation($"Search query logged: {query}, User: {userId}, HasResults: {hasResults}, Time: {responseTime}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging search query");
            }
        }

        public async Task<List<string>> GetZeroResultQueriesAsync(int limit = 20)
        {
            // Placeholder implementation
            return new List<string>();
        }

        public async Task<double> GetAverageSearchTimeAsync(DateTime startDate, DateTime endDate)
        {
            // Placeholder implementation
            return 150.0; // Average 150ms
        }

        public async Task<SearchAnalyticsDto> GetSearchAnalyticsAsync(DateTime date)
        {
            // Placeholder implementation
            return new SearchAnalyticsDto
            {
                Date = date,
                TotalSearches = 1000,
                UniqueSearches = 800,
                SuccessfulSearches = 950,
                AverageSearchTime = 150.0
            };
        }

        public async Task<SearchMetricsDto> GetSearchMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Placeholder implementation
            return new SearchMetricsDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalSearches = 5000,
                UniqueUsers = 2000,
                AverageSearchTime = 150.0,
                SearchSuccessRate = 95.0
            };
        }

        public async Task<List<TrendingSearchDto>> GetTrendingSearchesAsync(int limit = 10)
        {
            // Placeholder implementation
            return new List<TrendingSearchDto>
            {
                new TrendingSearchDto { Keyword = "moisturizer", SearchCount = 150, GrowthRate = 25.5 },
                new TrendingSearchDto { Keyword = "sunscreen", SearchCount = 120, GrowthRate = 18.2 },
                new TrendingSearchDto { Keyword = "cleanser", SearchCount = 100, GrowthRate = 12.8 }
            };
        }

        public async Task<List<PopularProductDto>> GetPopularProductsAsync(int limit = 10)
        {
            // Placeholder implementation
            return new List<PopularProductDto>
            {
                new PopularProductDto { ProductId = 1, ProductName = "Hydrating Moisturizer", ViewCount = 500, SearchCount = 200, PurchaseCount = 50, ConversionRate = 25.0, AverageRating = 4.5m },
                new PopularProductDto { ProductId = 2, ProductName = "SPF 50 Sunscreen", ViewCount = 450, SearchCount = 180, PurchaseCount = 45, ConversionRate = 25.0, AverageRating = 4.3m }
            };
        }

        // Helper methods
        private QueryContainer BuildSearchQuery(FullTextSearchDto searchDto)
        {
            var queries = new List<QueryContainer>();

            if (!string.IsNullOrEmpty(searchDto.Query))
            {
                var multiMatchQuery = new MultiMatchQuery
                {
                    Query = searchDto.Query,
                    Fields = searchDto.Fields.Select(f => $"{f}^{searchDto.FieldBoosts.GetValueOrDefault(f, 1.0)}").ToArray(),
                    Operator = searchDto.Operator == "and" ? Operator.And : Operator.Or,
                    Fuzziness = searchDto.Fuzzy ? Fuzziness.Auto : Fuzziness.EditDistance(0),
                    MinimumShouldMatch = searchDto.MinimumShouldMatch,
                    MaxExpansions = searchDto.MaxExpansions
                };

                queries.Add(multiMatchQuery);
            }

            return new BoolQuery
            {
                Must = queries.ToArray()
            };
        }

        private HighlightDescriptor<ElasticsearchProductDocument> BuildHighlightDescriptor(FullTextSearchDto searchDto)
        {
            if (!searchDto.Highlight)
                return null;

            return new HighlightDescriptor<ElasticsearchProductDocument>
            {
                Fields = new Dictionary<Field, IHighlightField>
                {
                    ["name"] = new HighlightField { PreTags = new[] { "<em>" }, PostTags = new[] { "</em>" } },
                    ["description"] = new HighlightField { PreTags = new[] { "<em>" }, PostTags = new[] { "</em>" } },
                    ["brand"] = new HighlightField { PreTags = new[] { "<em>" }, PostTags = new[] { "</em>" } }
                }
            };
        }

        private AggregationDictionary BuildAggregations()
        {
            return new AggregationDictionary
            {
                ["categories"] = new TermsAggregation("category") { Field = "category" },
                ["brands"] = new TermsAggregation("brand") { Field = "brand" },
                ["price_ranges"] = new RangeAggregation("price_range")
                {
                    Field = "price",
                    Ranges = new[]
                    {
                        new RangeAggregationRange { From = 0, To = 25 },
                        new RangeAggregationRange { From = 25, To = 50 },
                        new RangeAggregationRange { From = 50, To = 100 },
                        new RangeAggregationRange { From = 100 }
                    }
                }
            };
        }

        private SortDescriptor<ElasticsearchProductDocument> BuildSortDescriptor()
        {
            return new SortDescriptor<ElasticsearchProductDocument>()
                .Field(f => f.Rating, SortOrder.Descending)
                .Field(f => f.ReviewCount, SortOrder.Descending);
        }

        private List<FullTextProductDto> MapToFullTextProducts(IReadOnlyCollection<ElasticsearchProductDocument> documents, IReadOnlyCollection<IHit<ElasticsearchProductDocument>> hits)
        {
            var products = new List<FullTextProductDto>();

            foreach (var hit in hits)
            {
                var product = new FullTextProductDto
                {
                    Id = hit.Source.Id,
                    Name = hit.Source.Name,
                    Description = hit.Source.Description,
                    Price = hit.Source.Price,
                    ImageUrl = hit.Source.ImageUrl,
                    Brand = hit.Source.Brand,
                    Category = hit.Source.Category,
                    Rating = hit.Source.Rating,
                    ReviewCount = hit.Source.ReviewCount,
                    InStock = hit.Source.InStock,
                    Score = hit.Score ?? 0,
                    Highlights = hit.Highlights?.ToDictionary(h => h.Key, h => h.Value.ToList()) ?? new Dictionary<string, List<string>>(),
                    MatchedFields = hit.MatchedQueries?.ToList() ?? new List<string>()
                };

                products.Add(product);
            }

            return products;
        }

        private List<string> ExtractSuggestions(ISearchResponse<ElasticsearchProductDocument> response)
        {
            var suggestions = new List<string>();

            if (response.Suggest != null)
            {
                foreach (var suggest in response.Suggest.Values)
                {
                    foreach (var suggestion in suggest)
                    {
                        suggestions.AddRange(suggestion.Options.Select(o => o.Text));
                    }
                }
            }

            return suggestions.Distinct().ToList();
        }

        private Dictionary<string, object> ExtractAggregations(AggregateDictionary aggregations)
        {
            var result = new Dictionary<string, object>();

            if (aggregations != null)
            {
                foreach (var aggregation in aggregations)
                {
                    if (aggregation.Value is TermsAggregate termsAgg)
                    {
                        result[aggregation.Key] = termsAgg.Buckets.Select(b => new { Key = b.Key, Count = b.DocCount }).ToList();
                    }
                    else if (aggregation.Value is RangeAggregate rangeAgg)
                    {
                        result[aggregation.Key] = rangeAgg.Buckets.Select(b => new { From = b.From, To = b.To, Count = b.DocCount }).ToList();
                    }
                }
            }

            return result;
        }

        private async Task<List<string>> GetPopularSearchesAsync(int limit)
        {
            // Placeholder implementation
            return new List<string> { "moisturizer", "sunscreen", "cleanser", "serum", "toner" };
        }

        private async Task<List<string>> GetRelatedTermsAsync(string query)
        {
            // Placeholder implementation
            return new List<string> { $"{query} cream", $"{query} gel", $"{query} lotion" };
        }
    }

    public class ElasticsearchProductDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool InStock { get; set; }
        public string Ingredients { get; set; }
        public CompletionField Suggest { get; set; }
    }
} 