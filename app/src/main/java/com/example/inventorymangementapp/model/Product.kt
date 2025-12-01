package com.example.inventorymangementapp.model

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "products")
data class Product(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val remoteId: Int? = null, // For sync with SQL Server
    val name: String,
    val price: Double,
    val quantity: Int,
    val lowStockThreshold: Int,
    val owner: String? = null,
    val model: String? = null,
    val category: String? = null,
    val isSynced: Boolean = false
)
