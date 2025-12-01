package com.example.inventorymangementapp.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.viewmodel.UserViewModel

@Composable
fun LoginScreen(
    navController: NavController,
    userViewModel: UserViewModel
) {
    var username by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var displayName by remember { mutableStateOf("") }
    var error by remember { mutableStateOf<String?>(null) }
    var isRegisterMode by remember { mutableStateOf(false) }

    // Reset user state on entry (optional, keeps consistent state)
    LaunchedEffect(Unit) {
        userViewModel.logout()
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            text = if (isRegisterMode) "Register User" else "Inventory Login", 
            style = MaterialTheme.typography.headlineMedium
        )
        Spacer(modifier = Modifier.height(32.dp))
        
        OutlinedTextField(
            value = username,
            onValueChange = { username = it },
            label = { Text("Username / Email") },
            modifier = Modifier.fillMaxWidth()
        )
        Spacer(modifier = Modifier.height(8.dp))
        
        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            label = { Text("Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth()
        )
        Spacer(modifier = Modifier.height(8.dp))
        
        if (isRegisterMode) {
            OutlinedTextField(
                value = displayName,
                onValueChange = { displayName = it },
                label = { Text("Display Name") },
                modifier = Modifier.fillMaxWidth()
            )
            Spacer(modifier = Modifier.height(8.dp))
        }
        
        if (error != null) {
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = error!!, color = MaterialTheme.colorScheme.error)
        }
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Button(
            onClick = { 
                if (isRegisterMode) {
                    if (username.isNotBlank() && password.isNotBlank() && displayName.isNotBlank()) {
                        userViewModel.register(username, password, displayName)
                        isRegisterMode = false
                        error = "Account created. Please Login."
                        password = "" // Reset password field if they switch back
                    } else {
                        error = "All fields required"
                    }
                } else {
                    if (username.isNotBlank() && password.isNotBlank()) {
                        userViewModel.login(username, password) { success ->
                            if (success) {
                                navController.navigate("productList") {
                                    popUpTo("login") { inclusive = true }
                                }
                            } else {
                                error = "Invalid Username or Password"
                            }
                        }
                    } else {
                        error = "Username and Password required"
                    }
                }
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text(if (isRegisterMode) "Register" else "Login")
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        TextButton(onClick = { isRegisterMode = !isRegisterMode; error = null }) {
            Text(if (isRegisterMode) "Already have an account? Login" else "New User? Register Here")
        }
        
        Spacer(modifier = Modifier.height(8.dp))
        
        if (!isRegisterMode) {
            TextButton(
                onClick = {
                    // Guest Access (No login needed, just go to dashboard)
                    userViewModel.logout() // Ensures guest state
                    navController.navigate("productList") {
                        popUpTo("login") { inclusive = true }
                    }
                }
            ) {
                Text("Skip / Continue as Guest")
            }
        }
    }
}
