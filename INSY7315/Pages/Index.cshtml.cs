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
        [BindProperty(SupportsGet = true)] public decimal? MinPrice { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? MaxPrice { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? CreatedFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? CreatedTo { get; set; }

    
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

 
        [BindProperty(SupportsGet = true)] public string? SortBy { get; set; }   
        [BindProperty(SupportsGet = true)] public string? SortDir { get; set; }  

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

            if (MinPrice is not null) query = query.Where(p => p.Price >= MinPrice);
            if (MaxPrice is not null) query = query.Where(p => p.Price <= MaxPrice);
            if (CreatedFrom is not null) query = query.Where(p => p.CreatedOn >= CreatedFrom);
            if (CreatedTo is not null) query = query.Where(p => p.CreatedOn <= CreatedTo);

        
            var by = (SortBy ?? "name").Trim().ToLowerInvariant();
            var dir = (SortDir ?? "asc").Trim().ToLowerInvariant();
            if (dir != "asc" && dir != "desc") dir = "asc";

            query = (by, dir) switch
            {
                ("price", "asc") => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
                ("price", "desc") => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),

                ("name", "asc") => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
                ("name", "desc") => query.OrderByDescending(p => p.Name).ThenBy(p => p.Id),

                ("owner", "asc") => query.OrderBy(p => p.Owner).ThenBy(p => p.Id),
                ("owner", "desc") => query.OrderByDescending(p => p.Owner).ThenBy(p => p.Id),

                ("category", "asc") => query.OrderBy(p => p.Category).ThenBy(p => p.Id),
                ("category", "desc") => query.OrderByDescending(p => p.Category).ThenBy(p => p.Id),

                ("model", "asc") => query.OrderBy(p => p.Model).ThenBy(p => p.Id),
                ("model", "desc") => query.OrderByDescending(p => p.Model).ThenBy(p => p.Id),

                ("created", "asc") => query.OrderBy(p => p.CreatedOn).ThenBy(p => p.Id),
                ("created", "desc") => query.OrderByDescending(p => p.CreatedOn).ThenBy(p => p.Id),

                ("id", "asc") => query.OrderBy(p => p.Id),
                ("id", "desc") => query.OrderByDescending(p => p.Id),

                _ => query.OrderBy(p => p.Name).ThenBy(p => p.Id) 
            };

           
            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Product = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

       
        public string NextDir(string column) =>
            string.Equals(SortBy, column, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                ? "desc" : "asc";

        public string ArrowFor(string column)
        {
            if (!string.Equals(SortBy, column, StringComparison.OrdinalIgnoreCase)) return "";
            return string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase) ? " ▲" : " ▼";
        }
    }
}
