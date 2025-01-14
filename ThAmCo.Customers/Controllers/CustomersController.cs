using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.DTOs;
using ThAmCo.Customers.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ThAmCo.Customers.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly CustomerDbContext _context;

        public CustomersController(CustomerDbContext context)
        {
            _context = context;
        }

        // Register
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CustomerDto customerDto)
        {
            if (await _context.Customers.AnyAsync(c => c.Email == customerDto.Email))
            {
                ViewData["ErrorMessage"] = "An account with this email already exists.";
                return View(customerDto);
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                Address = new Address
                {
                    Street = customerDto.Address?.Street,
                    City = customerDto.Address?.City,
                    State = customerDto.Address?.State,
                    PostalCode = customerDto.Address?.PostalCode,
                    Country = customerDto.Address?.Country
                }
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }

        // Profile
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Address)
                                                .Include(c => c.Orders)
                                                .Include(c => c.Funds)
                                                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null) return NotFound();

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = new AddressDto
                {
                    Street = customer.Address?.Street,
                    City = customer.Address?.City,
                    State = customer.Address?.State,
                    PostalCode = customer.Address?.PostalCode,
                    Country = customer.Address?.Country
                },
                Orders = customer.Orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    OrderDate = o.OrderDate
                }).ToList(),
                TotalFunds = customer.Funds.Sum(f => f.Amount)
            };

            return View(customerDto);
        }

        // Edit Profile
        public async Task<IActionResult> Edit()
        {
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Address).FirstOrDefaultAsync(c => c.Email == userEmail);
            if (customer == null) return NotFound();

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = new AddressDto
                {
                    Street = customer.Address?.Street,
                    City = customer.Address?.City,
                    State = customer.Address?.State,
                    PostalCode = customer.Address?.PostalCode,
                    Country = customer.Address?.Country
                }
            };

            return View(customerDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerDto customerDto)
        {
            if (!ModelState.IsValid) return View(customerDto);

            var customer = await _context.Customers.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == customerDto.Id);
            if (customer == null) return NotFound();

            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;

            if (customer.Address == null)
            {
                customer.Address = new Address();
            }

            customer.Address.Street = customerDto.Address?.Street;
            customer.Address.City = customerDto.Address?.City;
            customer.Address.State = customerDto.Address?.State;
            customer.Address.PostalCode = customerDto.Address?.PostalCode;
            customer.Address.Country = customerDto.Address?.Country;

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile");
        }

        // Order History
        public async Task<IActionResult> OrderHistory()
        {
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Orders)
                                                   .ThenInclude(o => o.Product)
                                                   .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null) return NotFound();
            return View(customer.Orders);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRequestDelete()
        {
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null) return Unauthorized();

            // Toggle the RequestDelete property
            customer.RequestDelete = !customer.RequestDelete;
            await _context.SaveChangesAsync();

            // Prepare the CustomerDto for the view
            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address == null ? null : new AddressDto
                {
                    Street = customer.Address.Street,
                    City = customer.Address.City,
                    State = customer.Address.State,
                    PostalCode = customer.Address.PostalCode,
                    Country = customer.Address.Country
                },
                Orders = customer.Orders?.Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    OrderDate = o.OrderDate
                }).ToList(),
                RequestDelete = customer.RequestDelete
            };

            TempData["Message"] = customer.RequestDelete
                ? "Your deletion request has been submitted."
                : "Your deletion request has been cancelled.";

            return View("Edit", customerDto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFund(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Fund amount must be greater than zero.";
                return RedirectToAction("Profile");
            }

            var userEmail = User.Identity?.Name;
            var customer = await _context.Customers.Include(c => c.Funds).FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null) return NotFound();

            // Add the fund
            var fund = new Fund
            {
                Amount = amount,
                CustomerId = customer.Id
            };

            _context.Funds.Add(fund);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Funds added successfully!";
            return RedirectToAction("Profile");
        }



    }
}
