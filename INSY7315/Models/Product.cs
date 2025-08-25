using System.ComponentModel.DataAnnotations;

namespace INSY7315.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Required, StringLength(100)]
    public string Owner { get; set; } = "";

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public List<PriceHistory> PriceHistory { get; set; } = new();
}
