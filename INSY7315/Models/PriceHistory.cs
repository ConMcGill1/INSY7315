using System.ComponentModel.DataAnnotations;

namespace INSY7315.Models
{
    // This class stores the price change history for each product
    public class PriceHistory
    {
        // Unique ID for the record
        public int Id { get; set; }

        // The ID of the product this history belongs to
        public int ProductId { get; set; }

        // The previous price before the change
        [Range(0, 1_000_000)]
        public decimal OldPrice { get; set; }

        // The new price after the change
        [Range(0, 1_000_000)]
        public decimal NewPrice { get; set; }

        // The date and time when the price was changed
        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

        // Navigation property to link back to the Product table
        public Product Product { get; set; } = null!;
    }
}
