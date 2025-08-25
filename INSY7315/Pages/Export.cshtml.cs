using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using System.Text;

namespace INSY7315.Pages;

public class ExportModel : PageModel
{
    private readonly AppDbContext _ctx;
    public ExportModel(AppDbContext ctx) => _ctx = ctx;

    public async Task<FileContentResult> OnGetAsync()
    {
        var rows = await _ctx.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Price, p.Category, p.Model, p.Owner, p.CreatedOn })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Price,Category,Model,Owner,CreatedOn");
        foreach (var r in rows)
        {
            string name = r.Name.Replace("\"", "\"\"");
            string cat = r.Category?.Replace("\"", "\"\"") ?? "";
            string model = r.Model?.Replace("\"", "\"\"") ?? "";
            string owner = r.Owner.Replace("\"", "\"\"");
            sb.AppendLine($"{r.Id},\"{name}\",{r.Price},\"{cat}\",\"{model}\",\"{owner}\",{r.CreatedOn:o}");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "products.csv");
    }
}
