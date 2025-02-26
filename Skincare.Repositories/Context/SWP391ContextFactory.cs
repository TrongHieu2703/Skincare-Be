using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using Skincare.Repositories.Context;

namespace Skincare.Repositories.Context
{
    public class SWP391ContextFactory : IDesignTimeDbContextFactory<SWP391Context>
    {
        public SWP391Context CreateDbContext(string[] args)
        {
            // Giả sử file appsettings.json nằm ở Skincare.API
            // Giả sử Skincare.API và Skincare.Repositories cùng cấp
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Skincare.API");
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var config = builder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SWP391Context>();
            optionsBuilder.UseSqlServer(connectionString);

            return new SWP391Context(optionsBuilder.Options);
        }
    }
}
