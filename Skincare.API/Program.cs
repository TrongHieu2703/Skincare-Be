using Microsoft.EntityFrameworkCore;
using Skincare.Repositories.Context;
using Skincare.Repositories.Implements;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Implements;
using Skincare.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Skincare.API.Middleware;
using System.Text.Json.Serialization;
using Skincare.API.Configurations;

namespace Skincare.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đọc chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Cấu hình các dịch vụ
            ConfigureServices(builder.Services, connectionString, builder.Configuration);

            var app = builder.Build();

            // Cấu hình middleware
            ConfigureMiddleware(app);

            // Chạy ứng dụng
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            // Cấu hình DbContext
            services.AddDbContext<SWP391Context>(options =>
                options.UseSqlServer(connectionString));

            // Đăng ký Repository và Service
            services.AddServices();

            // Thêm Authentication (JWT)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                    };
                });

            // Đăng ký Controllers và Swagger với JWT cấu hình
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Skincare API",
                    Version = "v1"
                });

                // Thêm cấu hình Bearer Token vào Swagger
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer prefix in this field",
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // Đăng ký CORS
            var corsPolicyName = "AllowSpecificOrigins";
            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Cho phép React frontend
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            services.AddAuthorization();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Middleware xử lý lỗi
            app.UseMiddleware<ExceptionMiddleware>();

            // Bật Swagger UI trong môi trường Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Áp dụng CORS
            app.UseCors("AllowSpecificOrigins");

            // Middleware Authentication & Authorization
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Middleware Chuyển Hướng đến Swagger nếu Truy Cập Root
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/swagger/index.html");
                    return;
                }
                await next();
            });

            // Định tuyến API
            app.MapControllers();
        }
    }
}