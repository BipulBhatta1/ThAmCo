using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Staffs.Controllers;
using ThAmCo.Staffs.Data;
using ThAmCo.Staffs.DTOs;
using ThAmCo.Staffs.Models;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.Models;
using Xunit;

namespace ThAmCo.Staffs.Tests
{
    public class StaffsControllerTests
    {
        private StaffsController CreateController(out StaffDbContext staffDbContext, out CustomerDbContext customerDbContext)
        {
            // Configure unique in-memory databases for isolation
            var staffOptions = new DbContextOptionsBuilder<StaffDbContext>()
                .UseInMemoryDatabase(databaseName: $"StaffTestDb_{System.Guid.NewGuid()}")
                .Options;
            var customerOptions = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: $"CustomerTestDb_{System.Guid.NewGuid()}")
                .Options;

            staffDbContext = new StaffDbContext(staffOptions);
            customerDbContext = new CustomerDbContext(customerOptions);

            // Seed StaffDbContext
            staffDbContext.Staffs.Add(new Staff
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Role = "Admin"
            });
            staffDbContext.SaveChanges();

            // Seed CustomerDbContext
            customerDbContext.Customers.Add(new Customer
            {
                Id = 1,
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Funds = new[]
                {
                    new Fund { Id = 1, Amount = 100 }
                },
                Orders = new[]
                {
                    new Order
                    {
                        Id = 1,
                        ProductId = 1,
                        Product = new Product { Id = 1, Name = "Sample Product", Description = "Sample Description" },
                        OrderDate = System.DateTime.UtcNow
                    }
                }
            });
            customerDbContext.SaveChanges();

            return new StaffsController(customerDbContext, staffDbContext);
        }

        public async Task Register_ShouldAddStaffToDatabase()
        {
            // Arrange
            var controller = CreateController(out var staffDbContext, out var customerDbContext);
            var newStaff = new StaffDto
            {
                Name = "Alice Smith",
                Email = "alice.smith@example.com",
                Role = "Manager"
            };

            // Act
            var result = controller.Register(newStaff);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", ((RedirectToActionResult)result).ActionName);

            var staff = await staffDbContext.Staffs.FirstOrDefaultAsync(s => s.Email == "alice.smith@example.com");
            Assert.NotNull(staff);
            Assert.Equal("Alice Smith", staff.Name);
        }

        [Fact]
        public void Customers_ShouldReturnCustomerList()
        {
            // Arrange
            var controller = CreateController(out var staffDbContext, out var customerDbContext);

            // Act
            var result = controller.Customers();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var customers = Assert.IsAssignableFrom<IList<CustomerDto>>(viewResult.Model);
            Assert.Single(customers);
            Assert.Equal("Jane Doe", customers.First().Name);
        }

        [Fact]
        public void CustomerProfile_ShouldReturnCustomerDetails()
        {
            // Arrange
            var controller = CreateController(out var staffDbContext, out var customerDbContext);

            // Act
            var result = controller.CustomerProfile(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var customer = Assert.IsAssignableFrom<CustomerDto>(viewResult.Model);
            Assert.Equal("Jane Doe", customer.Name);
            Assert.Single(customer.Orders);
            Assert.Equal("Sample Product", customer.Orders.First().ProductName);
        }

        [Fact]
        public void DispatchOrder_ShouldMarkOrderAsDispatched()
        {
            // Arrange
            var controller = CreateController(out var staffDbContext, out var customerDbContext);

            // Act
            var result = controller.DispatchOrder(1);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(staffDbContext.DispatchRecords.Any(d => d.OrderId == 1 && d.IsDispatched));
        }

        [Fact]
        public void DeleteCustomer_ShouldAnonymizeCustomer()
        {
            // Arrange
            var controller = CreateController(out var staffDbContext, out var customerDbContext);

            // Act
            var result = controller.DeleteCustomer(1);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var customer = customerDbContext.Customers.FirstOrDefault(c => c.Id == 1);
            Assert.Equal("Anonymous", customer.Name);
            Assert.Equal("anonymous@deleted.com", customer.Email);
            Assert.False(customer.RequestDelete);
        }
    }
}
