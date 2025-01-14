namespace ThAmCo.Customers.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Navigation property for Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
