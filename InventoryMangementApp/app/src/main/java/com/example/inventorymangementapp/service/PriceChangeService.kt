package com.example.inventorymangementapp.service

import com.example.inventorymangementapp.model.Product
import java.util.Locale

class PriceChangeService {
    // Updated logic: 10% threshold or absolute difference of R1000
    fun validatePriceChange(oldPrice: Double, newPrice: Double): Boolean {
        // Prevent price increase of more than 10% at once
        if (oldPrice > 0 && newPrice > oldPrice * 1.10) {
            return false
        }
        return newPrice >= 0
    }
    
    fun isSignificantChange(oldPrice: Double, newPrice: Double): Boolean {
         // Significant if > 10% change OR > R1000 difference
         val thresholdPercent = 0.10 
         val thresholdAbsolute = 1000.0
         
         if (oldPrice <= 0) return false
         
         val change = kotlin.math.abs(newPrice - oldPrice)
         
         return (change / oldPrice >= thresholdPercent) || (change >= thresholdAbsolute)
    }

    fun formatPrice(price: Double): String {
        // Changed currency symbol from $ to ZAR (R)
        return String.format(Locale.US, "R%.2f", price)
    }
}
