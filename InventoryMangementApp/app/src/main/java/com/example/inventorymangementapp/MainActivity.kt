package com.example.inventorymangementapp

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.inventorymangementapp.ui.screens.*
import com.example.inventorymangementapp.ui.theme.InventoryMangementAppTheme
import com.example.inventorymangementapp.viewmodel.InventoryViewModel
import com.example.inventorymangementapp.viewmodel.UserViewModel

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            InventoryMangementAppTheme {
                val navController = rememberNavController()
                val inventoryViewModel: InventoryViewModel = viewModel()
                val userViewModel: UserViewModel = viewModel()

                NavHost(navController = navController, startDestination = "login") {
                    composable("login") {
                        LoginScreen(navController, userViewModel)
                    }
                    composable("productList") {
                        ProductListScreen(navController, inventoryViewModel, userViewModel)
                    }
                    composable("addProduct") {
                        AddEditProductScreen(navController, inventoryViewModel, userViewModel)
                    }
                    composable(
                        "editProduct/{productId}",
                        arguments = listOf(navArgument("productId") { type = NavType.IntType })
                    ) { backStackEntry ->
                        val productId = backStackEntry.arguments?.getInt("productId")
                        AddEditProductScreen(navController, inventoryViewModel, userViewModel, productId)
                    }
                    composable("alerts") {
                        AlertsScreen(navController, inventoryViewModel)
                    }
                    composable("reports") {
                        ReportsScreen(navController, inventoryViewModel)
                    }
                    composable("manageUsers") {
                        ManageUsersScreen(navController, userViewModel)
                    }
                    composable("dashboard") {
                        DashboardScreen(navController, inventoryViewModel)
                    }
                    composable("profile") {
                        ProfileScreen(navController, userViewModel)
                    }
                }
            }
        }
    }
}
