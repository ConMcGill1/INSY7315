package com.example.inventorymangementapp.service

import android.content.Context
import android.net.Uri
import com.example.inventorymangementapp.model.Product
import java.io.OutputStream

class PdfService(private val context: Context) {

    // Writes a report to the output stream obtained from the selected document Uri
    fun generateProductReport(products: List<Product>, uri: Uri): Boolean {
        return try {
            context.contentResolver.openOutputStream(uri)?.use { outputStream ->
                outputStream.bufferedWriter().use { writer ->
                    writer.write("Inventory Report (Simulated PDF)\n")
                    writer.write("Generated on: ${java.util.Date()}\n")
                    writer.write("--------------------------------------------------\n")
                    products.forEach { product ->
                        writer.write("ID: ${product.id} | Name: ${product.name} | Price: $${product.price} | Qty: ${product.quantity} | Category: ${product.category ?: "-"}\n")
                    }
                    writer.write("--------------------------------------------------\n")
                    writer.write("Total Products: ${products.size}\n")
                }
            }
            true
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }

    fun generateProductCsv(products: List<Product>, uri: Uri): Boolean {
        return try {
            context.contentResolver.openOutputStream(uri)?.use { outputStream ->
                outputStream.bufferedWriter().use { writer ->
                    // CSV Header
                    writer.write("ID,Name,Price,Quantity,LowStockThreshold,Category,Model,Owner\n")
                    // CSV Data
                    products.forEach { product ->
                        writer.write("${product.id},\"${product.name}\",${product.price},${product.quantity},${product.lowStockThreshold},\"${product.category ?: ""}\",\"${product.model ?: ""}\",\"${product.owner ?: ""}\"\n")
                    }
                }
            }
            true
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
}
