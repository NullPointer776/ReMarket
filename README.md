# ReMarket

A second-hand trading platform built with ASP.NET Core MVC, where users can buy and sell pre-owned items.

## Overview

ReMarket is a web application that connects buyers and sellers of second-hand goods. It provides a complete marketplace experience with item listings, image galleries, QR code sharing, shopping cart functionality, and secure payment processing.

## Features

### For Buyers
- Browse items with category-based navigation
- Search items by keyword
- Filter by category, condition, delivery option, and location
- Sort by posted date and price
- Add multiple items to shopping cart
- Secure checkout with Stripe payment integration

### For Sellers
- Create, edit, and delete item listings
- Upload cover images and image galleries (up to 8 images per item)
- Generate QR codes for item sharing
- Access camera directly to capture item photos
- Preview images before uploading
- View rejection reasons for moderated items

### For Administrators
- Full category management with hierarchical (self-referencing) structure
- Audit and moderate item listings (approve/reject with reasons)
- Search, filter, and sort items
- Manage orders and shipment details
- Track payment and order status

## Architecture

The solution follows a layered architecture with four projects:

| Project | Layer | Responsibility |
|---------|-------|----------------|
| `ReMarket.Web` | Presentation | ASP.NET Core MVC with Areas (Admin, Seller, Buyer), Controllers, Views, static assets |
| `ReMarket.DataAccess` | Data Access | Entity Framework Core DbContext, Repository Pattern, Unit of Work |
| `ReMarket.Models` | Domain | Entity classes, Enums, ViewModels |
| `ReMarket.Utility` | Cross-cutting | Helper classes, constants, file upload utilities |

## Tech Stack

- **Framework:** ASP.NET Core MVC
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Authentication:** ASP.NET Core Identity
- **Payment:** Stripe API
- **Frontend:** Bootstrap, Razor Views, Vanilla JavaScript
- **QR Codes:** QRCode NuGet package

## Key Implementation Details

### SEO-Friendly Routing
Categories and items use semantic slugs (e.g., `/item/vintage-camera`) instead of numeric IDs, with automatic uniqueness handling.

### Self-Referencing Categories
Categories support parent-child relationships, allowing nested subcategories with configurable depth.

### Image Management
- Cover image plus gallery support (up to 8 images)
- Client-side preview before upload
- Server-side validation for file type (JPG, JPEG, PNG, GIF, WebP), size (max 5MB), and quantity
- Organized storage by item slug

### QR Code Generation
Each item automatically gets a QR code linking to its public detail page, generated as a PNG file and stored on the server.

### Shopping Cart & Orders
- Persistent cart tied to user identity
- Quantity management with stock validation
- Order workflow from cart to payment confirmation

## Getting Started

### Prerequisites
- .NET SDK (latest version)
- SQL Server (LocalDB or full instance)
- Stripe account (for payment features)

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/NullPointer776/ReMarket.git
   cd ReMarket
2. Update the connection string in appsettings.json:
   ```bash
   "ConnectionStrings": {
   "DefaultConnection": "your-connection-string-here"
   }
   
3. Configure Stripe keys in appsettings.json:
   ```bash
     "Stripe": {
     "SecretKey": "your-stripe-secret-key",
     "PublishableKey": "your-stripe-publishable-key"
      }

4. Apply database migrations:
   ```bash
     dotnet ef database update

5.Run the application:
  ```bash
     dotnet run --project ReMarket.Web
   
