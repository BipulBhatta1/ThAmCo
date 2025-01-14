using Microsoft.EntityFrameworkCore;
using ThAmCo.Products.Models;

namespace ThAmCo.Products.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure precision for Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Configure Product -> Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category) // Product has one Category
                .WithMany(c => c.Products) // Category has many Products
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Product -> Brand relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand) // Product has one Brand
                .WithMany(b => b.Products) // Brand has many Products
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order -> Product relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product) // Order has one Product
                .WithMany() // No navigation property in Product
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
