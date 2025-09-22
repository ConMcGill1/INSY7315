using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;
using INSY7315.Services;
using Microsoft.AspNetCore.RateLimiting;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        var isTesting = builder.Environment.IsEnvironment("Test") || builder.Environment.IsEnvironment("Testing");
        var isProduction = builder.Environment.IsProduction();


        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/Privacy");
        });


        if (isTesting)
        {
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
        }


        builder.Services.AddDefaultIdentity<ApplicationUser>(opts =>
        {
            opts.SignIn.RequireConfirmedAccount = false;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireDigit = true;
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();


        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });


        builder.Services.AddControllersWithViews(opts =>
        {
            opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        });


        builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");


        builder.Services.AddScoped<PriceChangeService>();
        builder.Services.AddScoped<PdfService>();


        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("apiWrites", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 30;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }


        app.Use(async (ctx, next) =>
        {
            var h = ctx.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["Referrer-Policy"] = "no-referrer";
            h["X-Frame-Options"] = "DENY";
            h["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            h["Content-Security-Policy"] =
                "default-src 'self'; " +
                "style-src 'self' https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; " +
                "frame-ancestors 'none'; base-uri 'self'";
            await next();
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!isTesting)
            {
                try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }
            }
            IdentitySeed.EnsureSeedAsync(app.Services).GetAwaiter().GetResult();
        }


        app.MapRazorPages();


        var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
        var csrfFilter = new CsrfValidateFilter(antiforgery);
        var api = app.MapGroup("/api");


        if (isProduction)
        {
            api.RequireAuthorization()
               .RequireRateLimiting("apiWrites")
               .AddEndpointFilter(csrfFilter);
        }


        api.MapGet("/products", async (AppDbContext db) =>
            Results.Ok(await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync()));

        api.MapGet("/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/products", async (Product input, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.Owner) ||
                input.Price < 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Name"] = string.IsNullOrWhiteSpace(input.Name) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Owner"] = string.IsNullOrWhiteSpace(input.Owner) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Price"] = input.Price < 0 ? new[] { "Must be non-negative" } : Array.Empty<string>(),
                });
            }

            db.Products.Add(input);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{input.Id}", input);
        });

        api.MapPut("/products/{id:int}", async (int id, Product patch, AppDbContext db, PriceChangeService pcs) =>
        {
            var entity = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (entity is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(patch.Name) ||
                string.IsNullOrWhiteSpace(patch.Owner) ||
                patch.Price < 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Name"] = string.IsNullOrWhiteSpace(patch.Name) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Owner"] = string.IsNullOrWhiteSpace(patch.Owner) ? new[] { "Required" } : Array.Empty<string>(),
                    ["Price"] = patch.Price < 0 ? new[] { "Must be non-negative" } : Array.Empty<string>(),
                });
            }

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

        api.MapDelete("/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (entity is null) return Results.NotFound();
            db.Products.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapGet("/products/search", async (
            decimal? minPrice, decimal? maxPrice,
            DateTime? createdFrom, DateTime? createdTo,
            AppDbContext db) =>
        {
            var q = db.Products.AsNoTracking().AsQueryable();
            if (minPrice is not null) q = q.Where(p => p.Price >= minPrice);
            if (maxPrice is not null) q = q.Where(p => p.Price <= maxPrice);
            if (createdFrom is not null) q = q.Where(p => p.CreatedOn >= createdFrom);
            if (createdTo is not null) q = q.Where(p => p.CreatedOn <= createdTo);
            return Results.Ok(await q.OrderBy(p => p.Id).ToListAsync());
        });


        api.MapGet("/products/export.csv", async (AppDbContext db) =>
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

        api.MapGet("/products/{id:int}/history/export.csv", async (int id, AppDbContext db) =>
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

        api.MapGet("/products/export.pdf", async (AppDbContext db, PdfService pdf) =>
        {
            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            var bytes = pdf.BuildProductsPdf(items);
            return Results.File(bytes, "application/pdf", "products.pdf");
        });

        api.MapGet("/products/{id:int}/history/export.pdf", async (int id, AppDbContext db, PdfService pdf) =>
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


        api.MapGet("/alerts", async (AppDbContext db) =>
            Results.Ok(await db.Alerts.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync()));

        api.MapGet("/reports/summary", async (AppDbContext db) =>
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


public sealed class CsrfValidateFilter : IEndpointFilter
{
    private readonly IAntiforgery _antiforgery;
    public CsrfValidateFilter(IAntiforgery antiforgery) => _antiforgery = antiforgery;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        if (HttpMethods.IsPost(http.Request.Method) ||
            HttpMethods.IsPut(http.Request.Method) ||
            HttpMethods.IsDelete(http.Request.Method) ||
            HttpMethods.IsPatch(http.Request.Method))
        {
            await _antiforgery.ValidateRequestAsync(http);
        }
        return await next(context);
    }
}

public partial class Program { }
