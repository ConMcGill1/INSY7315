package com.example.inventorymangementapp.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.inventorymangementapp.model.User
import com.example.inventorymangementapp.viewmodel.UserViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ManageUsersScreen(
    navController: NavController,
    viewModel: UserViewModel
) {
    val users by viewModel.allUsers.collectAsState(initial = emptyList())

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Manage Users") })
        }
    ) { innerPadding ->
        Column(modifier = Modifier.padding(innerPadding)) {
            
            // Equivalent to the Razor Page table
            LazyColumn {
                // Header row
                item {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp),
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text("Email", modifier = Modifier.weight(1f), style = MaterialTheme.typography.titleSmall)
                        Text("Display Name", modifier = Modifier.weight(1f), style = MaterialTheme.typography.titleSmall)
                        Text("Admin?", modifier = Modifier.weight(0.5f), style = MaterialTheme.typography.titleSmall)
                        Text("Actions", modifier = Modifier.weight(1f), style = MaterialTheme.typography.titleSmall)
                    }
                    HorizontalDivider()
                }

                items(users) { user ->
                    UserRowItem(user = user, onToggleAdmin = {
                        viewModel.updateAdminStatus(user.id, !user.isAdmin)
                    })
                    HorizontalDivider()
                }
            }
        }
    }
}

@Composable
fun UserRowItem(user: User, onToggleAdmin: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = user.email,
            modifier = Modifier.weight(1f),
            style = MaterialTheme.typography.bodyMedium
        )
        
        Text(
            text = user.displayName ?: "-",
            modifier = Modifier.weight(1f),
            style = MaterialTheme.typography.bodyMedium
        )
        
        Text(
            text = if (user.isAdmin) "Yes" else "No",
            modifier = Modifier.weight(0.5f),
            style = MaterialTheme.typography.bodyMedium
        )
        
        Box(modifier = Modifier.weight(1f)) {
            if (user.isAdmin) {
                OutlinedButton(
                    onClick = onToggleAdmin,
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
                ) {
                    Text("Remove Admin")
                }
            } else {
                OutlinedButton(
                    onClick = onToggleAdmin
                ) {
                    Text("Add to Admin")
                }
            }
        }
    }
}
