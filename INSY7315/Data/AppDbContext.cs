using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using INSY7315.Models;

namespace INSY7315.Data
{
    // This is the main database context for the application
    // It inherits from IdentityDbContext so that user authentication and roles are supported
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor that sends the options to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables used in the system
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<Alert> Alerts { get; set; } = null!;

        // Configures relationships and indexes for the tables
        protected override void OnModelCreating(ModelBuilder b)
        {
            // Keeps the base identity configuration for user management
            base.OnModelCreating(b);

            // Sets up a one-to-many relationship between Product and PriceHistory
            // When a product is deleted, all related price history records are deleted as well
            b.Entity<Product>()
             .HasMany(p => p.PriceHistory)
             .WithOne(h => h.Product)
             .HasForeignKey(h => h.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            // Adds an index to help speed up searches by name, category, or model
            b.Entity<Product>()
             .HasIndex(p => new { p.Name, p.Category, p.Model });
        }
    }
}
