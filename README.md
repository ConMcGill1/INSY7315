# Inventory Management Mobile App

This is the mobile client for the Inventory Management System, built with Android Jetpack Compose. It replicates the core features of the web backend, ensuring security, data integrity, and efficient management on mobile devices.

## Executive Summary
The Inventory Management System provides a secure and efficient solution for small retailers to manage products, track historical price changes, receive alerts on significant price movements, and export reports. It was developed using **Android Jetpack Compose** (UI), **Room** (Database), and **Kotlin**. It mirrors the functionality of the original ASP.NET Core MVC application.

## System Architecture and Design
**Architecture**: MVVM (Model-View-ViewModel) with clean separation of concerns.
*   **App**: Jetpack Compose UI Layer.
*   **Data**: Room Database (`AppDatabase`) with DAOs (`ProductDao`, `UserDao`).
*   **Services**: 
    *   `PriceChangeService`: Validates price changes (prevents >10% increase without warning).
    *   `PdfService`: Generates inventory reports and exports via Android's Storage Access Framework (SAF).
*   **Auth**: Custom authentication logic in `UserViewModel` supporting Admin, Regular, and Guest roles.

## Implemented Features (Final)

### Core Features
*   **User Authentication**: 
    *   **Login/Register**: New users can register (stored in DB).
    *   **Admin Access**: Secured with credentials (`admin` / `Admin123!`).
    *   **Guest Access**: View-only mode without registration.
    *   **Sign Out**: Switch users easily from the dashboard.
    *   **Profile**: Update Display Name and Password.
*   **Product CRUD**: Add, View, Edit, Delete products with validation.
*   **Price Change Tracking**: Automatically records price changes in `PriceHistory` table.
*   **Search & Filter**: Search by Name, Category, or Model.
*   **Currency**: All monetary values displayed in **ZAR (R)**.

### Secondary Features
*   **Reports & Exports**: 
    *   **Storage Access**: Uses Android system picker to let users choose where to save files.
    *   **Formats**: Export inventory as **Real PDF (.pdf)** or **CSV**.
*   **Price Change Alerts/Validation**: Prevents price increases greater than 10% (aligned with backend rules).
*   **Alerts System**: Dedicated view for low-stock items (Admin only).
*   **User Management**: Admin can manage user roles (add/remove admin rights).
*   **Admin Dashboard**: Visual summary of inventory stats, stock levels, and major price changes.

## Database Design (SQLite/Room)
Primary tables:
*   **Products** (`id`, `name`, `owner`, `price`, `category`, `model`, `lowStockThreshold`)
*   **PriceHistories** (`id`, `productId`, `oldPrice`, `newPrice`, `changedOn`)
*   **Alerts** (`id`, `productId`, `message`, `timestamp`)
*   **Users** (`id`, `email`, `passwordHash`, `displayName`, `isAdmin`)

## Tech Stack
*   **Language**: Kotlin
*   **UI**: Jetpack Compose (Material Design 3)
*   **Database**: Room (SQLite)
*   **Navigation**: Jetpack Navigation Compose
*   **Networking**: Retrofit (configured for API integration)
*   **Compatibility**: API Level 24+ (Android 7.0+)

## Setup & Running
1.  **Prerequisites**: Android Studio Ladybug+, JDK 11+.
2.  **Import**: Open `InventoryMangementApp` folder in Android Studio.
3.  **Build**: Sync Gradle and `Build > Make Project`.
4.  **Run**: Deploy to Emulator or Device via USB.

## How to Use
1.  **Authentication**:
    *   **Register**: Click "New User? Register Here" to create a persistent account.
    *   **Login**: Use your credentials or the admin defaults (`admin` / `Admin123!`).
    *   **Guest**: "Skip / Continue as Guest" for read-only access.
2.  **Inventory**:
    *   Use the **FAB (+)** to add products.
    *   Search using the top bar.
3.  **Reports**:
    *   Navigate to "Reports".
    *   Click "Export Report" or "Export CSV".
    *   **System Picker**: Choose a folder and filename to save the file securely.
4.  **Profile & Admin Actions**:
    *   **Profile**: Click your display name in the top bar to update profile or change password.
    *   **Dashboard**: (Admin only) Click the Home/Grid icon to view top stats.
    *   **Alerts**: (Admin only) View low-stock warnings.
    *   **Manage Users**: (Admin only) Grant/Revoke admin access via top bar menu.

## Testing & Quality Assurance
*   **Unit Testing**: `PriceChangeService` logic verified (10% threshold, ZAR formatting).
*   **Integration**: Verified Auth flow -> Admin Privileges -> Database Persistence.
*   **Security**: Restricted "Delete" and "Edit Price" buttons to Admin users only.

## Conclusion
The mobile app is feature-complete, mirroring the backend's logic for price validation (10% threshold) and role-based security. It is ready for deployment or UAT.
