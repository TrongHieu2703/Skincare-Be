// DependencyInjection.cs
using Skincare.Repositories.Implements;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Implements;
using Skincare.Services.Interfaces;

namespace Skincare.API.Configurations
{
    public static class DependencyInjection
    {
        public static void AddServices(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IFaqService, FaqService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBlogService, BlogService>();
            





            // Register repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();


            // === Đăng ký IEmailService (bạn thêm dòng này!) ===
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}