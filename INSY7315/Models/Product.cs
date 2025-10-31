using System.ComponentModel.DataAnnotations;

namespace INSY7315.Models
{
    // This class represents a product in the system
    public class Product
    {
        // Unique ID for each product
        public int Id { get; set; }

        // Product name (required and limited to 200 characters)
        [Required, StringLength(200)]
        public string Name { get; set; } = "";

        // Current price of the product
        [Range(0, 1_000_000)]
        public decimal Price { get; set; }

        // Date and time when the product was created
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // The owner or person responsible for the product
        [Required, StringLength(100)]
        public string Owner { get; set; } = "";

        // Optional model name or number
        [StringLength(100)]
        public string? Model { get; set; }

        // Optional category to help group similar products
        [StringLength(100)]
        public string? Category { get; set; }

        // List of past price changes linked to this product
        public List<PriceHistory> PriceHistory { get; set; } = new();
    }
}
