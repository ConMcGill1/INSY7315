package com.example.inventorymangementapp.data

import androidx.room.*
import com.example.inventorymangementapp.model.User
import kotlinx.coroutines.flow.Flow

@Dao
interface UserDao {
    @Query("SELECT * FROM users")
    fun getAllUsers(): Flow<List<User>>
    
    @Query("SELECT * FROM users WHERE id = :id")
    suspend fun getUserById(id: String): User?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertUser(user: User)

    @Update
    suspend fun updateUser(user: User)

    @Query("UPDATE users SET isAdmin = :isAdmin WHERE id = :userId")
    suspend fun updateAdminStatus(userId: String, isAdmin: Boolean)
}
