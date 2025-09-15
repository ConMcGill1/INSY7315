using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using INSY7315.Data;
using INSY7315.Models;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();

       
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<INSY7315.Services.PriceChangeService>();

        var app = builder.Build();

       
        var env = app.Services.GetRequiredService<IHostEnvironment>();
        if (!env.IsEnvironment("Test") && !env.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapRazorPages();

       
        app.MapGet("/api/products", async (AppDbContext db) =>
        {
            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            return Results.Ok(items);
        });

      
        app.MapGet("/api/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        
        app.MapPost("/api/products", async (Product input, AppDbContext db) =>
        {
           
            if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Owner) || input.Price < 0)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Name"] = string.IsNullOrWhiteSpace(input.Name) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Owner"] = string.IsNullOrWhiteSpace(input.Owner) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Price"] = input.Price < 0 ? new[] { "Must be non-negative" } : Array.Empty<string>(),
                });

            db.Products.Add(input);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{input.Id}", input);
        });

        
        app.MapPut("/api/products/{id:int}", async (int id, Product patch, AppDbContext db, INSY7315.Services.PriceChangeService pcs) =>
        {
            var entity = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (entity is null) return Results.NotFound();

            var oldPrice = entity.Price;

         
            entity.Name = patch.Name;
            entity.Owner = patch.Owner;
            entity.Category = patch.Category;
            entity.Model = patch.Model;
            entity.Price = patch.Price;

            
            if (entity.Price != oldPrice)
            {
                db.PriceHistories.Add(new PriceHistory
                {
                    ProductId = entity.Id,
                    OldPrice = oldPrice,
                    NewPrice = entity.Price,
                    ChangedOn = DateTime.UtcNow
                });

              
                await pcs.HandlePriceChangeAsync(entity, oldPrice);
            }

            await db.SaveChangesAsync();
            return Results.Ok(entity);
        });

      
        app.MapDelete("/api/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (entity is null) return Results.NotFound();
            db.Products.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        
        app.MapGet("/api/products/search", async (decimal? minPrice, decimal? maxPrice, AppDbContext db) =>
        {
            var query = db.Products.AsNoTracking().AsQueryable();
            if (minPrice is not null) query = query.Where(p => p.Price >= minPrice);
            if (maxPrice is not null) query = query.Where(p => p.Price <= maxPrice);
            var items = await query.OrderBy(p => p.Id).ToListAsync();
            return Results.Ok(items);
        });

        
        app.MapGet("/api/products/export.csv", async (AppDbContext db) =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Owner,Category,Model,Price,CreatedOn");

            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            foreach (var p in items)
            {
                
                static string Cell(string? s)
                {
                    if (string.IsNullOrEmpty(s)) return "";
                    if (s.Contains('"') || s.Contains(','))
                        return $"\"{s.Replace("\"", "\"\"")}\"";
                    return s;
                }

                sb.AppendLine($"{p.Id},{Cell(p.Name)},{Cell(p.Owner)},{Cell(p.Category)},{Cell(p.Model)},{p.Price},{p.CreatedOn:o}");
            }

            return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
        });

        
        app.MapGet("/api/alerts", async (AppDbContext db) =>
        {
            var items = await db.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Results.Ok(items);
        });

        
        app.Run();
    }
}


public partial class Program { }
