package com.example.inventorymangementapp.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.inventorymangementapp.data.AppDatabase
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
