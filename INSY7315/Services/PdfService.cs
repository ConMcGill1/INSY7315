using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using INSY7315.Models;

namespace INSY7315.Services
{
    public class PdfService
    {
        public byte[] BuildProductsPdf(IEnumerable<Product> products)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var list = products.ToList();

            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(36);
                    p.Header().Text("Products").SemiBold().FontSize(18);
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
                            h.Cell().AlignRight().Text("Price").SemiBold();
                        });

                        int i = 1;
                        foreach (var p in list)
                        {
                            t.Cell().Text(i++.ToString());
                            t.Cell().Text(p.Name);
                            t.Cell().Text(p.Owner);
                            t.Cell().Text(p.Category ?? "");
                            t.Cell().Text(p.Model ?? "");
                            t.Cell().AlignRight().Text(p.Price.ToString("0.00"));
                        }
                    });
                    p.Footer().AlignRight().Text(DateTime.UtcNow.ToString("u"));
                });
            }).GeneratePdf();
        }

        public byte[] BuildHistoryPdf(Product product, IEnumerable<PriceHistory> history)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var list = history.OrderByDescending(x => x.ChangedOn).ToList();

            return Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(36);
                    p.Header().Text($"Price History — {product.Name} (Id {product.Id})").SemiBold().FontSize(18);
                    p.Content().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
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
                    p.Footer().AlignRight().Text(DateTime.UtcNow.ToString("u"));
                });
            }).GeneratePdf();
        }
    }
}
