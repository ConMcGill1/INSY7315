package com.example.inventorymangementapp.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.inventorymangementapp.data.AppDatabase
import com.example.inventorymangementapp.model.User
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.firstOrNull
import kotlinx.coroutines.launch

class UserViewModel(application: Application) : AndroidViewModel(application) {
    private val userDao = AppDatabase.getDatabase(application).userDao()
    val allUsers: Flow<List<User>> = userDao.getAllUsers()

    // Current logged in user state. Null means "Guest" (not logged in, but viewing dashboard)
    // If logged in as Admin, isAdmin = true
    private val _currentUser = MutableStateFlow<User?>(null)
    val currentUser = _currentUser.asStateFlow()
    
    val isAdmin: Boolean
        get() = _currentUser.value?.isAdmin == true

    init {
        // Seed some data for demo purposes if empty
        viewModelScope.launch {
            // Check if the user list is empty by collecting the first emission
            val users = userDao.getAllUsers().firstOrNull()
            if (users.isNullOrEmpty()) {
                // Admin with default password
                userDao.insertUser(User("1", "admin", "Admin123!", "Admin User", true))
                // Regular user
                userDao.insertUser(User("2", "user@example.com", "password", "Regular User", false))
                // Guest placeholder
                userDao.insertUser(User("3", "guest@example.com", null, null, false))
            }
        }
    }

    fun login(username: String, password: String, onResult: (Boolean) -> Unit) {
        viewModelScope.launch {
            // Special case for hardcoded admin if somehow DB is not synced or initialized
            if (username == "admin" && password == "Admin123!") {
                // Fetch admin from DB or create dummy
                val users = userDao.getAllUsers().first()
                val adminUser = users.find { it.email == "admin" } ?: User("1", "admin", "Admin123!", "Admin User", true)
                _currentUser.value = adminUser
                onResult(true)
                return@launch
            }

            // Fetch users from DB to check if user exists and if password matches
            val users = userDao.getAllUsers().first()
            val user = users.find { it.email == username }
            
            if (user != null && user.passwordHash == password) {
                _currentUser.value = user
                onResult(true)
            } else {
                 onResult(false)
            }
        }
    }
    
    fun register(username: String, password: String, displayName: String) {
        viewModelScope.launch {
            val newUser = User(
                id = java.util.UUID.randomUUID().toString(),
                email = username,
                passwordHash = password, // Storing plain text for demo simplicity
                displayName = displayName,
                isAdmin = false // Default to false, admin can promote later
            )
            userDao.insertUser(newUser)
        }
    }
    
    fun logout() {
        _currentUser.value = null
    }

    fun updateAdminStatus(userId: String, isAdmin: Boolean) {
        viewModelScope.launch {
            userDao.updateAdminStatus(userId, isAdmin)
        }
    }

    fun updateProfile(userId: String, displayName: String?, passwordHash: String?) {
        viewModelScope.launch {
            val user = userDao.getUserById(userId)
            if (user != null) {
                val updatedUser = user.copy(
                    displayName = displayName ?: user.displayName,
                    passwordHash = passwordHash ?: user.passwordHash
                )
                userDao.updateUser(updatedUser)
                // Update current user state if it's the logged in user
                if (_currentUser.value?.id == userId) {
                    _currentUser.value = updatedUser
                }
            }
        }
    }
}
