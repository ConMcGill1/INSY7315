using System.ComponentModel.DataAnnotations;

namespace INSY7315.Models;

public class PriceHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Range(0, 1_000_000)]
    public decimal OldPrice { get; set; }

    [Range(0, 1_000_000)]
    public decimal NewPrice { get; set; }

    public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
