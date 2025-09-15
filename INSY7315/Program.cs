using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;
using INSY7315.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

       
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/Privacy");
        });

        
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

       
        builder.Services.AddDefaultIdentity<ApplicationUser>(opts =>
        {
            opts.SignIn.RequireConfirmedAccount = false;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireDigit = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddScoped<PriceChangeService>();
        builder.Services.AddScoped<PdfService>();

        var app = builder.Build();

        
        var env = app.Services.GetRequiredService<IHostEnvironment>();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!env.IsEnvironment("Test") && !env.IsEnvironment("Testing"))
            {
                try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }
            }
            IdentitySeed.EnsureSeedAsync(app.Services).GetAwaiter().GetResult();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

       
        app.MapGet("/api/products", async (AppDbContext db) =>
            Results.Ok(await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync()));

       
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

       
        app.MapPut("/api/products/{id:int}", async (int id, Product patch, AppDbContext db, PriceChangeService pcs) =>
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

       
        app.MapGet("/api/products/search", async (
            decimal? minPrice,
            decimal? maxPrice,
            DateTime? createdFrom,
            DateTime? createdTo,
            AppDbContext db) =>
        {
            var q = db.Products.AsNoTracking().AsQueryable();
            if (minPrice is not null) q = q.Where(p => p.Price >= minPrice);
            if (maxPrice is not null) q = q.Where(p => p.Price <= maxPrice);
            if (createdFrom is not null) q = q.Where(p => p.CreatedOn >= createdFrom);
            if (createdTo is not null) q = q.Where(p => p.CreatedOn <= createdTo);
            return Results.Ok(await q.OrderBy(p => p.Id).ToListAsync());
        });

       
        app.MapGet("/api/products/export.csv", async (AppDbContext db) =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Owner,Category,Model,Price,CreatedOn");
            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

            static string Cell(string? s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                if (s.Contains('"') || s.Contains(',')) return $"\"{s.Replace("\"", "\"\"")}\"";
                return s;
            }

            foreach (var p in items)
                sb.AppendLine($"{p.Id},{Cell(p.Name)},{Cell(p.Owner)},{Cell(p.Category)},{Cell(p.Model)},{p.Price},{p.CreatedOn:o}");

            return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
        });

        app.MapGet("/api/products/{id:int}/history/export.csv", async (int id, AppDbContext db) =>
        {
            var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            if (product is null) return Results.NotFound();

            var sb = new StringBuilder();
            sb.AppendLine("ChangedOn,OldPrice,NewPrice");
            var hist = await db.PriceHistories.AsNoTracking()
                        .Where(h => h.ProductId == id)
                        .OrderByDescending(h => h.ChangedOn)
                        .ToListAsync();

            foreach (var h in hist)
                sb.AppendLine($"{h.ChangedOn:o},{h.OldPrice},{h.NewPrice}");

            return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
        });

        app.MapGet("/api/products/export.pdf", async (AppDbContext db, PdfService pdf) =>
        {
            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            var bytes = pdf.BuildProductsPdf(items);
            return Results.File(bytes, "application/pdf", "products.pdf");
        });

        app.MapGet("/api/products/{id:int}/history/export.pdf", async (int id, AppDbContext db, PdfService pdf) =>
        {
            var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            if (product is null) return Results.NotFound();

            var hist = await db.PriceHistories.AsNoTracking()
                        .Where(h => h.ProductId == id)
                        .OrderByDescending(h => h.ChangedOn)
                        .ToListAsync();

            var bytes = pdf.BuildHistoryPdf(product, hist);
            return Results.File(bytes, "application/pdf", $"product-{id}-history.pdf");
        });

        
        app.MapGet("/api/alerts", async (AppDbContext db) =>
            Results.Ok(await db.Alerts.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync()));

       
        app.MapGet("/api/reports/summary", async (AppDbContext db) =>
        {
            var total = await db.Products.CountAsync();
            var totalValue = await db.Products.SumAsync(p => (decimal?)p.Price) ?? 0m;
            var recentAlerts = await db.Alerts.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(5).ToListAsync();
            var topCategories = await db.Products.AsNoTracking()
                .GroupBy(p => p.Category ?? "Uncategorized")
                .Select(g => new { Category = g.Key, Count = g.Count(), Value = g.Sum(p => p.Price) })
                .OrderByDescending(x => x.Count).Take(5).ToListAsync();

            return Results.Ok(new { totalProducts = total, totalInventoryValue = totalValue, recentAlerts, topCategories });
        });

        app.Run();
    }
}

public partial class Program { }