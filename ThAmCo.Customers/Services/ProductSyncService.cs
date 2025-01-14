using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Customers.Data;
using CustomerCategory = ThAmCo.Customers.Models.Category;
using CustomerBrand = ThAmCo.Customers.Models.Brand;
using CustomerProduct = ThAmCo.Customers.Models.Product;
using ProductDbContext = ThAmCo.Products.Data.ProductDbContext;

namespace ThAmCo.Customers.Services
{
    public class ProductSyncService : IHostedService, IDisposable
    {
        private readonly ILogger<ProductSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public ProductSyncService(IServiceProvider serviceProvider, ILogger<ProductSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductSyncService is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("ProductSyncService is running at {Time}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var productsDbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var customersDbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

            try
            {
                // Test database connection
                await productsDbContext.Database.CanConnectAsync();
                await customersDbContext.Database.CanConnectAsync();

                // Sync Categories
                await SyncCategories(productsDbContext, customersDbContext);

                // Sync Brands
                await SyncBrands(productsDbContext, customersDbContext);

                // Sync Products
                await SyncProducts(productsDbContext, customersDbContext);

                _logger.LogInformation("ProductSyncService completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the sync process.");
            }
        }

        private async Task SyncCategories(ProductDbContext productsDbContext, CustomerDbContext customersDbContext)
        {
            var categories = await productsDbContext.Categories.ToListAsync();

            using var transaction = await customersDbContext.Database.BeginTransactionAsync();
            try
            {
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON");

                foreach (var category in categories)
                {
                    if (!customersDbContext.Categories.Any(c => c.Id == category.Id))
                    {
                        customersDbContext.Categories.Add(new CustomerCategory
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }
                }

                await customersDbContext.SaveChangesAsync();
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SyncBrands(ProductDbContext productsDbContext, CustomerDbContext customersDbContext)
        {
            var brands = await productsDbContext.Brands.ToListAsync();

            using var transaction = await customersDbContext.Database.BeginTransactionAsync();
            try
            {
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands ON");

                foreach (var brand in brands)
                {
                    if (!customersDbContext.Brands.Any(b => b.Id == brand.Id))
                    {
                        customersDbContext.Brands.Add(new CustomerBrand
                        {
                            Id = brand.Id,
                            Name = brand.Name
                        });
                    }
                }

                await customersDbContext.SaveChangesAsync();
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands OFF");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SyncProducts(ProductDbContext productsDbContext, CustomerDbContext customersDbContext)
        {
            var products = await productsDbContext.Products.ToListAsync();

            using var transaction = await customersDbContext.Database.BeginTransactionAsync();
            try
            {
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products ON");

                foreach (var product in products)
                {
                    if (!customersDbContext.Products.Any(p => p.Id == product.Id))
                    {
                        customersDbContext.Products.Add(new CustomerProduct
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            Stock = product.Stock,
                            CategoryId = product.CategoryId,
                            BrandId = product.BrandId
                        });
                    }
                }

                await customersDbContext.SaveChangesAsync();
                await customersDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products OFF");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductSyncService is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
