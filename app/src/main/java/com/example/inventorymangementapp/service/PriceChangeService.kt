package com.example.inventorymangementapp.service

import com.example.inventorymangementapp.model.Product
import java.util.Locale
import kotlin.math.abs

class PriceChangeService {


    fun validatePriceChange(oldPrice: Double, newPrice: Double): Boolean {

        if (newPrice < 0) return false


        return true
    }


    fun isSignificantChange(oldPrice: Double, newPrice: Double): Boolean {
        val thresholdPercent = 0.10      // 10%
        val thresholdAbsolute = 1000.0   // R1000


        if (oldPrice <= 0 || newPrice < 0) return false

        val change = abs(newPrice - oldPrice)

        return (change / oldPrice >= thresholdPercent) || (change >= thresholdAbsolute)
    }

    fun formatPrice(price: Double): String {
        // Use "R" for ZAR
        return String.format(Locale("en", "ZA"), "R%.2f", price)
    }
}
