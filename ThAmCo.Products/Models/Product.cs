namespace ThAmCo.Products.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }

        // Make navigation properties optional
        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
    }
}
