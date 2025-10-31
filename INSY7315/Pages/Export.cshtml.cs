using Microsoft.AspNetCore.Mvc.RazorPages;

namespace INSY7315.Pages
{
    // Page model for the export page
    public class ExportModel : PageModel
    {
        // Link to the API endpoint that generates the CSV file
        public string ExportUrl => "/api/products/export.csv";

        // Called when the page loads (no extra logic needed here)
        public void OnGet() { }
    }
}
