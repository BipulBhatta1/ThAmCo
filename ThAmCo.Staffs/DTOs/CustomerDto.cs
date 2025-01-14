namespace ThAmCo.Staffs.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Funds { get; set; } // Total funds
        public bool RequestDelete { get; set; } // Whether the customer requested deletion
        public List<OrderDto> Orders { get; set; } // List of orders for the customer
    }
}
