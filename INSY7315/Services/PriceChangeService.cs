using System;
using System.Threading.Tasks;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Services
{
    // This service checks if a product's price has changed enough to trigger an alert
    public class PriceChangeService
    {
        private readonly AppDbContext _db;
        private readonly decimal _thresholdPercent;

        // The threshold defines when a price change should be recorded (default is 10%)
        public PriceChangeService(AppDbContext db, decimal thresholdPercent = 10m)
        {
            _db = db;
            _thresholdPercent = thresholdPercent;
        }

        // Handles price change logic for a product
        public async Task HandlePriceChangeAsync(Product after, decimal oldPrice)
        {
            decimal pct;

            // Avoid divide-by-zero errors and calculate percentage change
            if (oldPrice == 0m)
                pct = after.Price == 0m ? 0m : 100m;
            else
                pct = ((after.Price - oldPrice) / oldPrice) * 100m;

            // If price change exceeds the threshold, create a new alert
            if (Math.Abs(pct) >= _thresholdPercent)
            {
                var alert = new Alert
                {
                    ProductId = after.Id,
                    OldPrice = oldPrice,
                    NewPrice = after.Price,
                    DeltaPercent = Math.Round(pct, 2),
                    Message = $"Price changed by {Math.Round(pct, 2)}%: {oldPrice:0.00} -> {after.Price:0.00}"
                };

                // Save the alert in the database
                _db.Alerts.Add(alert);
                await _db.SaveChangesAsync();
            }
        }
    }
}
