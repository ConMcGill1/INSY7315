package com.example.inventorymangementapp

import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.assertIsDisplayed
import androidx.test.ext.junit.runners.AndroidJUnit4
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

/**
 * Smoke Testing:
 * Verified application builds and runs.
 * Verifies the entry point (Login Screen) loads successfully.
 */
@RunWith(AndroidJUnit4::class)
class SmokeTest {

    @get:Rule
    val composeTestRule = createAndroidComposeRule<MainActivity>()

    @Test
    fun appLaunchesSuccessfully_showsLoginScreen() {
        // Check if the "Inventory Login" text is displayed
        // Note: Title might be "Inventory Login" or "Register User" depending on state, but defaults to Login.
        // Actually, logic defaults isRegisterMode=false, so "Inventory Login" is correct.
        composeTestRule.onNodeWithText("Inventory Login").assertIsDisplayed()
        
        // Check if the "Login" button exists
        composeTestRule.onNodeWithText("Login").assertIsDisplayed()
    }
}
