using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Products.Data;
using ThAmCo.Products.DTOs;
using ThAmCo.Products.Models;
using ThAmCo.Products.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Products.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly UnderCuttersService _underCuttersService;
        private readonly DodgyDealersService _dodgyDealersService;

        public ProductsController(ProductDbContext context, UnderCuttersService underCuttersService, DodgyDealersService dodgyDealersService)
        {
            _context = context;
            _underCuttersService = underCuttersService;
            _dodgyDealersService = dodgyDealersService;
        }

        // UNDERCUTTERS METHODS

        // UNDERCUTTERS: FETCH ORDER BY ID
        [HttpGet("UnderCutters/Orders/{id}")]
        public async Task<IActionResult> FetchOrderByIdFromUnderCutters(int id)
        {
            var orderDto = await FetchOrderById(_underCuttersService, id);
            if (orderDto == null) return NotFound(new { Message = "Order not found in UnderCutters." });
            return Ok(orderDto);
        }

        // UNDERCUTTERS: CREATE ORDER
        [HttpPost("UnderCutters/Orders")]
        public async Task<IActionResult> CreateOrderInUnderCutters([FromBody] OrderDto orderDto)
        {
            var success = await CreateOrder(_underCuttersService, orderDto);
            if (!success) return StatusCode(500, new { Message = "Failed to create order in UnderCutters." });
            return Ok(new { Message = "Order created successfully in UnderCutters." });
        }

        // UNDERCUTTERS: DELETE ORDER
        [HttpDelete("UnderCutters/Orders/{id}")]
        public async Task<IActionResult> DeleteOrderFromUnderCutters(int id)
        {
            var success = await DeleteOrder(_underCuttersService, id);
            if (!success) return StatusCode(500, new { Message = "Failed to delete order from UnderCutters." });
            return Ok(new { Message = "Order deleted successfully from UnderCutters." });
        }

        [HttpGet("UnderCutters/FetchProducts")]
        public async Task<IActionResult> FetchProductsFromUnderCutters()
        {
            await FetchAndSaveProductsFromUnderCutters();
            return Ok(new { Message = "Products fetched and saved from UnderCutters successfully." });
        }

        [HttpGet("UnderCutters/FetchCategories")]
        public async Task<IActionResult> FetchCategoriesFromUnderCutters()
        {
            await FetchAndSaveCategoriesFromUnderCutters();
            return Ok(new { Message = "Categories fetched and saved from UnderCutters successfully." });
        }

        [HttpGet("UnderCutters/FetchBrands")]
        public async Task<IActionResult> FetchBrandsFromUnderCutters()
        {
            await FetchAndSaveBrandsFromUnderCutters();
            return Ok(new { Message = "Brands fetched and saved from UnderCutters successfully." });
        }

        // DODGY DEALERS METHODS

        // DODGY DEALERS: FETCH ORDER BY ID
        [HttpGet("DodgyDealers/Orders/{id}")]
        public async Task<IActionResult> FetchOrderByIdFromDodgyDealers(int id)
        {
            var orderDto = await FetchOrderById(_dodgyDealersService, id);
            if (orderDto == null) return NotFound(new { Message = "Order not found in DodgyDealers." });
            return Ok(orderDto);
        }

        // DODGY DEALERS: CREATE ORDER
        [HttpPost("DodgyDealers/Orders")]
        public async Task<IActionResult> CreateOrderInDodgyDealers([FromBody] OrderDto orderDto)
        {
            var success = await CreateOrder(_dodgyDealersService, orderDto);
            if (!success) return StatusCode(500, new { Message = "Failed to create order in DodgyDealers." });
            return Ok(new { Message = "Order created successfully in DodgyDealers." });
        }

        // DODGY DEALERS: DELETE ORDER
        [HttpDelete("DodgyDealers/Orders/{id}")]
        public async Task<IActionResult> DeleteOrderFromDodgyDealers(int id)
        {
            var success = await DeleteOrder(_dodgyDealersService, id);
            if (!success) return StatusCode(500, new { Message = "Failed to delete order from DodgyDealers." });
            return Ok(new { Message = "Order deleted successfully from DodgyDealers." });
        }

        [HttpGet("DodgyDealers/FetchProducts")]
        public async Task<IActionResult> FetchProductsFromDodgyDealers()
        {
            await FetchAndSaveProductsFromDodgyDealers();
            return Ok(new { Message = "Products fetched and saved from DodgyDealers successfully." });
        }

        [HttpGet("DodgyDealers/FetchCategories")]
        public async Task<IActionResult> FetchCategoriesFromDodgyDealers()
        {
            await FetchAndSaveCategoriesFromDodgyDealers();
            return Ok(new { Message = "Categories fetched and saved from DodgyDealers successfully." });
        }

        [HttpGet("DodgyDealers/FetchBrands")]
        public async Task<IActionResult> FetchBrandsFromDodgyDealers()
        {
            await FetchAndSaveBrandsFromDodgyDealers();
            return Ok(new { Message = "Brands fetched and saved from DodgyDealers successfully." });
        }

        // HELPER METHODS FOR UNDERCUTTERS

        private async Task FetchAndSaveProductsFromUnderCutters()
        {
            var products = await _underCuttersService.FetchProductsAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products ON");

                foreach (var productDto in products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId
                }))
                {
                    if (!_context.Products.Any(p => p.Id == productDto.Id))
                    {
                        var product = new Product
                        {
                            Id = productDto.Id,
                            Name = productDto.Name,
                            Description = productDto.Description,
                            Price = productDto.Price,
                            Stock = productDto.Stock,
                            CategoryId = productDto.CategoryId,
                            BrandId = productDto.BrandId
                        };
                        _context.Products.Add(product);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task FetchAndSaveCategoriesFromUnderCutters()
        {
            var categories = await _underCuttersService.FetchCategoriesAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON");

                foreach (var categoryDto in categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }))
                {
                    if (!_context.Categories.Any(c => c.Id == categoryDto.Id))
                    {
                        var category = new Category
                        {
                            Id = categoryDto.Id,
                            Name = categoryDto.Name
                        };
                        _context.Categories.Add(category);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task FetchAndSaveBrandsFromUnderCutters()
        {
            var brands = await _underCuttersService.FetchBrandsAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands ON");

                foreach (var brandDto in brands.Select(b => new BrandDto { Id = b.Id, Name = b.Name }))
                {
                    if (!_context.Brands.Any(b => b.Id == brandDto.Id))
                    {
                        var brand = new Brand
                        {
                            Id = brandDto.Id,
                            Name = brandDto.Name
                        };
                        _context.Brands.Add(brand);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // HELPER METHODS FOR DODGY DEALERS

        private async Task FetchAndSaveProductsFromDodgyDealers()
        {
            var products = await _dodgyDealersService.FetchProductsAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products ON");

                foreach (var productDto in products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId
                }))
                {
                    if (!_context.Products.Any(p => p.Id == productDto.Id))
                    {
                        var product = new Product
                        {
                            Id = productDto.Id,
                            Name = productDto.Name,
                            Description = productDto.Description,
                            Price = productDto.Price,
                            Stock = productDto.Stock,
                            CategoryId = productDto.CategoryId,
                            BrandId = productDto.BrandId
                        };
                        _context.Products.Add(product);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task FetchAndSaveCategoriesFromDodgyDealers()
        {
            var categories = await _dodgyDealersService.FetchCategoriesAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON");

                foreach (var categoryDto in categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }))
                {
                    if (!_context.Categories.Any(c => c.Id == categoryDto.Id))
                    {
                        var category = new Category
                        {
                            Id = categoryDto.Id,
                            Name = categoryDto.Name
                        };
                        _context.Categories.Add(category);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task FetchAndSaveBrandsFromDodgyDealers()
        {
            var brands = await _dodgyDealersService.FetchBrandsAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands ON");

                foreach (var brandDto in brands.Select(b => new BrandDto { Id = b.Id, Name = b.Name }))
                {
                    if (!_context.Brands.Any(b => b.Id == brandDto.Id))
                    {
                        var brand = new Brand
                        {
                            Id = brandDto.Id,
                            Name = brandDto.Name
                        };
                        _context.Brands.Add(brand);
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Brands OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<OrderDto> FetchOrderById<TService>(TService service, int id)
            where TService : class
        {
            var order = service switch
            {
                UnderCuttersService underCutters => await underCutters.FetchOrderByIdAsync(id),
                DodgyDealersService dodgyDealers => await dodgyDealers.FetchOrderByIdAsync(id),
                _ => null
            };

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                ProductId = order.ProductId,
                OrderDate = order.OrderDate
            };
        }

        private async Task<bool> CreateOrder<TService>(TService service, OrderDto orderDto)
            where TService : class
        {
            var order = new Order
            {
                Id = orderDto.Id,
                ProductId = orderDto.ProductId,
                OrderDate = orderDto.OrderDate
            };

            return service switch
            {
                UnderCuttersService underCutters => await underCutters.CreateOrderAsync(order),
                DodgyDealersService dodgyDealers => await dodgyDealers.CreateOrderAsync(order),
                _ => false
            };
        }

        private async Task<bool> DeleteOrder<TService>(TService service, int id)
            where TService : class
        {
            return service switch
            {
                UnderCuttersService underCutters => await underCutters.DeleteOrderAsync(id),
                DodgyDealersService dodgyDealers => await dodgyDealers.DeleteOrderAsync(id),
                _ => false
            };
        }

    }
}
