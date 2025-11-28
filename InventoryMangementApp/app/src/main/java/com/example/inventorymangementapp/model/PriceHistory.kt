package com.example.inventorymangementapp.model

import androidx.room.Entity
import androidx.room.PrimaryKey
import androidx.room.ForeignKey
import androidx.room.Index

@Entity(
    tableName = "price_history",
    foreignKeys = [
        ForeignKey(
            entity = Product::class,
            parentColumns = ["id"],
            childColumns = ["productId"],
            onDelete = ForeignKey.CASCADE
        )
    ],
    indices = [Index(value = ["productId"])]
)
data class PriceHistory(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val productId: Int,
    val oldPrice: Double,
    val newPrice: Double,
    val changedDate: Long = System.currentTimeMillis(),
    val changedBy: String? = null // Could be username or ID
)
