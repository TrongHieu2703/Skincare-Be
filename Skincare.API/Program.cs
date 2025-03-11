using Microsoft.EntityFrameworkCore;
using Skincare.Repositories.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Skincare.API.Middleware;
using System.Text.Json.Serialization;
using Skincare.API.Configurations;
using Microsoft.OpenApi.Models;

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

            // Đọc cấu hình JWT từ appsettings.json
            var jwtSettings = configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"];
            var jwtIssuer = jwtSettings["Issuer"];
            var jwtAudience = jwtSettings["Audience"];

            // ✅ Thêm Authentication (JWT)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Không cho phép thời gian trễ
                };
            });

            // Đăng ký Controllers và Swagger với cấu hình JSON
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddEndpointsApiExplorer();

            // ✅ Cấu hình Swagger với Bearer Token
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Skincare API",
                    Version = "v1",
                    Description = "API Documentation for Skincare Project"
                });

                // Cấu hình Bearer trong Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT Token theo định dạng: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ✅ Đăng ký CORS (cho phép từ frontend localhost:3000)
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:3000") // Thay đổi domain nếu cần
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            services.AddAuthorization();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Middleware xử lý lỗi toàn cục
            app.UseMiddleware<ExceptionMiddleware>();

            // Bật Swagger UI trong môi trường Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skincare API v1");
                    c.RoutePrefix = string.Empty; // Để Swagger hiển thị tại root "/"
                });
            }

            // Áp dụng CORS
            app.UseCors("AllowSpecificOrigins");

            // Middleware Authentication & Authorization
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Middleware chuyển hướng về Swagger nếu truy cập root
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
