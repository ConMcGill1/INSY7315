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

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    navController: NavController,
    userViewModel: UserViewModel
) {
    val currentUser by userViewModel.currentUser.collectAsState()
    
    var displayName by remember { mutableStateOf(currentUser?.displayName ?: "") }
    var currentPassword by remember { mutableStateOf("") }
    var newPassword by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }
    
    var profileMessage by remember { mutableStateOf<String?>(null) }
    var passwordMessage by remember { mutableStateOf<String?>(null) }
    var passwordError by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Profile Management") })
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp)
        ) {
            if (currentUser == null) {
                Text("Please log in to view profile.")
                return@Column
            }

            Text("User Details", style = MaterialTheme.typography.headlineSmall)
            Spacer(modifier = Modifier.height(16.dp))
            
            Text("Email: ${currentUser!!.email}")
            Spacer(modifier = Modifier.height(8.dp))
            
            OutlinedTextField(
                value = displayName,
                onValueChange = { displayName = it },
                label = { Text("Display Name") },
                modifier = Modifier.fillMaxWidth()
            )
            
            Button(
                onClick = {
                    if (displayName.isNotBlank()) {
                        userViewModel.updateProfile(currentUser!!.id, displayName, null)
                        profileMessage = "Profile updated successfully."
                    }
                },
                modifier = Modifier.align(Alignment.End).padding(top = 8.dp)
            ) {
                Text("Update Profile")
            }
            
            if (profileMessage != null) {
                Text(profileMessage!!, color = MaterialTheme.colorScheme.primary)
            }
            
            Spacer(modifier = Modifier.height(32.dp))
            HorizontalDivider()
            Spacer(modifier = Modifier.height(16.dp))
            
            Text("Change Password", style = MaterialTheme.typography.headlineSmall)
            Spacer(modifier = Modifier.height(16.dp))
            
            OutlinedTextField(
                value = currentPassword,
                onValueChange = { currentPassword = it },
                label = { Text("Current Password") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth()
            )
            
            OutlinedTextField(
                value = newPassword,
                onValueChange = { newPassword = it },
                label = { Text("New Password") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth()
            )
            
            OutlinedTextField(
                value = confirmPassword,
                onValueChange = { confirmPassword = it },
                label = { Text("Confirm Password") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth()
            )
            
            Button(
                onClick = {
                    // Validate
                    if (currentPassword != currentUser!!.passwordHash) {
                        passwordMessage = "Incorrect current password."
                        passwordError = true
                    } else if (newPassword != confirmPassword) {
                        passwordMessage = "New passwords do not match."
                        passwordError = true
                    } else if (newPassword.isBlank()) {
                        passwordMessage = "New password cannot be empty."
                        passwordError = true
                    } else {
                        userViewModel.updateProfile(currentUser!!.id, null, newPassword)
                        passwordMessage = "Password updated successfully."
                        passwordError = false
                        currentPassword = ""
                        newPassword = ""
                        confirmPassword = ""
                    }
                },
                modifier = Modifier.align(Alignment.End).padding(top = 8.dp)
            ) {
                Text("Change Password")
            }
            
            if (passwordMessage != null) {
                Text(
                    text = passwordMessage!!, 
                    color = if (passwordError) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary
                )
            }
        }
    }
}
