using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _ctx;
    public IndexModel(AppDbContext ctx) => _ctx = ctx;


    public IList<Product> Product { get; set; } = new List<Product>();

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Model { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;

        IQueryable<Product> query = _ctx.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            query = query.Where(p =>
                p.Name.Contains(Q) ||
                p.Owner.Contains(Q) ||
                (p.Category != null && p.Category.Contains(Q)) ||
                (p.Model != null && p.Model.Contains(Q)) ||
                p.Id.ToString() == Q);
        }
        if (!string.IsNullOrWhiteSpace(Category)) query = query.Where(p => p.Category == Category);
        if (!string.IsNullOrWhiteSpace(Model)) query = query.Where(p => p.Model == Model);

        var count = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        Product = await query
            .OrderBy(p => p.Name)
            .Skip((Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
