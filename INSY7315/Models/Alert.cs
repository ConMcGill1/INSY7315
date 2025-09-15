using System;

namespace INSY7315.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal DeltaPercent { get; set; }           // e.g., 15.5 = +15.5%
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }
}
