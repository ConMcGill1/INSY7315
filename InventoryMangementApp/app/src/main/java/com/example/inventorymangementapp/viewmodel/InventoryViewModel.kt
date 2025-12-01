package com.example.inventorymangementapp.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.inventorymangementapp.data.AppDatabase
import com.example.inventorymangementapp.model.Alert
import com.example.inventorymangementapp.model.PriceHistory
import com.example.inventorymangementapp.model.Product
import com.example.inventorymangementapp.service.PriceChangeService
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.launch

class InventoryViewModel(application: Application) : AndroidViewModel(application) {
    private val productDao = AppDatabase.getDatabase(application).productDao()
    // We can also access alertDao if we had one, or insert alerts via Room directly?
    // Assuming Alert entity exists but we didn't add AlertDao methods yet or we can insert via logic?
    // Actually AppDatabase has Alert entity. Let's check if we can access it.
    // But wait, alerts are usually separate. 
    // The prompt asks "where is the alert for big price change".
    // We should probably insert an Alert record when a significant price change happens.
    
    // For this demo, let's say we generate alerts dynamically or we should persist them.
    // The Alert entity exists. Let's add insertAlert to DAO or use a separate AlertDao.
    // ProductDao is currently handling everything. Let's stick to ProductDao for simplicity or check AppDatabase.
    // AppDatabase has entities [Product, Alert, ...].
    // We need a way to insert Alert.
    
    // Let's add insertAlert to ProductDao for now to avoid creating new file if not needed,
    // or better, check if we can just use PriceHistory as the "Alert" source (which we do in Dashboard).
    // BUT "Alerts" screen usually shows "Low Stock".
    // If user wants "Price Change Alerts" in the "Alerts" screen, we should combine them.
    
    private val priceChangeService = PriceChangeService()

    // --- Search & Filter State ---
    private val _searchQuery = MutableStateFlow("")
    val searchQuery = _searchQuery.asStateFlow()

    
    fun setSearchQuery(query: String) {
        _searchQuery.value = query
    }

    val allProducts: Flow<List<Product>> = _searchQuery.combine(productDao.getAllProducts()) { query, products ->
        if (query.isBlank()) {
            products
        } else {
            products.filter { 
                it.name.contains(query, ignoreCase = true) || 
                (it.category?.contains(query, ignoreCase = true) == true) ||
                (it.model?.contains(query, ignoreCase = true) == true)
            }
        }
    }

    // Combined Alerts: Low Stock + Significant Price Changes
    // We'll fetch price history and filter for significant ones to display as "Alerts" alongside low stock.
    val alerts: Flow<List<AlertItem>> = productDao.getLowStockProducts()
        .combine(productDao.getAllPriceHistory()) { lowStock, history ->
            val stockAlerts = lowStock.map { 
                AlertItem.StockAlert(it)
            }
            
            val priceAlerts = history.filter { 
                priceChangeService.isSignificantChange(it.oldPrice, it.newPrice)
            }.sortedByDescending { it.changedDate }.take(20).map { 
                AlertItem.PriceAlert(it)
            }
            
            stockAlerts + priceAlerts
        }

    // Keep original lowStockProducts for compatibility if needed, but UI likely uses 'alerts' now?
    // The original AlertsScreen used 'lowStockProducts'. We should update AlertsScreen to use this new 'alerts' flow.
    val lowStockProducts: Flow<List<Product>> = productDao.getLowStockProducts()

    // Dashboard Statistics
    val dashboardStats: Flow<DashboardStats> = productDao.getAllProducts().combine(productDao.getAllPriceHistory()) { products, priceHistories ->
        val totalProducts = products.size
        val totalStock = products.sumOf { it.quantity }
        val totalValue = products.sumOf { it.price * it.quantity }
        
        val categoryDistribution = products
            .groupBy { it.category ?: "Uncategorized" }
            .mapValues { it.value.size }
            .entries.sortedByDescending { it.value }
            .take(3) // Top 3 categories

        // Major Price Changes (using service threshold)
        val significantPriceChanges = priceHistories.filter { 
            priceChangeService.isSignificantChange(it.oldPrice, it.newPrice)
        }.sortedByDescending { it.changedDate }.take(5)

        DashboardStats(
            totalProducts = totalProducts,
            totalStock = totalStock,
            totalValue = totalValue,
            topCategories = categoryDistribution,
            significantPriceChanges = significantPriceChanges
        )
    }

    // --- CRUD Operations ---

    fun addProduct(product: Product) {
        viewModelScope.launch {
            productDao.insertProduct(product)
        }
    }

    fun updateProduct(product: Product) {
        viewModelScope.launch {
            val oldProduct = productDao.getProductById(product.id)
            
            if (oldProduct != null) {
                productDao.updateProduct(product)

                if (oldProduct.price != product.price) {
                    val history = PriceHistory(
                        productId = product.id,
                        oldPrice = oldProduct.price,
                        newPrice = product.price,
                        changedBy = product.owner ?: "Unknown"
                    )
                    productDao.insertPriceHistory(history)
                }
            }
        }
    }

    fun deleteProduct(product: Product) {
        viewModelScope.launch {
            productDao.deleteProduct(product)
        }
    }
    
    suspend fun getProductById(id: Int): Product? {
        return productDao.getProductById(id)
    }

    fun getPriceHistory(productId: Int): Flow<List<PriceHistory>> {
        return productDao.getPriceHistory(productId)
    }
}

data class DashboardStats(
    val totalProducts: Int = 0,
    val totalStock: Int = 0,
    val totalValue: Double = 0.0,
    val topCategories: List<Map.Entry<String, Int>> = emptyList(),
    val significantPriceChanges: List<PriceHistory> = emptyList()
)

sealed class AlertItem {
    data class StockAlert(val product: Product) : AlertItem()
    data class PriceAlert(val history: PriceHistory) : AlertItem()
}
