namespace INSY7315.Models;

public record ProductDto(
    int? Id,
    string Name,
    decimal Price,
    string Owner,
    string? Model,
    string? Category
);
