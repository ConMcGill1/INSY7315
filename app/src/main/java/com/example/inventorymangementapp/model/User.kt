package com.example.inventorymangementapp.model

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "users")
data class User(
    @PrimaryKey val id: String,
    val email: String,
    val passwordHash: String?, // Added to store password (plain for demo, ideally hashed)
    val displayName: String?,
    val isAdmin: Boolean
)
