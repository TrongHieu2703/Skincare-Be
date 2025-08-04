// DependencyInjection.cs
using Skincare.Repositories.Implements;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Implements;
using Skincare.Services.Interfaces;
using Skincare.Services.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace Skincare.API.Configurations
{
    public static class DependencyInjection
    {
        public static void AddServices(this IServiceCollection services)
        {
            // Register Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductTypeService, ProductTypeService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ISkinTypeService, SkinTypeService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IVoucherService, VoucherService>();
            
            // Register Search Service
            services.AddScoped<ISearchService, ElasticsearchService>();
            
            // Register Notification Services
            services.AddScoped<INotificationService, EmailService>();
            services.AddScoped<SignalRNotificationService>();
            
            // Register Analytics Service
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            
            // Register Logging Service
            services.AddScoped<ILoggingService, LoggingService>();
            
            // Register Repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
            services.AddScoped<IProductSkinTypeRepository, ProductSkinTypeRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ISkinTypeRepository, SkinTypeRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            
            // Register Search Repository
            services.AddScoped<ISearchRepository, SearchRepository>();

            // Register FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            
            // Register Validators
            services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
            services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();
            services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
            services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
            services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();
        }
    }
}