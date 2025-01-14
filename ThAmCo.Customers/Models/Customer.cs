using System.Collections.Generic;

namespace ThAmCo.Customers.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public Address Address { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Fund> Funds { get; set; }
        public bool RequestDelete { get; set; } = false;

    }
}
