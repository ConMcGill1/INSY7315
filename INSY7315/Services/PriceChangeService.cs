using System;
using System.Threading.Tasks;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Services
{
    public class PriceChangeService
    {
        private readonly AppDbContext _db;
        private readonly decimal _thresholdPercent;

        public PriceChangeService(AppDbContext db, decimal thresholdPercent = 10m) // default 10%
        {
            _db = db;
            _thresholdPercent = thresholdPercent;
        }

        public async Task MaybeCreateAlertAsync(decimal oldPrice, Product after)
        {
            if (oldPrice <= 0) return;

            var delta = after.Price - oldPrice;
            var pct = Math.Abs((delta / oldPrice) * 100m);

            if (pct >= _thresholdPercent)
            {
                var alert = new Alert
                {
                    ProductId = after.Id,
                    OldPrice = oldPrice,
                    NewPrice = after.Price,
                    DeltaPercent = Math.Round(pct, 2),
                    Message = $"Price changed by {Math.Round(pct, 2)}%: {oldPrice} -> {after.Price}"
                };

                _db.Alerts.Add(alert);
                await _db.SaveChangesAsync();
            }
        }
    }
}
