package com.example.inventorymangementapp

import android.content.Context
import androidx.room.Room
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.inventorymangementapp.data.AppDatabase
import com.example.inventorymangementapp.data.ProductDao
import com.example.inventorymangementapp.model.Product
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.junit.Assert.*

/**
 * Integration tests for Database operations (CRUD).
 */
@RunWith(AndroidJUnit4::class)
class IntegrationTests {
    private lateinit var db: AppDatabase
    private lateinit var dao: ProductDao

    @Before
    fun createDb() {
        val context = ApplicationProvider.getApplicationContext<Context>()
        // Using an in-memory database for testing (fresh state every time)
        db = Room.inMemoryDatabaseBuilder(context, AppDatabase::class.java).build()
        dao = db.productDao()
    }

    @After
    fun closeDb() {
        db.close()
    }

    @Test
    fun insertAndGetProduct() = runBlocking {
        val product = Product(name = "Test Product", price = 10.0, quantity = 5, lowStockThreshold = 2)
        dao.insertProduct(product)

        val allProducts = dao.getAllProducts().first()
        assertEquals(1, allProducts.size)
        assertEquals("Test Product", allProducts[0].name)
    }

    @Test
    fun updateProduct() = runBlocking {
        val product = Product(name = "Old Name", price = 10.0, quantity = 5, lowStockThreshold = 2)
        dao.insertProduct(product)
        
        // Fetch inserted product to get ID
        val inserted = dao.getAllProducts().first()[0]
        
        val updated = inserted.copy(name = "New Name")
        dao.updateProduct(updated)

        val allProducts = dao.getAllProducts().first()
        assertEquals("New Name", allProducts[0].name)
    }

    @Test
    fun deleteProduct() = runBlocking {
        val product = Product(name = "To Delete", price = 10.0, quantity = 5, lowStockThreshold = 2)
        dao.insertProduct(product)
        
        val inserted = dao.getAllProducts().first()[0]
        dao.deleteProduct(inserted)

        val allProducts = dao.getAllProducts().first()
        assertTrue(allProducts.isEmpty())
    }
}
