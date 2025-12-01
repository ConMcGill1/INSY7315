package com.example.inventorymangementapp.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Info
import androidx.compose.material.icons.filled.KeyboardArrowUp
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.model.PriceHistory
import com.example.inventorymangementapp.viewmodel.InventoryViewModel
import java.text.SimpleDateFormat
import java.util.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    navController: NavController,
    viewModel: InventoryViewModel
) {
    val stats by viewModel.dashboardStats.collectAsState(initial = null)

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Admin Dashboard") }
            )
        }
    ) { innerPadding ->
        if (stats == null) {
             Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                 CircularProgressIndicator()
             }
        } else {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .verticalScroll(rememberScrollState())
                    .padding(16.dp)
            ) {
                // Summary Cards
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    DashboardCard(
                        title = "Total Products",
                        value = stats!!.totalProducts.toString(),
                        icon = Icons.Default.Info, // Changed to Core icon
                        modifier = Modifier.weight(1f)
                    )
                    DashboardCard(
                        title = "Total Value",
                        value = String.format(Locale.US, "R%.2f", stats!!.totalValue), // Changed to R
                        icon = Icons.Default.Star, // Changed to Core icon
                        modifier = Modifier.weight(1f)
                    )
                }
                
                Spacer(modifier = Modifier.height(8.dp))
                
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    DashboardCard(
                        title = "Total Stock",
                        value = stats!!.totalStock.toString(),
                        icon = Icons.Default.Check, // Changed to Core icon
                        modifier = Modifier.weight(1f)
                    )
                }

                Spacer(modifier = Modifier.height(24.dp))
                
                // Top Categories
                Text("Top Categories", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
                Spacer(modifier = Modifier.height(8.dp))
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant)
                ) {
                    Column(modifier = Modifier.padding(16.dp)) {
                         if (stats!!.topCategories.isEmpty()) {
                             Text("No data available")
                         } else {
                             stats!!.topCategories.forEach { entry ->
                                 Row(
                                     modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
                                     horizontalArrangement = Arrangement.SpaceBetween
                                 ) {
                                     Text(entry.key, style = MaterialTheme.typography.bodyLarge)
                                     Text("${entry.value} items", style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold)
                                 }
                                 HorizontalDivider()
                             }
                         }
                    }
                }

                Spacer(modifier = Modifier.height(24.dp))

                // Significant Price Changes
                Text("Major Price Changes (>10%)", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
                Spacer(modifier = Modifier.height(8.dp))
                
                if (stats!!.significantPriceChanges.isEmpty()) {
                    Text("No significant price changes detected recently.", style = MaterialTheme.typography.bodyMedium)
                } else {
                    stats!!.significantPriceChanges.forEach { history ->
                         PriceChangeCard(history)
                         Spacer(modifier = Modifier.height(8.dp))
                    }
                }
            }
        }
    }
}

@Composable
fun DashboardCard(
    title: String,
    value: String,
    icon: ImageVector,
    modifier: Modifier = Modifier
) {
    Card(
        modifier = modifier,
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(
            modifier = Modifier
                .padding(16.dp)
                .fillMaxWidth(),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Icon(icon, contentDescription = null, tint = MaterialTheme.colorScheme.primary)
            Spacer(modifier = Modifier.height(8.dp))
            Text(value, style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
            Text(title, style = MaterialTheme.typography.labelMedium)
        }
    }
}

@Composable
fun PriceChangeCard(history: PriceHistory) {
    val dateFormat = SimpleDateFormat("MMM dd, yyyy", Locale.getDefault())
    val dateStr = dateFormat.format(Date(history.changedDate))
    val change = history.newPrice - history.oldPrice
    val percent = if (history.oldPrice > 0) (change / history.oldPrice) * 100 else 0.0

    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.errorContainer)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(Icons.Default.KeyboardArrowUp, contentDescription = "Price Hike", tint = MaterialTheme.colorScheme.onErrorContainer)
            Spacer(modifier = Modifier.width(16.dp))
            Column {
                Text(
                    text = "Price Change on $dateStr", 
                    style = MaterialTheme.typography.labelLarge,
                    color = MaterialTheme.colorScheme.onErrorContainer
                )
                Text(
                    text = "Changed by: ${history.changedBy ?: "Unknown"}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onErrorContainer
                )
                Text(
                    text = "${String.format(Locale.US, "%.1f", percent)}% Increase (R${String.format(Locale.US, "%.2f", history.oldPrice)} -> R${String.format(Locale.US, "%.2f", history.newPrice)})",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onErrorContainer
                )
            }
        }
    }
}
