using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public IndexModel(AppDbContext ctx) => _ctx = ctx;

        public IList<Product> Product { get; set; } = new List<Product>();

        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public string? Category { get; set; }
        [BindProperty(SupportsGet = true)] public string? Model { get; set; }

        
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            const int pageSize = 10;

            var query = _ctx.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.Owner.Contains(q) ||
                    (p.Category != null && p.Category.Contains(q)) ||
                    (p.Model != null && p.Model.Contains(q)) ||
                    p.Id.ToString() == q);
            }

            if (!string.IsNullOrWhiteSpace(Category))
                query = query.Where(p => p.Category == Category);

            if (!string.IsNullOrWhiteSpace(Model))
                query = query.Where(p => p.Model == Model);

            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Product = await query
                .OrderBy(p => p.Name)
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
