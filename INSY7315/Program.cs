using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();


app.MapPost("/api/products", async (AppDbContext db, ProductDto dto) =>
{
    var p = new Product
    {
        Name = dto.Name,
        Price = dto.Price,
        Owner = dto.Owner ?? "",
        Model = dto.Model,
        Category = dto.Category
    };
    db.Products.Add(p);
    await db.SaveChangesAsync();
    return Results.Ok(new { p.Id });
});

app.MapPut("/api/products/{id:int}", async (int id, AppDbContext db, ProductDto dto) =>
{
    var existing = await db.Products.Include(x => x.PriceHistory).FirstOrDefaultAsync(x => x.Id == id);
    if (existing is null) return Results.NotFound();

    if (existing.Price != dto.Price)
    {
        db.PriceHistories.Add(new PriceHistory
        {
            ProductId = existing.Id,
            OldPrice = existing.Price,
            NewPrice = dto.Price,
            ChangedOn = DateTime.UtcNow
        });
        existing.Price = dto.Price;
    }

    existing.Name = dto.Name;
    existing.Owner = dto.Owner ?? "";
    existing.Model = dto.Model;
    existing.Category = dto.Category;

    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/api/products/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return Results.Ok();
});


app.Run();
public partial class Program { } // enables WebApplicationFactory in tests
