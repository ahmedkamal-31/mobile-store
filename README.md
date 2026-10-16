# 📱 Mobile Store

A full-stack mobile e-commerce web application built with **ASP.NET Core MVC**.
The platform provides product management, seller accounts, role-based access, and a complete workflow for managing mobile phone products.

🌐 **Live Demo:** [mobile-store](http://mobile-store.somee.com/)

## ✨ Features

### 🛍️ Customer Features

* Browse mobile phones
* Search products
* View product details
* Filter and explore available products
* Responsive user interface

### 👨‍💼 Seller Portal

* Dedicated seller dashboard
* Add new mobile phones
* Edit product information
* Delete products
* Manage uploaded product images
* View seller-owned products

### 🔐 Authentication & Authorization

* ASP.NET Core Identity
* User registration and login
* Role-based authorization
* Seller-specific access control
* Protected seller management pages

### 📦 Product Management

Each mobile phone can contain information such as:

* Name
* Brand
* Color
* Storage
* RAM
* Condition
* Price
* Image

Products use generated slugs based on relevant product information to provide cleaner URLs.

### 🔔 Notifications

* Seller-related notifications
* Product management feedback
* User-friendly success and error messages

## 🧰 Technologies

* **C#**
* **ASP.NET Core MVC**
* **Entity Framework Core**
* **ASP.NET Core Identity**
* **SQL Server**
* **Razor Views**
* **HTML5**
* **CSS3**
* **JavaScript**
* **Git & GitHub**

## 🏗️ Architecture

The project follows the **MVC architecture**:

```text
Mobile-Store/
│
├── Areas/
├── Controllers/
├── Data/
├── Models/
├── ViewModels/
├── Views/
├── Migrations/
├── wwwroot/
│
├── Program.cs
├── appsettings.json
└── MobileStore.csproj
```

## 🗄️ Database

**Entity Framework Core** is used for database access and Code First migrations.

The application manages:

* Products
* Users
* Sellers
* Roles
* Product-related data
* Notifications

## 🔒 Seller Data Isolation

Sellers can manage their own products while access to product management operations is controlled through authentication and authorization.

This helps prevent sellers from modifying products belonging to other sellers.

## 🚀 Getting Started

### Prerequisites

* .NET SDK
* Visual Studio
* SQL Server / SQL Server Express
* Git

### Installation

Clone the repository:

```bash
git clone https://github.com/ahmedkamal-31/mobile-store.git
```

Open the project:

```bash
cd mobile-store
```

Restore dependencies:

```bash
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```

Or open the solution in Visual Studio and run the project.

## 🎯 Project Goals

This project was developed to practice building a realistic e-commerce application using **ASP.NET Core MVC** and to apply practical concepts including:

* MVC architecture
* Entity Framework Core
* Code First migrations
* ASP.NET Identity
* Role-based authorization
* CRUD operations
* Seller portals
* Data isolation
* Search functionality
* Image uploads
* Form validation
* SQL Server integration

## 👨‍💻 Author

**Ahmed Kamal Mohamed**

Junior .NET Developer focused on:

**C# · ASP.NET Core · MVC · Entity Framework Core · SQL Server**

GitHub:
https://github.com/ahmedkamal-31
