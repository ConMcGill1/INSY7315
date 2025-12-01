package com.example.inventorymangementapp.service

import android.content.Context
import android.graphics.Color
import android.graphics.Paint
import android.graphics.pdf.PdfDocument
import android.net.Uri
import com.example.inventorymangementapp.model.Product
import java.io.OutputStream
import java.util.Locale

class PdfService(private val context: Context) {

    // Writes a real PDF report using Android's PdfDocument
    fun generateProductReport(products: List<Product>, uri: Uri): Boolean {
        val pdfDocument = PdfDocument()
        val paint = Paint()

        try {
            context.contentResolver.openOutputStream(uri)?.use { outputStream ->
                
                var pageNumber = 1
                // A4 size in points: 595 x 842
                val pageWidth = 595
                val pageHeight = 842
                
                var pageInfo = PdfDocument.PageInfo.Builder(pageWidth, pageHeight, pageNumber).create()
                var page = pdfDocument.startPage(pageInfo)
                var canvas = page.canvas
                
                // Paint setup
                paint.color = Color.BLACK
                
                var y = 50f
                val x = 40f
                val lineHeight = 20f
                
                // Title
                paint.textSize = 18f
                paint.isFakeBoldText = true
                canvas.drawText("Inventory Report", x, y, paint)
                y += 30f
                
                // Metadata
                paint.textSize = 12f
                paint.isFakeBoldText = false
                canvas.drawText("Generated on: ${java.util.Date()}", x, y, paint)
                y += 30f
                
                // Separator
                paint.strokeWidth = 1f
                canvas.drawLine(x, y, pageWidth - 40f, y, paint)
                y += 30f
                
                // Table Header
                paint.isFakeBoldText = true
                // Simple columnar layout manual spacing
                // ID (0), Name (50), Price (250), Qty (350), Category (420)
                canvas.drawText("ID", x, y, paint)
                canvas.drawText("Name", x + 40, y, paint)
                canvas.drawText("Price", x + 200, y, paint)
                canvas.drawText("Qty", x + 300, y, paint)
                canvas.drawText("Category", x + 360, y, paint)
                
                paint.isFakeBoldText = false
                y += 25f
                
                for (product in products) {
                    // Check if we need a new page
                    if (y > pageHeight - 50f) {
                        pdfDocument.finishPage(page)
                        pageNumber++
                        pageInfo = PdfDocument.PageInfo.Builder(pageWidth, pageHeight, pageNumber).create()
                        page = pdfDocument.startPage(pageInfo)
                        canvas = page.canvas
                        y = 50f
                        
                        // Draw headers again on new page
                        paint.isFakeBoldText = true
                        canvas.drawText("ID", x, y, paint)
                        canvas.drawText("Name", x + 40, y, paint)
                        canvas.drawText("Price", x + 200, y, paint)
                        canvas.drawText("Qty", x + 300, y, paint)
                        canvas.drawText("Category", x + 360, y, paint)
                        paint.isFakeBoldText = false
                        y += 25f
                    }
                    
                    val priceStr = String.format(Locale.US, "R%.2f", product.price)
                    val nameStr = if (product.name.length > 20) product.name.substring(0, 17) + "..." else product.name
                    val categoryStr = product.category ?: "-"
                    val catDisplay = if (categoryStr.length > 15) categoryStr.substring(0, 12) + "..." else categoryStr
                    
                    canvas.drawText(product.id.toString(), x, y, paint)
                    canvas.drawText(nameStr, x + 40, y, paint)
                    canvas.drawText(priceStr, x + 200, y, paint)
                    canvas.drawText(product.quantity.toString(), x + 300, y, paint)
                    canvas.drawText(catDisplay, x + 360, y, paint)
                    
                    y += lineHeight
                }
                
                y += 10f
                canvas.drawLine(x, y, pageWidth - 40f, y, paint)
                y += 30f
                paint.isFakeBoldText = true
                canvas.drawText("Total Products: ${products.size}", x, y, paint)
                
                pdfDocument.finishPage(page)
                
                // Write to file
                pdfDocument.writeTo(outputStream)
            }
            return true
        } catch (e: Exception) {
            e.printStackTrace()
            return false
        } finally {
            pdfDocument.close()
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
