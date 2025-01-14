namespace ThAmCo.Customers.Models
{
    public class Fund
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }


        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
