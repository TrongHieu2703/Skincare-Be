using Microsoft.EntityFrameworkCore;
using Skincare.Repositories.Context;
using Skincare.Repositories.Implements;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Implements;
using Skincare.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Skincare.API.Middleware;

namespace Skincare.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔥 1. Đọc chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // 🔥 2. Cấu hình DbContext
            builder.Services.AddDbContext<SWP391Context>(options =>
                options.UseSqlServer(connectionString));

            // 🔥 3. Đăng ký Repository
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();

            // 🔥 4. Đăng ký Service
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ICartService, CartService>();

            // 🔥 5. Thêm Authentication (JWT)
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        //ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            // 🔥 6. Đăng ký Swagger
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 🔥 7. Đăng ký CORS
            var corsPolicyName = "AllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Cho phép React frontend
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // 🔥 8. Middleware xử lý lỗi
            app.UseMiddleware<ExceptionMiddleware>();

            // 🔥 9. Bật Swagger UI trong môi trường Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 🔥 10. Áp dụng CORS
            app.UseCors(corsPolicyName);

            // 🔥 11. Middleware Authentication & Authorization
            app.UseHttpsRedirection(); 
            app.UseAuthentication();
            app.UseAuthorization();

            // 🔥 12. Middleware Chuyển Hướng đến Swagger nếu Truy Cập Root
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/swagger/index.html");
                    return;
                }
                await next();
            });

            // 🔥 13. Định tuyến API
            app.MapControllers();

            // 🔥 14. Chạy Ứng Dụng
            app.Run();
        }
    }
}
