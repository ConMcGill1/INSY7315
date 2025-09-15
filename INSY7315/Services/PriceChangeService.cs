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

        /// <summary>
        /// Create an Alert if price delta >= threshold. Assumes caller adds PriceHistory entry separately.
        /// </summary>
        public async Task HandlePriceChangeAsync(Product after, decimal oldPrice)
        {
            // Old price 0 → avoid division by zero; treat as 100% increase if new > 0
            decimal pct;
            if (oldPrice == 0m)
            {
                pct = after.Price == 0m ? 0m : 100m;
            }
            else
            {
                pct = ((after.Price - oldPrice) / oldPrice) * 100m;
            }

            if (Math.Abs(pct) >= _thresholdPercent)
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
