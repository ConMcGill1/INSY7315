package com.example.inventorymangementapp.ui.screens

import android.net.Uri
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.service.PdfService
import com.example.inventorymangementapp.viewmodel.InventoryViewModel

@Composable
fun ReportsScreen(
    navController: NavController,
    viewModel: InventoryViewModel
) {
    val context = LocalContext.current
    val pdfService = remember { PdfService(context) }
    val products by viewModel.allProducts.collectAsState(initial = emptyList())

    // Launcher for creating a real PDF file
    val createDocumentLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.CreateDocument("application/pdf")
    ) { uri: Uri? ->
        if (uri != null) {
            if (pdfService.generateProductReport(products, uri)) {
                Toast.makeText(context, "PDF Report saved successfully!", Toast.LENGTH_LONG).show()
            } else {
                Toast.makeText(context, "Failed to save PDF report.", Toast.LENGTH_SHORT).show()
            }
        }
    }
    
    // Launcher for creating a CSV file
    val createCsvLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.CreateDocument("text/csv")
    ) { uri: Uri? ->
        if (uri != null) {
            if (pdfService.generateProductCsv(products, uri)) {
                Toast.makeText(context, "CSV saved successfully!", Toast.LENGTH_LONG).show()
            } else {
                Toast.makeText(context, "Failed to save CSV.", Toast.LENGTH_SHORT).show()
            }
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("Reports & Exports", style = MaterialTheme.typography.headlineMedium)
        Spacer(modifier = Modifier.height(32.dp))
        
        Text("Export your inventory data to local storage.", style = MaterialTheme.typography.bodyMedium)
        Spacer(modifier = Modifier.height(16.dp))
        
        Button(
            onClick = {
                // Using .pdf extension for real PDF
                createDocumentLauncher.launch("inventory_report_${System.currentTimeMillis()}.pdf")
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Export Report (PDF)")
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Button(
            onClick = {
                createCsvLauncher.launch("inventory_export_${System.currentTimeMillis()}.csv")
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Export as CSV")
        }
    }
}
