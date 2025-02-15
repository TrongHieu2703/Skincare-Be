
using Microsoft.EntityFrameworkCore;
using Skincare.Repositories.Context;

namespace Skincare.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //Add dbcontext
            builder.Services.AddDbContext<SWP391Context>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Thêm chính sách CORS
            var corsPolicyName = "AllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Chỉ định các nguồn được phép truy cập
                          .AllowAnyMethod()  // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE,...)
                          .AllowAnyHeader()  // Cho phép tất cả các headers
                          .AllowCredentials(); // Cho phép gửi cookies và headers Authorization
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(corsPolicyName);
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
