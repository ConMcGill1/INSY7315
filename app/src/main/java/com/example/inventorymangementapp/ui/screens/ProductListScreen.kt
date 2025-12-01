package com.example.inventorymangementapp.ui.screens

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.model.Product
import com.example.inventorymangementapp.viewmodel.InventoryViewModel
import com.example.inventorymangementapp.viewmodel.UserViewModel
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProductListScreen(
    navController: NavController,
    viewModel: InventoryViewModel,
    userViewModel: UserViewModel
) {
    // Observe the filtered list from ViewModel
    val products by viewModel.allProducts.collectAsState(initial = emptyList())
    var searchQuery by remember { mutableStateOf("") }
    
    // Check if user is admin
    val isAdmin = userViewModel.isAdmin
    val currentUser by userViewModel.currentUser.collectAsState()

    Scaffold(
        floatingActionButton = {
            FloatingActionButton(onClick = { navController.navigate("addProduct") }) {
                Icon(Icons.Default.Add, contentDescription = "Add Product")
            }
        },
        topBar = {
            TopAppBar(
                title = { Text("Inventory") },
                actions = {
                    // Display current user
                    if (currentUser != null) {
                        TextButton(onClick = { navController.navigate("profile") }) {
                            Text(
                                text = currentUser!!.displayName ?: currentUser!!.email, 
                                style = MaterialTheme.typography.bodySmall
                            )
                        }
                    } else {
                         Text("Guest", style = MaterialTheme.typography.bodySmall, modifier = Modifier.padding(end = 8.dp))
                    }
                    
                    // Only admin can see Manage Users and Dashboard
                    if (isAdmin) {
                        IconButton(onClick = { navController.navigate("dashboard") }) {
                            Icon(Icons.Default.Home, contentDescription = "Admin Dashboard")
                        }
                        TextButton(onClick = { navController.navigate("manageUsers") }) {
                            Text("Users")
                        }
                    } else if (currentUser != null) {
                        // Regular authenticated users can see Profile icon if not clicked on name
                        IconButton(onClick = { navController.navigate("profile") }) {
                            Icon(Icons.Default.Person, contentDescription = "Profile")
                        }
                    }
                    
                    // Sign Out Button
                    IconButton(onClick = {
                        userViewModel.logout()
                        navController.navigate("login") {
                            popUpTo("productList") { inclusive = true }
                        }
                    }) {
                        Icon(Icons.Default.Close, contentDescription = "Sign Out")
                    }
                }
            )
        }
    ) { innerPadding ->
        Column(modifier = Modifier.padding(innerPadding)) {
            // Search Bar
            OutlinedTextField(
                value = searchQuery,
                onValueChange = { 
                    searchQuery = it
                    viewModel.setSearchQuery(it) // Updates the flow in ViewModel
                },
                label = { Text("Search by Name, Category, or Model") },
                leadingIcon = { Icon(Icons.Default.Search, contentDescription = null) },
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp)
            )
            
            Row(
                modifier = Modifier.fillMaxWidth().padding(horizontal = 8.dp),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                if (isAdmin) {
                    Button(onClick = { navController.navigate("alerts") }, modifier = Modifier.weight(1f).padding(end = 4.dp)) {
                        Text("Alerts")
                    }
                }
                Button(onClick = { navController.navigate("reports") }, modifier = Modifier.weight(1f).padding(start = 4.dp)) {
                    Text("Reports")
                }
            }

            LazyColumn {
                items(products) { product ->
                    ProductItem(product = product, onClick = {
                        navController.navigate("editProduct/${product.id}")
                    })
                }
            }
        }
    }
}

@Composable
fun ProductItem(product: Product, onClick: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(8.dp)
            .clickable { onClick() },
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(text = product.name, style = MaterialTheme.typography.titleMedium)
                if (!product.category.isNullOrBlank()) {
                    Text(text = "Category: ${product.category}", style = MaterialTheme.typography.bodySmall)
                }
                if (!product.model.isNullOrBlank()) {
                    Text(text = "Model: ${product.model}", style = MaterialTheme.typography.bodySmall)
                }
                // Updated currency to ZAR
                Text(text = "Price: ${String.format(Locale.US, "R%.2f", product.price)}", style = MaterialTheme.typography.bodyMedium)
                Text(text = "Qty: ${product.quantity}", style = MaterialTheme.typography.bodyMedium)
            }
            if (product.quantity <= product.lowStockThreshold) {
                Icon(Icons.Default.Warning, contentDescription = "Low Stock", tint = MaterialTheme.colorScheme.error)
            }
        }
    }
}
