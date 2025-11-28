package com.example.inventorymangementapp.data

import androidx.room.*
import com.example.inventorymangementapp.model.Product
import com.example.inventorymangementapp.model.PriceHistory
import kotlinx.coroutines.flow.Flow

@Dao
interface ProductDao {
    // --- Product Operations ---
    @Query("SELECT * FROM products")
    fun getAllProducts(): Flow<List<Product>>

    @Query("SELECT * FROM products WHERE id = :id")
    suspend fun getProductById(id: Int): Product?

    @Query("""
        SELECT * FROM products 
        WHERE name LIKE '%' || :query || '%' 
        OR category LIKE '%' || :query || '%' 
        OR model LIKE '%' || :query || '%'
    """)
    fun searchProducts(query: String): Flow<List<Product>>
    
    @Query("""
        SELECT * FROM products 
        WHERE (:minPrice IS NULL OR price >= :minPrice)
        AND (:maxPrice IS NULL OR price <= :maxPrice)
        AND (:category IS NULL OR category = :category)
    """)
    fun filterProducts(minPrice: Double?, maxPrice: Double?, category: String?): Flow<List<Product>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertProduct(product: Product): Long

    @Update
    suspend fun updateProduct(product: Product)

    @Delete
    suspend fun deleteProduct(product: Product)
    
    @Query("SELECT * FROM products WHERE quantity <= lowStockThreshold")
    fun getLowStockProducts(): Flow<List<Product>>

    // --- Price History Operations ---
    @Insert
    suspend fun insertPriceHistory(history: PriceHistory)

    @Query("SELECT * FROM price_history WHERE productId = :productId ORDER BY changedDate DESC")
    fun getPriceHistory(productId: Int): Flow<List<PriceHistory>>
    
    // Added for Dashboard
    @Query("SELECT * FROM price_history")
    fun getAllPriceHistory(): Flow<List<PriceHistory>>
}
