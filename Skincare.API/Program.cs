using Microsoft.EntityFrameworkCore;
using Skincare.Repositories.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Skincare.API.Middleware;
using System.Text.Json.Serialization;
using Skincare.API.Configurations;
using Microsoft.OpenApi.Models;
using Skincare.Services.Implements;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Skincare.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đọc chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Set the content root path for the application
            builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Content Root Path: {builder.Environment.ContentRootPath}");

            // Verify credential file exists
            var uploadPath = Path.Combine(builder.Environment.ContentRootPath,
                builder.Configuration["FileSettings:UploadPath"]);
            var avatarFolder = Path.Combine(uploadPath,
                            builder.Configuration["FileSettings:AvatarFolder"]);
            var productFolder = Path.Combine(uploadPath,
                            builder.Configuration["FileSettings:ProductFolder"]);

            Console.WriteLine($"Checking upload path: {uploadPath}");
            if (!Directory.Exists(uploadPath))
            {
                Console.WriteLine("Upload path does not exist. Creating...");
                Directory.CreateDirectory(uploadPath);
            }
            Console.WriteLine("Upload path is ready.");


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

            // Add Response Caching services
            services.AddResponseCaching();

            // Register LocalFileService before other services
            services.AddSingleton<FileService>();

            // Đăng ký Repository và Service
            services.AddServices();

            // Memory Cache for Products
            services.AddMemoryCache();

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
                    Version = "3.0.0",
                    Description = "API Documentation for Skincare Project"
                });

                options.OperationFilter<FileUploadOperationFilter>();

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

                // ✅ Thêm hỗ trợ upload file trong Swagger
            });

            // ✅ Đăng ký CORS (cho phép từ frontend localhost:3000)
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",  // Vite dev server mặc định
                            "http://localhost:5174",
                            "http://127.0.0.1:5173",
                            "http://127.0.0.1:5174"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            services.AddAuthorization();

            // Increase the request size limit for file uploads
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
            });
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

            // Serve static files - add this for local file uploads
            app.UseStaticFiles();

            // Configure static files serving
            var uploadPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var avatarPath = Path.Combine(uploadPath, "avatar-images");
            if (!Directory.Exists(avatarPath))
            {
                Directory.CreateDirectory(avatarPath);
            }

            var productPath = Path.Combine(uploadPath, "product-images");
            if (!Directory.Exists(productPath))
            {
                Directory.CreateDirectory(productPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    // Enable CORS for images
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
                    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=600");
                }
            });

            // Áp dụng CORS
            app.UseCors("AllowSpecificOrigins");

            // Add Response Caching middleware before Authentication and Authorization
            app.UseResponseCaching();

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
