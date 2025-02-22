using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Skincare.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all products.");
                // Simulate fetching all products
                await Task.Delay(500); // Simulate async work

                // Return dummy products
                return new List<Product>
                {
                    new Product { Id = 1, Name = "Product 1", Price = 10.00m, Description = "Description 1", IsAvailable = true },
                    new Product { Id = 2, Name = "Product 2", Price = 20.00m, Description = "Description 2", IsAvailable = true }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all products.");
                throw;
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching product with ID: {id}");
                // Simulate fetching a product by ID
                await Task.Delay(500); // Simulate async work

                // Return a dummy product
                return new Product { Id = id, Name = "Product", Price = 10.00m, Description = "Description", IsAvailable = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching product with ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId)
        {
            try
            {
                _logger.LogInformation($"Fetching products for type ID: {productTypeId}");
                // Simulate fetching products by type
                await Task.Delay(500); // Simulate async work

                // Return dummy products
                return new List<Product>
                {
                    new Product { Id = 1, Name = "Product 1", Price = 10.00m, Description = "Description 1", IsAvailable = true, ProductTypeId = productTypeId },
                    new Product { Id = 2, Name = "Product 2", Price = 20.00m, Description = "Description 2", IsAvailable = true, ProductTypeId = productTypeId }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching products for type ID: {productTypeId}");
                throw;
            }
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            try
            {
                _logger.LogInformation($"Creating product: {product.Name}");
                // Simulate creating a product
                await Task.Delay(500); // Simulate async work

                // Return the created product
                product.Id = new Random().Next(1, 1000); // Simulate ID generation
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating product: {product.Name}");
                throw;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            try
            {
                _logger.LogInformation($"Updating product: {product.Name}");
                // Simulate updating a product
                await Task.Delay(500); // Simulate async work
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating product: {product.Name}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting product with ID: {id}");
                // Simulate deleting a product
                await Task.Delay(500); // Simulate async work
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting product with ID: {id}");
                throw;
            }
        }
    }
}