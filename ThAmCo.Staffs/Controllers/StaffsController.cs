using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Staffs.Data;
using ThAmCo.Staffs.DTOs;
using ThAmCo.Staffs.Models;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.Models;

namespace ThAmCo.Staffs.Controllers
{
    [Authorize]
    public class StaffsController : Controller
    {
        private readonly StaffDbContext _staffDbContext;
        private readonly CustomerDbContext _customerDbContext;

        public StaffsController(CustomerDbContext customerDbContext, StaffDbContext staffDbContext)
        {
            _staffDbContext = staffDbContext;
            _customerDbContext = customerDbContext;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            var staffEmail = User.Identity?.Name;
            var staff = _staffDbContext.Staffs.FirstOrDefault(s => s.Email == staffEmail);

            if (staff == null)
                return RedirectToAction("Register");

            return View();
        }

        // Staff Registration Portal
        public IActionResult Register()
        {
            var model = new StaffDto
            {
                Email = User.Identity?.Name
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(StaffDto staffDto)
        {
            if (!ModelState.IsValid) return View(staffDto);

            // Check if staff is already registered
            if (_staffDbContext.Staffs.Any(s => s.Email == staffDto.Email))
            {
                ModelState.AddModelError("", "This email is already registered.");
                return View(staffDto);
            }

            var staff = new Staff
            {
                Name = staffDto.Name,
                Email = staffDto.Email,
                Role = staffDto.Role
            };

            _staffDbContext.Staffs.Add(staff);
            _staffDbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // List Customers
        public IActionResult Customers()
        {
            var customers = _customerDbContext.Customers
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Funds = c.Funds.Sum(f => f.Amount),
                    RequestDelete = c.RequestDelete
                }).ToList();

            return View(customers);
        }

        // View Customer Profile
        public IActionResult CustomerProfile(int customerId)
        {
            var customer = _customerDbContext.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.Product)
                .FirstOrDefault(c => c.Id == customerId);

            if (customer == null) return NotFound();

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Funds = customer.Funds?.Sum(f => f.Amount) ?? 0, // Handle null Funds
                RequestDelete = customer.RequestDelete,
                Orders = customer.Orders?.Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = o.Product?.Name ?? "Unknown", // Handle null Product
                    ProductDescription = o.Product?.Description ?? "No description",
                    OrderDate = o.OrderDate,
                    IsDispatched = _staffDbContext.DispatchRecords.Any(d => d.OrderId == o.Id && d.IsDispatched)
                }).ToList() ?? new List<OrderDto>() // Handle null Orders
            };

            return View(customerDto);
        }


        // Mark Order as Dispatched
        [HttpPost]
        public IActionResult DispatchOrder(int orderId)
        {
            if (_staffDbContext.DispatchRecords.Any(d => d.OrderId == orderId && d.IsDispatched))
                return BadRequest("Order is already dispatched.");

            _staffDbContext.DispatchRecords.Add(new DispatchRecord
            {
                OrderId = orderId,
                IsDispatched = true,
                DispatchedAt = DateTime.UtcNow
            });

            _staffDbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        // Delete Customer Profile
        [HttpPost]
        public IActionResult DeleteCustomer(int customerId)
        {
            var customer = _customerDbContext.Customers.Find(customerId);
            if (customer == null) return NotFound();

            customer.Name = "Anonymous";
            customer.Email = "anonymous@deleted.com";
            customer.RequestDelete = false;

            _customerDbContext.SaveChanges();
            return RedirectToAction("Customers");
        }


    }
}
