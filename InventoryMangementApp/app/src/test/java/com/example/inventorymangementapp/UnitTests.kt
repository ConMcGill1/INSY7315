package com.example.inventorymangementapp

import com.example.inventorymangementapp.service.PriceChangeService
import org.junit.Test
import org.junit.Assert.*

/**
 * Unit tests for Business Logic (Price Validation).
 */
class UnitTests {
    private val service = PriceChangeService()

    @Test
    fun priceChange_isNotSignificant_whenIncreaseIsSmall() {
        // 5% increase (Under 10% threshold)
        val oldPrice = 100.0
        val newPrice = 105.0
        assertTrue(service.validatePriceChange(oldPrice, newPrice))
    }

    @Test
    fun priceChange_isSignificant_whenIncreaseIsLarge() {
        // 15% increase (Over 10% threshold)
        val oldPrice = 100.0
        val newPrice = 115.0
        assertFalse(service.validatePriceChange(oldPrice, newPrice))
    }

    @Test
    fun priceChange_isValid_whenPriceDecreases() {
        // Decrease is always valid in our logic
        val oldPrice = 100.0
        val newPrice = 90.0
        assertTrue(service.validatePriceChange(oldPrice, newPrice))
    }

    @Test
    fun priceFormat_isCorrectZAR() {
        val price = 123.456
        val formatted = service.formatPrice(price)
        assertEquals("R123.46", formatted)
    }
}
