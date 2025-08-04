namespace Skincare.Services.Constants
{
    public static class CacheKeys
    {
        // Product related cache keys
        public const string ProductList = "products:list";
        public const string ProductDetail = "products:detail:{0}"; // {0} = productId
        public const string ProductByCategory = "products:category:{0}"; // {0} = categoryId
        public const string ProductSearch = "products:search:{0}"; // {0} = searchTerm
        
        // Category related cache keys
        public const string CategoryList = "categories:list";
        public const string CategoryDetail = "categories:detail:{0}"; // {0} = categoryId
        
        // User related cache keys
        public const string UserProfile = "users:profile:{0}"; // {0} = userId
        public const string UserSession = "users:session:{0}"; // {0} = userId
        
        // Order related cache keys
        public const string UserOrders = "orders:user:{0}"; // {0} = userId
        public const string OrderDetail = "orders:detail:{0}"; // {0} = orderId
        
        // Review related cache keys
        public const string ProductReviews = "reviews:product:{0}"; // {0} = productId
        public const string UserReviews = "reviews:user:{0}"; // {0} = userId
        
        // Dashboard related cache keys
        public const string DashboardStats = "dashboard:stats";
        public const string DashboardSales = "dashboard:sales:{0}"; // {0} = dateRange
        
        // Pattern for cache invalidation
        public const string ProductPattern = "products:*";
        public const string CategoryPattern = "categories:*";
        public const string UserPattern = "users:*";
        public const string OrderPattern = "orders:*";
        public const string ReviewPattern = "reviews:*";
        public const string DashboardPattern = "dashboard:*";
    }
} 