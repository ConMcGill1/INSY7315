package com.example.inventorymangementapp.data

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import com.example.inventorymangementapp.model.Alert
import com.example.inventorymangementapp.model.PriceHistory
import com.example.inventorymangementapp.model.Product
import com.example.inventorymangementapp.model.User

@Database(
    entities = [Product::class, Alert::class, User::class, PriceHistory::class], 
    version = 4, // Incremented version due to User schema change
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {
    abstract fun productDao(): ProductDao
    abstract fun userDao(): UserDao

    companion object {
        @Volatile
        private var INSTANCE: AppDatabase? = null

        fun getDatabase(context: Context): AppDatabase {
            return INSTANCE ?: synchronized(this) {
                val instance = Room.databaseBuilder(
                    context.applicationContext,
                    AppDatabase::class.java,
                    "inventory_database"
                )
                .fallbackToDestructiveMigration() // Clears DB on version change
                .build()
                INSTANCE = instance
                instance
            }
        }
    }
}
