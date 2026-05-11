# H2BIG System — Tech Stack & Detailed Mapping Guide

## Agent-Hub Audit Route
> **Task Class: G — Verification (Audit)**  
> **Status:** Audit-Certified and fully aligned with the updated `H2BIG_FLOW.md`.

---

## Final Tech Stack (Confirmed)
- **Backend:** ASP.NET Core MVC (.NET 8)
- **Database:** MySQL (XAMPP) via **Raw SQL** (MySqlConnector)
- **Auth:** Session-based (Roles: Admin, Staff, Rider)
- **Frontend:** Razor Views (.cshtml) + Tailwind (from Stitch)
- **Reports:** Browser Print (`window.print()`)

---

## 🗺️ Navigation & Page Mapping Guide
### 1. Authentication (Login)
| Stitch Folder Path | C# Route (URL) | Controller / Action |
| :--- | :--- | :--- |
| `Stitch-Final-Design/LOGIN/` | `/Auth/Login` | `AuthController.Login` |

### 2. Admin Pages
| Category | Stitch Folder Path | C# Route (URL) | Controller / Action |
| :--- | :--- | :--- | :--- |
| **Dashboard** | `.../ADMIN/admin_dashboard_...` | `/Admin/Dashboard` | `AdminController.Index` |
| **Customers** | `.../ADMIN/customer_management_...` | `/Admin/Customers` | `CustomerController.Index` |
| **POS (Walk-In)** | `.../ADMIN/sales_pos_...h2big_1` | `/Admin/POS/WalkIn` | `SalesController.WalkIn` |
| **POS (Delivery)** | `.../ADMIN/sales_pos_...h2big_2` | `/Admin/POS/Delivery` | `SalesController.Delivery` |
| **Bottle Ledger** | `.../ADMIN/bottle_ledger/` | `/Admin/BottleLedger` | `BottleController.Index` |
| **Deliveries** | `.../ADMIN/assigned_deliveries_...` | `/Admin/Deliveries` | `DeliveryController.Index` |
| **Inventory** | `.../ADMIN/product_inventory_...` | `/Admin/Inventory` | `InventoryController.Index` |
| **Admin Reports** | `.../ADMIN/admin_reports/` | `/Admin/Reports` | `ReportController.Index` |

### 3. Staff Pages
| Category | Stitch Folder Path | C# Route (URL) | Controller / Action |
| :--- | :--- | :--- | :--- |
| **Dashboard** | `.../STAFF/staff_dashboard_...` | `/Staff/Dashboard` | `StaffController.Index` |
| **Customers** | `.../STAFF/customer_management_...` | `/Staff/Customers" | `CustomerController.Index` |
| **POS (Walk-In)** | `.../STAFF/sales_pos_...Walk-In" | `/Staff/POS/WalkIn` | `SalesController.WalkIn` |
| **POS (Delivery)** | `.../STAFF/sales_pos_...Delivery" | `/Staff/POS/Delivery` | `SalesController.Delivery` |
| **Bottle Ledger** | `.../STAFF/bottle_ledger/` | `/Staff/BottleLedger` | `BottleController.Index` |
| **Deliveries** | `.../STAFF/assigned_deliveries_...` | `/Staff/Deliveries" | `DeliveryController.Index` |

---

## 📂 Project Folder Structure (C#)
```
H2BIG/
├── Controllers/         
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── StaffController.cs
│   ├── CustomerController.cs
│   ├── ProductController.cs
│   ├── SalesController.cs
│   ├── BottleController.cs
│   ├── DeliveryController.cs
│   ├── ReportController.cs
│   └── UserController.cs
├── Models/              
├── Data/                # DatabaseHelper.cs
├── Views/               
├── wwwroot/             
├── appsettings.json     
└── Program.cs           
```

---

## Phased Build Plan

### Phase 0 — Prerequisites & Project Setup 🛠️
**Goal:** Prepare your environment, download dependencies, and initialize the C# project in VS Code/Antigravity.

**📦 Required Downloads:**
1.  **.NET 8 SDK** — [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2.  **XAMPP** — [Download here](https://www.apachefriends.org/download.html)

**1. Verify .NET Installation:**
Open your VS Code terminal and run:
```powershell
dotnet --version
```
*(You should see `8.x.x`)*

**2. Database Preparation (XAMPP):**
- Start **Apache** and **MySQL** in the XAMPP Control Panel.
- Go to `localhost/phpmyadmin` and create a database named `h2big_db`.

**3. Project Initialization (VS Code Terminal):**
Run these commands to create the project and install NuGet dependencies:
```powershell
# Create the project
dotnet new mvc -n H2BIG

# Enter the project folder
cd H2BIG

# Install MySQL and Session dependencies
dotnet add package MySqlConnector
dotnet add package Microsoft.AspNetCore.Session
```

**4. Running the System:**
Use `dotnet run` to start the app and open the provided localhost URL.

---

### Phase 1 — Database Design (XAMPP/phpMyAdmin) 🗄️
**Goal:** Design and create all MySQL tables, relationships, and seed data in your local XAMPP.

**Deliverables:**
- ERD (Entity Relationship Diagram)
- SQL schema script (`h2big_db.sql`)
- **Tables to Create:**
    - `users`: id, fullname, username, password, role [Admin/Staff/Rider], status [Active/Inactive], time_in, time_out
    - `customers`: id, name, contact, address, bottle_debt, created_at (timestamp for "New Customer" metrics)
    - `products`: id, name, price, stock
    - `sales`: id, customer_id (NULL for Walk-In), total_amount, type [Walk-In/Delivery], date_time, staff_id, rider_id
    - `sale_items`: id, sale_id, product_id, quantity, subtotal
    - `deliveries`: id, sale_id, rider_id, status [Pending/Delivered/Cancelled]
    - `bottle_ledger`: id, customer_id, bottles_out, bottles_in, balance, transaction_id, date
    - `expenses`: id, description, amount, date
- **Seed Data:** 1 Admin user, 1 Staff user, 1 Rider user, sample customers, and products for demo purposes.

---

### Phase 2 — Backend Development 🔧
1.  **Auth Module:** Implement role-based redirection and role-aware Session logic.
2.  **Customer Management Logic (Admin & Staff):**
    - Search logic by Customer Name.
    - Summary queries: Count of total active customers, sum of total bottle debt, and count of new customers added in the current month.
    - CRUD logic for adding/editing customers (Name, Contact, Address).
3.  **Transaction & POS Logic (Admin & Staff):** 
    - **Walk-In:** Logic for immediate sales using "Walk-In Customer" profile.
    - **Delivery:** Customer search integration, Rider availability check, and Rider assignment modal logic.
4.  **Bottle Ledger Logic (Admin & Staff):** 
    - Search by Customer Name/ID.
    - Query transaction history to calculate "Running Balance" (Bottles Out - Bottles In).
5.  **Assigned Deliveries Logic (Admin & Staff):** 
    - Fetch active deliveries with assigned rider names.
    - Action logic: "Mark as Delivered" (updates status and ledger), "Cancel Order," and "Details."
6.  **Product Inventory Logic (Admin Only):** 
    - Search by Product Name.
    - CRUD logic for adding/editing/deleting products (Price and Stock management).
7.  **User & Employee Management Logic (Admin Only):** 
    - Table logic for displaying User ID, Full Name, Username, Role, and Status.
    - Tracking logic for "Time In" and "Time Out" based on system logs.
    - CRUD logic for adding new Staff/Rider accounts.
8.  **Staff Dashboard Logic:**
    - Calculate "Today's Sales Count" and "Pending Deliveries".
    - Aggregate "Bottles In/Out" (Returned vs Dispatched).
    - Query "Upcoming Deliveries" table data sourced from active sales.
9.  **Admin Reports Logic:**
    - Calculate Sales Today, Pending Deliveries, Total Bottle Debt.
    - Aggregate P&L Chart data (Revenue from `sales` vs Expenses from `expenses`).
    - Identify Critical Debtors for "Debt Alerts" table.

---

### Phase 3 — Frontend Integration 🎨
1.  **Layout Porting:** Use Stitch sidebars. (Admin sidebar order: Dashboard -> Customers -> POS -> Bottle Ledger -> Deliveries -> Inventory -> Admin Reports).
2.  **View Porting:** Port all Stitch layouts, including the new **Admin Reports** dashboard.
3.  **Interactions:** Modal for Rider selection; Chart.js integration for P&L chart on Admin Reports.
4.  **Print Styling:** Browser-print CSS for receipts and reports.

---

### Phase 4 — Polish & Verification ✅
1.  **Validation & Security:** Implement role-based [Authorize] logic.
2.  **Verification:** Complete end-to-end walkthrough for all Admin and Staff flows.
