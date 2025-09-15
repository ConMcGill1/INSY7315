using System.Text;                  // for CSV
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

        // Only run migrations when NOT in the test environment
        var env = app.Services.GetRequiredService<IHostEnvironment>();
        if (!env.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
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

        // ---------- API ENDPOINTS ----------

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

        app.MapPut("/api/products/{id:int}", async (int id, AppDbContext db, ProductDto dto, INSY7315.Services.PriceChangeService alerts) =>
        {
            var existing = await db.Products
                .Include(x => x.PriceHistory)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing is null) return Results.NotFound();

            decimal? oldPrice = null;

            if (existing.Price != dto.Price)
            {
                oldPrice = existing.Price;   // capture before changing

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

            // if price changed a lot, record alert
            if (oldPrice.HasValue)
                await alerts.MaybeCreateAlertAsync(oldPrice.Value, existing);

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

        // Advanced search
        app.MapGet("/api/products/search", async (
            string? q,
            decimal? minPrice,
            decimal? maxPrice,
            DateTime? fromDate,
            DateTime? toDate,
            AppDbContext db) =>
        {
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Owner.ToLower().Contains(s) ||
                    (p.Category != null && p.Category.ToLower().Contains(s)) ||
                    (p.Model != null && p.Model.ToLower().Contains(s)) ||
                    p.Id.ToString().Contains(s));
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (fromDate.HasValue) query = query.Where(p => p.CreatedOn >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(p => p.CreatedOn <= toDate.Value);

            var list = await query.OrderBy(p => p.Name).ToListAsync();
            return Results.Ok(list);
        });

        // CSV export
        app.MapGet("/api/products/export.csv", async (AppDbContext db) =>
        {
            static string Esc(string? s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                return (s.Contains(',') || s.Contains('"'))
                    ? $"\"{s.Replace("\"", "\"\"")}\""
                    : s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Owner,Category,Model,Price,CreatedOn");

            var rows = await db.Products
                .Select(p => new { p.Id, p.Name, p.Owner, p.Category, p.Model, p.Price, p.CreatedOn })
                .OrderBy(p => p.Name)
                .ToListAsync();

            foreach (var r in rows)
                sb.AppendLine($"{r.Id},{Esc(r.Name)},{Esc(r.Owner)},{Esc(r.Category)},{Esc(r.Model)},{r.Price},{r.CreatedOn:yyyy-MM-dd}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"products_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        // Alerts list
        app.MapGet("/api/alerts", async (AppDbContext db) =>
        {
            var items = await db.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Results.Ok(items);
        });

        // ---------- RUN ----------
        app.Run();
    }
}

// Keep this so WebApplicationFactory<Program> can locate the entry point in tests
public partial class Program { }
