using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class FullTextSearchDto
    {
        public string Query { get; set; }
        public List<string> Fields { get; set; } = new List<string> { "name", "description", "brand", "ingredients" };
        public string Operator { get; set; } = "or"; // "and", "or"
        public bool Fuzzy { get; set; } = true;
        public int Fuzziness { get; set; } = 2;
        public bool Highlight { get; set; } = true;
        public int MaxExpansions { get; set; } = 50;
        public string MinimumShouldMatch { get; set; } = "75%";
        public List<string> ExcludeFields { get; set; } = new List<string>();
        public Dictionary<string, double> FieldBoosts { get; set; } = new Dictionary<string, double>
        {
            { "name", 3.0 },
            { "brand", 2.0 },
            { "description", 1.0 },
            { "ingredients", 0.5 }
        };
    }

    public class FullTextSearchResultDto
    {
        public int TotalHits { get; set; }
        public double MaxScore { get; set; }
        public double Took { get; set; }
        public List<FullTextProductDto> Products { get; set; } = new List<FullTextProductDto>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public Dictionary<string, object> Aggregations { get; set; } = new Dictionary<string, object>();
    }

    public class FullTextProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool InStock { get; set; }
        public double Score { get; set; }
        public Dictionary<string, List<string>> Highlights { get; set; } = new Dictionary<string, List<string>>();
        public List<string> MatchedFields { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class SearchSuggestionDto
    {
        public string Text { get; set; }
        public double Score { get; set; }
        public int Frequency { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> RelatedTerms { get; set; } = new List<string>();
    }

    public class SearchAggregationDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<AggregationBucketDto> Buckets { get; set; } = new List<AggregationBucketDto>();
    }

    public class AggregationBucketDto
    {
        public string Key { get; set; }
        public long Count { get; set; }
        public double? Score { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
} 