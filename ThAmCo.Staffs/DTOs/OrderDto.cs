namespace ThAmCo.Staffs.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Reference to the product
        public string ProductName { get; set; } // Product name
        public string ProductDescription { get; set; } // Product description
        public DateTime OrderDate { get; set; } // When the order was placed
        public bool IsDispatched { get; set; } // Whether the order was dispatched
    }
}
