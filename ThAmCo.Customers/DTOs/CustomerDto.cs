using System.Collections.Generic;

namespace ThAmCo.Customers.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public AddressDto Address { get; set; }
        public ICollection<OrderDto> Orders { get; set; }
        public decimal TotalFunds { get; set; }
        public bool RequestDelete { get; set; }

    }
}
