package com.example.inventorymangementapp.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.model.Product
import com.example.inventorymangementapp.service.PriceChangeService
import com.example.inventorymangementapp.viewmodel.InventoryViewModel
import com.example.inventorymangementapp.viewmodel.UserViewModel
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddEditProductScreen(
    navController: NavController,
    viewModel: InventoryViewModel,
    userViewModel: UserViewModel,
    productId: Int? = null
) {
    var name by remember { mutableStateOf("") }
    var price by remember { mutableStateOf("") }
    var quantity by remember { mutableStateOf("") }
    var lowStockThreshold by remember { mutableStateOf("") }
    var owner by remember { mutableStateOf("") }
    var model by remember { mutableStateOf("") }
    var category by remember { mutableStateOf("") }
    
    var errorMessage by remember { mutableStateOf<String?>(null) }
    var oldPrice by remember { mutableDoubleStateOf(0.0) }

    val scope = rememberCoroutineScope()
    val priceChangeService = remember { PriceChangeService() }
    val isAdmin = userViewModel.isAdmin
    
    // State for price history
    val priceHistory by if (productId != null && productId != -1) {
        viewModel.getPriceHistory(productId).collectAsState(initial = emptyList())
    } else {
        remember { mutableStateOf(emptyList()) }
    }

    LaunchedEffect(productId) {
        if (productId != null && productId != -1) {
            val product = viewModel.getProductById(productId)
            product?.let {
                name = it.name
                price = it.price.toString()
                oldPrice = it.price
                quantity = it.quantity.toString()
                lowStockThreshold = it.lowStockThreshold.toString()
                owner = it.owner ?: ""
                model = it.model ?: ""
                category = it.category ?: ""
            }
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(if (productId == null || productId == -1) "Add Product" else "Edit Product") }
            )
        }
    ) { innerPadding ->
        // Using LazyColumn to handle content + price history list gracefully
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp)
        ) {
            item {
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Name") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = owner,
                    onValueChange = { owner = it },
                    label = { Text("Owner") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))

                // Price field - ONLY ADMIN CAN EDIT if updating, but anyone can set initial price
                val canEditPrice = isAdmin || (productId == null || productId == -1)
                OutlinedTextField(
                    value = price,
                    onValueChange = { if (canEditPrice) price = it },
                    label = { Text("Price") },
                    enabled = canEditPrice,
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    modifier = Modifier.fillMaxWidth()
                )
                if (!canEditPrice) {
                    Text("Only Admins can change price", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.secondary)
                }
                Spacer(modifier = Modifier.height(8.dp))
                
                OutlinedTextField(
                    value = category,
                    onValueChange = { category = it },
                    label = { Text("Category (Optional)") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = model,
                    onValueChange = { model = it },
                    label = { Text("Model (Optional)") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = quantity,
                    onValueChange = { quantity = it },
                    label = { Text("Quantity") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = lowStockThreshold,
                    onValueChange = { lowStockThreshold = it },
                    label = { Text("Low Stock Threshold") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.fillMaxWidth()
                )

                if (errorMessage != null) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(text = errorMessage!!, color = MaterialTheme.colorScheme.error)
                }

                Spacer(modifier = Modifier.height(16.dp))

                Button(
                    onClick = {
                        val priceVal = price.toDoubleOrNull()
                        val qtyVal = quantity.toIntOrNull()
                        val threshVal = lowStockThreshold.toIntOrNull()

                        if (name.isNotBlank() && priceVal != null && qtyVal != null && threshVal != null && owner.isNotBlank()) {
                            if (priceVal < 0) {
                                errorMessage = "Price must be non-negative"
                            } else {
                                if (productId != null && productId != -1) {
                                    // Update Logic
                                    // Validate price change only if admin is changing it (since only admin CAN change it)
                                    if (canEditPrice && priceVal != oldPrice && !priceChangeService.validatePriceChange(oldPrice, priceVal)) {
                                        errorMessage = "Invalid price change (>10% increase)."
                                    } else {
                                        // Save
                                        viewModel.updateProduct(
                                            Product(
                                                id = productId,
                                                name = name,
                                                price = priceVal,
                                                quantity = qtyVal,
                                                lowStockThreshold = threshVal,
                                                owner = owner,
                                                model = model,
                                                category = category
                                            )
                                        )
                                        navController.popBackStack()
                                    }
                                } else {
                                    // Create Logic
                                    viewModel.addProduct(
                                        Product(
                                            name = name,
                                            price = priceVal,
                                            quantity = qtyVal,
                                            lowStockThreshold = threshVal,
                                            owner = owner,
                                            model = model,
                                            category = category
                                        )
                                    )
                                    navController.popBackStack()
                                }
                            }
                        } else {
                            errorMessage = "Please fill required fields (Name, Owner, Price, Qty, Threshold)"
                        }
                    },
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Save")
                }

                // DELETE - ONLY ADMIN CAN DELETE
                if (productId != null && productId != -1) {
                    if (isAdmin) {
                        Spacer(modifier = Modifier.height(8.dp))
                        OutlinedButton(
                            onClick = {
                                scope.launch {
                                    val product = viewModel.getProductById(productId)
                                    if (product != null) {
                                        viewModel.deleteProduct(product)
                                        navController.popBackStack()
                                    }
                                }
                            },
                            modifier = Modifier.fillMaxWidth(),
                            colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
                        ) {
                            Text("Delete Product")
                        }
                    } else {
                        Spacer(modifier = Modifier.height(8.dp))
                        Text("Only Admins can delete products", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.error)
                    }
                }
                
                // Section for Price History
                if (productId != null && productId != -1) {
                    Spacer(modifier = Modifier.height(24.dp))
                    Text("Price History", style = MaterialTheme.typography.titleMedium)
                    HorizontalDivider()
                }
            }
            
            // History items
            if (productId != null && productId != -1) {
                if (priceHistory.isEmpty()) {
                    item {
                        Text("No price changes recorded.", modifier = Modifier.padding(8.dp), style = MaterialTheme.typography.bodySmall)
                    }
                } else {
                    items(priceHistory) { history ->
                        val dateFormat = SimpleDateFormat("dd MMM yyyy HH:mm", Locale.getDefault())
                        val dateStr = dateFormat.format(Date(history.changedDate))
                        
                        // Update to ZAR
                        ListItem(
                            headlineContent = { Text("New: ${String.format(Locale.US, "R%.2f", history.newPrice)} (was ${String.format(Locale.US, "R%.2f", history.oldPrice)})") },
                            supportingContent = { Text("By ${history.changedBy ?: "Unknown"} on $dateStr") }
                        )
                        HorizontalDivider()
                    }
                }
            }
        }
    }
}
