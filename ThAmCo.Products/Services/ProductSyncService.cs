using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThAmCo.Products.Data;
using ThAmCo.Products.Models;

namespace ThAmCo.Products.Services
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
            // Call immediately and then every 5 minutes
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("ProductSyncService is running at {Time}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var underCuttersService = scope.ServiceProvider.GetRequiredService<UnderCuttersService>();
            var dodgyDealersService = scope.ServiceProvider.GetRequiredService<DodgyDealersService>();

            try
            {
                _logger.LogInformation("Starting sync from UnderCutters and DodgyDealers.");

                // Sync from UnderCutters
                await FetchAndSaveFromUnderCutters(dbContext, underCuttersService);

                // Sync from DodgyDealers
                await FetchAndSaveFromDodgyDealers(dbContext, dodgyDealersService);

                _logger.LogInformation("Sync completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the sync process.");
            }
        }

        private async Task FetchAndSaveFromUnderCutters(ProductDbContext dbContext, UnderCuttersService service)
        {
            _logger.LogInformation("Fetching data from UnderCutters.");

            // Fetch and save categories
            var categories = await service.FetchCategoriesAsync();
            foreach (var category in categories)
            {
                if (!dbContext.Categories.Any(c => c.Id == category.Id))
                {
                    dbContext.Categories.Add(new Category { Id = category.Id, Name = category.Name });
                }
            }

            // Fetch and save brands
            var brands = await service.FetchBrandsAsync();
            foreach (var brand in brands)
            {
                if (!dbContext.Brands.Any(b => b.Id == brand.Id))
                {
                    dbContext.Brands.Add(new Brand { Id = brand.Id, Name = brand.Name });
                }
            }

            await dbContext.SaveChangesAsync();

            // Fetch and save products
            var products = await service.FetchProductsAsync();
            foreach (var product in products)
            {
                if (!dbContext.Products.Any(p => p.Id == product.Id))
                {
                    dbContext.Products.Add(new Product
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

            await dbContext.SaveChangesAsync();
        }

        private async Task FetchAndSaveFromDodgyDealers(ProductDbContext dbContext, DodgyDealersService service)
        {
            _logger.LogInformation("Fetching data from DodgyDealers.");

            // Fetch and save categories
            var categories = await service.FetchCategoriesAsync();
            foreach (var category in categories)
            {
                if (!dbContext.Categories.Any(c => c.Id == category.Id))
                {
                    dbContext.Categories.Add(new Category { Id = category.Id, Name = category.Name });
                }
            }

            // Fetch and save brands
            var brands = await service.FetchBrandsAsync();
            foreach (var brand in brands)
            {
                if (!dbContext.Brands.Any(b => b.Id == brand.Id))
                {
                    dbContext.Brands.Add(new Brand { Id = brand.Id, Name = brand.Name });
                }
            }

            await dbContext.SaveChangesAsync();

            // Fetch and save products
            var products = await service.FetchProductsAsync();
            foreach (var product in products)
            {
                if (!dbContext.Products.Any(p => p.Id == product.Id))
                {
                    dbContext.Products.Add(new Product
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

            await dbContext.SaveChangesAsync();
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
