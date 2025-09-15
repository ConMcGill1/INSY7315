using Microsoft.AspNetCore.Mvc.RazorPages;

namespace INSY7315.Pages
{
    public class ExportModel : PageModel
    {
        public string ExportUrl => "/api/products/export.csv";
        public void OnGet() { }
    }
}
