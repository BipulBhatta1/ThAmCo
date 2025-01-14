using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ThAmCo.Customers.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly CustomerDbContext _context;

        public ProductsController(CustomerDbContext context)
        {
            _context = context;
        }

        // Browse Products
        public async Task<IActionResult> Index(string searchQuery, int? categoryId, int? brandId, string sortOrder)
        {
            var query = _context.Products.AsQueryable();

            // Search by name
            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(p => p.Name.Contains(searchQuery));

            // Filter by category
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // Filter by brand
            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            // Sort by price
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query
            };

            var products = await query.ToListAsync();

            // Populate filters and sorting data
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewData["CurrentSearchQuery"] = searchQuery;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CurrentBrandId"] = brandId;
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["IsGuestUser"] = !User.Identity.IsAuthenticated;

            return View(products);
        }

        // Product Details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["IsGuestUser"] = !User.Identity!.IsAuthenticated;
            return View("detail", product);  // Explicitly specify the view name
        }

        // Place Order with Eligibility Check
        [HttpPost]
        public IActionResult CheckOrderEligibility()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect guest users to login
                return RedirectToAction("Index", "Home");
            }

            var userEmail = User.Identity.Name;
            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);

            if (customer == null)
            {
                // Redirect logged-in users without registration to register
                return RedirectToAction("Register", "Customers");
            }

            return RedirectToAction("Index", "Products");
        }

        // Place Order
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(int productId)
        {
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Funds).FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
                return Unauthorized();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            // Calculate total funds available
            var totalFunds = customer.Funds.Sum(f => f.Amount);

            // Check if the customer has sufficient funds
            if (totalFunds < product.Price)
            {
                TempData["ErrorMessage"] = "Insufficient funds to place the order.";
                return RedirectToAction("Details", new { id = productId });
            }

            // Deduct the product price from available funds
            foreach (var fund in customer.Funds)
            {
                if (product.Price <= 0) break;
                var deduction = Math.Min(product.Price, fund.Amount);
                fund.Amount -= deduction;
                product.Price -= deduction;
            }

            // Create a new order
            var order = new Order
            {
                CustomerId = customer.Id,
                ProductId = product.Id,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}
