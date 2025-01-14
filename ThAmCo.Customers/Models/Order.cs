using System;

namespace ThAmCo.Customers.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int ProductId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public Product Product { get; set; }
    }
}
