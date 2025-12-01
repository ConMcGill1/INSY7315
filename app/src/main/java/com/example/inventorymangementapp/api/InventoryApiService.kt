package com.example.inventorymangementapp.api

import com.example.inventorymangementapp.model.Product
import retrofit2.http.*

interface InventoryApiService {
    @GET("api/products")
    suspend fun getProducts(): List<Product>

    @GET("api/products/{id}")
    suspend fun getProduct(@Path("id") id: Int): Product

    @POST("api/products")
    suspend fun createProduct(@Body product: Product): Product

    @PUT("api/products/{id}")
    suspend fun updateProduct(@Path("id") id: Int, @Body product: Product): Product

    @DELETE("api/products/{id}")
    suspend fun deleteProduct(@Path("id") id: Int)
}
