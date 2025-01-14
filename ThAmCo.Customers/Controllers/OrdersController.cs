using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ThAmCo.Customers.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly CustomerDbContext _context;

        public OrdersController(CustomerDbContext context)
        {
            _context = context;
        }

        // Place an Order
        public IActionResult Create(int productId)
        {
            var order = new Order
            {
                ProductId = productId,
                OrderDate = DateTime.UtcNow
            };
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Funds).FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null) return Unauthorized();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == order.ProductId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "The product does not exist.";
                return RedirectToAction("OutOfStock");
            }

            if (product.Stock <= 0)
            {
                TempData["ErrorMessage"] = "The product is out of stock.";
                return RedirectToAction("OutOfStock");
            }

            // Deduct the stock
            product.Stock--;

            // Calculate total funds available
            var totalFunds = customer.Funds.Sum(f => f.Amount);

            if (totalFunds < product.Price)
            {
                TempData["ErrorMessage"] = "Insufficient funds to place the order.";
                return RedirectToAction("OutOfStock");
            }

            // Deduct the product price from available funds
            foreach (var fund in customer.Funds)
            {
                if (product.Price <= 0) break;
                var deduction = Math.Min(product.Price, fund.Amount);
                fund.Amount -= deduction;
                product.Price -= deduction;
            }

            order.CustomerId = customer.Id;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("OrderSuccess");
        }

        // Success Page
        public IActionResult OrderSuccess()
        {
            ViewData["Message"] = TempData["SuccessMessage"];
            return View();
        }

        // Out of Stock Page
        public IActionResult OutOfStock()
        {
            ViewData["Message"] = TempData["ErrorMessage"];
            return View();
        }
    }
}