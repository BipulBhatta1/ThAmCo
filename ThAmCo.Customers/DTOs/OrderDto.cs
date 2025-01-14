using System;

namespace ThAmCo.Customers.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int ProductId { get; set; }
        public ProductDto Product { get; set; }
    }
}
