using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using INSY7315.Models;

namespace INSY7315.Services
{
    public class PdfService
    {
        private static string Title(string main) => $"Inventory Tracker · {main}";

        public byte[] BuildProductsPdf(IEnumerable<Product> products)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var list = products.ToList();

            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(30);
                    p.Header().Row(r =>
                    {
                        r.RelativeItem().Text(Title("Products")).SemiBold().FontSize(16);
                        r.ConstantItem(120).AlignRight().Text(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });
                    p.Content().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.ConstantColumn(70);
                        });

                        t.Header(h =>
                        {
                            h.Cell().Text("#").SemiBold();
                            h.Cell().Text("Name").SemiBold();
                            h.Cell().Text("Owner").SemiBold();
                            h.Cell().Text("Category").SemiBold();
                            h.Cell().Text("Model").SemiBold();
                            h.Cell().Text("Price").SemiBold();
                        });

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
                    p.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Generated ").FontSize(9);
                        x.Span(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });
                });
            }).GeneratePdf();
        }

        public byte[] BuildProductHistoryPdf(Product product, IEnumerable<PriceHistory> history)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var list = history.ToList();

            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(30);
                    p.Header().Row(r =>
                    {
                        r.RelativeItem().Text(Title($"History · {product.Name} (#{product.Id})")).SemiBold().FontSize(16);
                        r.ConstantItem(120).AlignRight().Text(DateTime.UtcNow.ToString("u")).FontSize(9);
                    });
                    p.Content().Table(t =>
                    {
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

                        foreach (var h in list)
                        {
                            t.Cell().Text(h.ChangedOn.ToString("u"));
                            t.Cell().Text(h.OldPrice.ToString("0.00"));
                            t.Cell().Text(h.NewPrice.ToString("0.00"));
                        }
                    });
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
