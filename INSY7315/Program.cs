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
    private static async Task Main(string[] args)
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
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
        }

        
        builder.Services
        .AddDefaultIdentity<ApplicationUser>(opts =>
        {
            opts.SignIn.RequireConfirmedAccount = false;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireDigit = true;
            opts.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddRoles<IdentityRole>()                   
        .AddEntityFrameworkStores<AppDbContext>();

       
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        
        var threshold = builder.Configuration.GetValue<decimal>("Alerts:PriceChangePercent", 10m);
        builder.Services.AddScoped(sp => new PriceChangeService(sp.GetRequiredService<AppDbContext>(), threshold));
        builder.Services.AddScoped<PdfService>();

      
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        
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
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        if (!isProduction)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

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

            input.CreatedOn = DateTime.UtcNow;
            db.Products.Add(input);
            await db.SaveChangesAsync(); 

           
            db.PriceHistories.Add(new PriceHistory
            {
                ProductId = input.Id,
                OldPrice = input.Price,
                NewPrice = input.Price,
                ChangedOn = DateTime.UtcNow
            });
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

            entity.Name = patch.Name.Trim();
            entity.Owner = patch.Owner.Trim();
            entity.Price = patch.Price;
            entity.Model = patch.Model?.Trim();
            entity.Category = patch.Category?.Trim();

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

        // ===== Exports =====
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

            var hist = await db.PriceHistories.AsNoTracking().Where(h => h.ProductId == id)
                .OrderByDescending(h => h.ChangedOn).ToListAsync();

            var bytes = pdf.BuildProductHistoryPdf(product, hist);
            return Results.File(bytes, "application/pdf", $"product_{id}_history.pdf");
        });

        api.MapGet("/products/export.csv", async (AppDbContext db) =>
        {
            var items = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Owner,Category,Model,Price,CreatedOn");
            foreach (var p in items)
            {
                static string Esc(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
                sb.AppendLine(string.Join(",", new[]
                {
                    p.Id.ToString(),
                    Esc(p.Name),
                    Esc(p.Owner),
                    Esc(p.Category),
                    Esc(p.Model),
                    p.Price.ToString("0.00"),
                    p.CreatedOn.ToString("u")
                }));
            }
            return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
        });

        api.MapGet("/products/{id:int}/history/export.csv", async (int id, AppDbContext db) =>
        {
            var hist = await db.PriceHistories.AsNoTracking().Where(h => h.ProductId == id)
                .OrderByDescending(h => h.ChangedOn).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ChangedOn,OldPrice,NewPrice");
            foreach (var h in hist)
            {
                sb.AppendLine($"{h.ChangedOn:u},{h.OldPrice:0.00},{h.NewPrice:0.00}");
            }
            return Results.Text(sb.ToString(), "text/csv", Encoding.UTF8);
        });

        
        api.MapGet("/alerts", async (AppDbContext db) =>
        {
            var list = await db.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();
            return Results.Ok(list);
        });

        api.MapGet("/reports/summary", async (AppDbContext db) =>
        {
            var totalProducts = await db.Products.CountAsync();
            var totalInventoryValue = await db.Products.SumAsync(p => (decimal?)p.Price) ?? 0m;

            var recentAlerts = await db.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var topCategories = await db.Products.AsNoTracking()
                .GroupBy(p => p.Category ?? "Uncategorized")
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Value = g.Sum(x => x.Price)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return Results.Ok(new { totalProducts, totalInventoryValue, recentAlerts, topCategories });
        });

        using (var scope = app.Services.CreateScope())
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var adminEmail = "admin@demo.local";
            var adminPass = "Admin123!";

            if (!await roleMgr.RoleExistsAsync("Admin"))
                await roleMgr.CreateAsync(new IdentityRole("Admin"));
            if (!await roleMgr.RoleExistsAsync("Owner"))
                await roleMgr.CreateAsync(new IdentityRole("Owner"));

            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, adminPass);
                await userMgr.AddToRoleAsync(admin, "Admin");
                await userMgr.AddToRoleAsync(admin, "Owner");
            }
        }

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
