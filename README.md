# Online Shopping Backend System

## What is Online Shopping?

Online Shopping is a console-based e-commerce backend simulation built with .NET.  
It supports customer and administrator workflows including product management, cart handling, checkout, wallet-based payments, order tracking, reviews, and sales reporting.

The application is designed with a service-oriented structure (`Interfaces`, `Services`, `Menus`, `Models`, `Utilities`) and uses a JSON-backed local data store for persistence.

---

## Why Online Shopping?

### End-to-End E-commerce Flow  
The system supports the full lifecycle from browsing products to checkout, payment, and order tracking.

### Role-Based Access  
Users can register/login as `Customer` or `Administrator`, and each role sees only relevant actions.

### Persistent JSON Data Store  
Data is saved to `Data/database.json`, so users, products, orders, and payments persist between runs.

### Seeded for Immediate Use  
Default accounts and products are created automatically on first run.


---

# Documentation

## Software Requirements Specification (SRS)

### Overview

Online Shopping is a console application for managing core e-commerce operations.  
It enables authentication, product catalog browsing/searching, cart operations, checkout with wallet payments, order status management, and reporting.

---

## Roles and Permissions

| Role | How Obtained | Access Level |
|---|---|---|
| Administrator | Register as admin or use seeded admin account | Full product/order/report management |
| Customer | Register as customer or use seeded customer account | Browse/search products, manage cart, checkout, add funds, track orders, review purchased products |

Role restrictions are enforced by menu routing and service-level checks.

---

## Application Structure

### Navigation by Role

#### Customer
- Browse Products
- Search Products
- Add Product to Cart
- View / Update / Clear Cart
- Checkout
- View Wallet / Add Funds
- View Order History / Track Order
- Review Purchased Products

#### Administrator
- Add / Update / Delete Product
- Restock Product
- View Products
- View Orders
- Update Order Status
- View Low Stock Products
- Generate Sales Reports

---

## Workflow Model

### Typical Customer Flow

1. Login or register as customer
2. Browse or search products
3. Add items to cart
4. Add wallet funds if needed
5. Checkout
6. Track order and leave product reviews

### Typical Admin Flow

1. Login as administrator
2. Maintain product catalog
3. Monitor and update orders
4. Review low-stock items
5. Generate sales reports

---

## Services Architecture

The application uses interface-driven services:

- `IAuthService` / `AuthService`
- `IProductService` / `ProductService`
- `ICartService` / `CartService`
- `IOrderService` / `OrderService`
- `IPaymentService` / `PaymentService`
- `IReportService` / `ReportService`

Each service encapsulates business rules and is wired in `Program.cs`.

---

## Authentication and Authorization

### Authentication Flow

1. User registers with username/password and role
2. User logs in with credentials
3. System routes the user to role-specific menu

### Authorization Model

- `MainMenu` routes users by concrete type (`Customer` or `Administrator`)
- Customer-only and admin-only operations are separated by menu/service boundaries

---

## Data Persistence and Seeding

### JSON Database

- Primary data file: `Data/database.json`
- Managed by `AppDataContext`
- Automatically loads on app start
- Saves on create/update/delete operations

### Seed Data

On first run (empty data store), the system seeds:

- Admin account: `admin / admin123`
- Customer account: `customer / cust123` (wallet preloaded)
- Initial product catalog entries

---

## Reporting

### Built-in Reports
- Sales report generation
- Low-stock product reporting
- Order visibility for admin operations

Reports are generated from persisted application state.

---

## Technology Stack

- Runtime: .NET 10
- Language: C#
- App Type: Console Application
- Data Storage: JSON file (`System.Text.Json`)
- Architecture: Interface + Service + Menu separation

---

## Environment Variables

No environment variables are required for local execution.

---

## Running the Application

### Local Setup

1. Clone the repository

```bash
git clone https://github.com/RRusso15/online-shopping
cd online-shopping/OnlineShopping
```

2. Restore dependencies

```bash
dotnet restore
```

3. Build

```bash
dotnet build
```

4. Run

```bash
dotnet run
```

---

## Build and Quality Settings

The project is configured to be strict:

- Nullable reference types enabled
- Latest analyzer level enabled
- .NET analyzers enabled
- Code style enforced during build
- Warnings treated as errors

These settings are defined in `OnlineShopping.csproj`.

---

## Development Guidelines

- Keep business logic in `Services`
- Keep user interaction logic in `Menus`
- Keep models focused on domain representation
- Validate inputs at boundaries (`InputHelper` + service guards)
- Persist changes through `AppDataContext.SaveChanges()`

---

## Project Management

- Use feature branches per change
- Keep commits focused and atomic
- Run `dotnet build` before merging
- Update documentation when behavior changes

---

## License

This project is developed for academic and demonstration purposes.

