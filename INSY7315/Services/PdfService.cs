using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using INSY7315.Models;

namespace INSY7315.Services
{
    // Service class for generating PDF files using QuestPDF
    public class PdfService
    {
        // Helper method to make consistent page titles
        private static string Title(string main) => $"Inventory Tracker · {main}";

        // Builds a PDF document listing all products
        public byte[] BuildProductsPdf(IEnumerable<Product> products)
        {
            // Use the free community license for QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Convert to list to avoid multiple enumerations
            var list = products.ToList();

            // Create the PDF document
            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(30);

                    // --- Page Header ---
                    p.Header().Row(r =>
                    {
                        r.RelativeItem().Text(Title("Products")).SemiBold().FontSize(16);
                        r.ConstantItem(120).AlignRight().Text(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });

                    // --- Main Content Table ---
                    p.Content().Table(t =>
                    {
                        // Define table column layout
                        t.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);    // index
                            cols.RelativeColumn(2);     // name
                            cols.RelativeColumn();      // owner
                            cols.RelativeColumn();      // category
                            cols.RelativeColumn();      // model
                            cols.ConstantColumn(70);    // price
                        });

                        // Table header labels
                        t.Header(h =>
                        {
                            h.Cell().Text("#").SemiBold();
                            h.Cell().Text("Name").SemiBold();
                            h.Cell().Text("Owner").SemiBold();
                            h.Cell().Text("Category").SemiBold();
                            h.Cell().Text("Model").SemiBold();
                            h.Cell().Text("Price").SemiBold();
                        });

                        // Fill table rows with product data
                        var idx = 1;
                        foreach (var p in list)
                        {
                            t.Cell().Text(idx++.ToString());
                            t.Cell().Text(p.Name);
                            t.Cell().Text(p.Owner);
                            t.Cell().Text(p.Category ?? "");
                            t.Cell().Text(p.Model ?? "");
                            t.Cell().Text(p.Price.ToString("0.00"));
                        }
                    });

                    // --- Page Footer ---
                    p.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Generated ").FontSize(9);
                        x.Span(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });
                });
            }).GeneratePdf(); // Output as byte array
        }

        // Builds a PDF showing price history for a single product
        public byte[] BuildProductHistoryPdf(Product product, IEnumerable<PriceHistory> history)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var list = history.ToList();

            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(30);

                    // --- Page Header ---
                    p.Header().Row(r =>
                    {
                        r.RelativeItem().Text(Title($"History · {product.Name} (#{product.Id})")).SemiBold().FontSize(16);
                        r.ConstantItem(120).AlignRight().Text(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });

                    // --- Price History Table ---
                    p.Content().Table(t =>
                    {
                        // Three columns: date, old, new price
                        t.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        t.Header(h =>
                        {
                            h.Cell().Text("Changed On").SemiBold();
                            h.Cell().Text("Old").SemiBold();
                            h.Cell().Text("New").SemiBold();
                        });

                        // Add one row per history entry
                        foreach (var h in list)
                        {
                            t.Cell().Text(h.ChangedOn.ToString("u"));
                            t.Cell().Text(h.OldPrice.ToString("0.00"));
                            t.Cell().Text(h.NewPrice.ToString("0.00"));
                        }
                    });

                    // --- Page Footer ---
                    p.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Generated ").FontSize(9);
                        x.Span(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });
                });
            }).GeneratePdf();
        }
    }
}
