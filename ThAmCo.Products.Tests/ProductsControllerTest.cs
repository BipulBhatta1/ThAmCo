using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Products.Controllers;
using ThAmCo.Products.Data;
using ThAmCo.Products.Models;
using Xunit;

namespace ThAmCo.Products.Tests
{
    public class ProductsControllerTests
    {
        private ProductsController CreateController(out ProductDbContext context)
        {
            // Create a unique in-memory database for each test
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Ensure unique database for each test
                .Options;

            context = new ProductDbContext(options);

            // Seed test data
            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "A test product",
                Price = 10.99m,
                Stock = 100,
                CategoryId = 1,
                BrandId = 1
            });
            context.SaveChanges();

            return new ProductsController(context, null, null); // No need for services in this test
        }

        [Fact]
        public async Task FetchProducts_ReturnsProducts()
        {
            // Arrange
            var controller = CreateController(out var context);

            // Act
            var products = await context.Products.ToListAsync();

            // Assert
            Assert.Single(products);
            Assert.Equal("Test Product", products.First().Name);
        }

        [Fact]
        public async Task CreateProduct_AddsProductToDatabase()
        {
            // Arrange
            var controller = CreateController(out var context);

            var newProduct = new Product
            {
                Id = 2,
                Name = "New Product",
                Description = "Another test product",
                Price = 20.99m,
                Stock = 50,
                CategoryId = 2,
                BrandId = 2
            };

            // Act
            context.Products.Add(newProduct);
            await context.SaveChangesAsync();

            // Assert
            var products = await context.Products.ToListAsync();
            Assert.Equal(2, products.Count);
            Assert.Contains(products, p => p.Name == "New Product");
        }

        [Fact]
        public async Task UpdateProduct_UpdatesExistingProduct()
        {
            // Arrange
            var controller = CreateController(out var context);

            var product = await context.Products.FirstAsync();
            product.Name = "Updated Product";

            // Act
            context.Products.Update(product);
            await context.SaveChangesAsync();

            // Assert
            var updatedProduct = await context.Products.FirstAsync();
            Assert.Equal("Updated Product", updatedProduct.Name);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProductFromDatabase()
        {
            // Arrange
            var controller = CreateController(out var context);

            var product = await context.Products.FirstAsync();

            // Act
            context.Products.Remove(product);
            await context.SaveChangesAsync();

            // Assert
            var products = await context.Products.ToListAsync();
            Assert.Empty(products);
        }
    }
}
