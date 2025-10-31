using System;

namespace INSY7315.Models
{
    // This class stores information about alerts raised when a product price changes
    public class Alert
    {
        // Unique ID for each alert
        public int Id { get; set; }

        // The ID of the product the alert is linked to
        public int ProductId { get; set; }

        // The old price before the change
        public decimal OldPrice { get; set; }

        // The new price after the change
        public decimal NewPrice { get; set; }

        // The percentage difference between the old and new prices
        public decimal DeltaPercent { get; set; }

        // The date and time when the alert was created (UTC for consistency)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Message shown with the alert, e.g. "Price increased by 10%"
        public string Message { get; set; } = string.Empty;
    }
}
