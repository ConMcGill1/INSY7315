package com.example.inventorymangementapp.service

import com.example.inventorymangementapp.model.Product
import java.util.Locale

class PriceChangeService {
    // Updated logic: 10% threshold matches the ASP.NET Core summary
    fun validatePriceChange(oldPrice: Double, newPrice: Double): Boolean {
        // Prevent price increase of more than 10% at once
        if (oldPrice > 0 && newPrice > oldPrice * 1.10) {
            return false
        }
        return newPrice >= 0
    }
    
    fun isSignificantChange(oldPrice: Double, newPrice: Double): Boolean {
         val threshold = 0.10 // 10%
         if (oldPrice <= 0) return false
         val change = kotlin.math.abs(newPrice - oldPrice)
         return change / oldPrice >= threshold
    }

    fun formatPrice(price: Double): String {
        // Changed currency symbol from $ to ZAR (R)
        return String.format(Locale.US, "R%.2f", price)
    }
}
