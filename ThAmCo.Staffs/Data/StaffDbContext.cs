using Microsoft.EntityFrameworkCore;
using ThAmCo.Staffs.Models;
using ThAmCo.Customers.Models;

namespace ThAmCo.Staffs.Data
{
    public class StaffDbContext : DbContext
    {
        public StaffDbContext(DbContextOptions<StaffDbContext> options) : base(options) { }

        public DbSet<Staff> Staffs { get; set; }
        public DbSet<DispatchRecord> DispatchRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure DispatchRecord
            modelBuilder.Entity<DispatchRecord>()
                .HasKey(dr => dr.Id);

            // Configure foreign key for OrderId
            modelBuilder.Entity<DispatchRecord>()
                .HasOne(dr => dr.Order)
                .WithMany()
                .HasForeignKey(dr => dr.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DispatchRecord>()
                .Property(dr => dr.IsDispatched)
                .HasDefaultValue(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}

