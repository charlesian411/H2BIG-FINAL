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

## 🗺️ Role-Based Navigation Mapping
This guide defines which links are visible in the **Global Sidebar** for each role.

| Category | C# Route (URL) | Admin | Staff | Rider |
| :--- | :--- | :---: | :---: | :---: |
| **Dashboard** | `/[Role]/Dashboard` | ✅ | ✅ | ✅ |
| **Customers** | `/Customer/Index` | ✅ | ✅ | ❌ |
| **POS/Sales** | `/Sales/POS` | ✅ | ✅ | ❌ |
| **Bottle Ledger** | `/Bottle/Ledger` | ✅ | ✅ | ❌ |
| **Deliveries** | `/Delivery/Index` | ✅ | ✅ | ❌ |
| **Inventory** | `/Inventory/Index` | ✅ | ❌ | ❌ |
| **Admin Reports** | `/Report/Index` | ✅ | ❌ | ❌ |

---

## 📂 Project Folder Structure (C#)
```
H2BIG/
├── Controllers/         
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── StaffController.cs
│   ├── RiderController.cs
│   ├── CustomerController.cs
│   ├── SalesController.cs
│   ├── BottleController.cs
│   ├── DeliveryController.cs
│   ├── InventoryController.cs
│   └── ReportController.cs
├── Models/              
├── Data/                
│   └── DatabaseHelper.cs
├── Views/               
│   ├── Shared/
│   │   └── _Layout.cshtml   # Global Dynamic Sidebar
│   ├── Auth/
│   │   └── Login.cshtml
│   ├── Admin/
│   │   └── Dashboard.cshtml
│   ├── Staff/
│   │   └── Dashboard.cshtml
│   ├── Rider/
│   │   └── Dashboard.cshtml
│   ├── Customer/
│   │   └── Index.cshtml
│   ├── Sales/
│   │   └── POS.cshtml
│   ├── Bottle/
│   │   └── Ledger.cshtml
│   ├── Delivery/
│   │   └── Index.cshtml
│   ├── Inventory/
│   │   └── Index.cshtml
│   └── Report/
│       └── Index.cshtml
├── wwwroot/             
├── appsettings.json     
└── Program.cs           
```

---

## Phased Build Plan

### Phase 0 — Prerequisites & Project Setup ✅ [COMPLETED]
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

### Phase 1 — Database Design (XAMPP/phpMyAdmin) ✅ [COMPLETED]
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

### Phase 2 — Backend Development 🔧 ✅ [COMPLETED]
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

### Phase 3 — Frontend Integration 🎨 [PENDING]

#### 1. Global Dynamic Sidebar (`_Layout.cshtml`)
**Goal:** Implement a single master sidebar that adapts to the logged-in role.
- **Architecture:** Use `@if (User.IsInRole("..."))` or Session-based role checks to show/hide the `<li>` items.
- **Global Branding & Icons:** As per the `ADMIN/bottle_ledger` design, the global sidebar must strictly use:
    - **Logo:** Water drop icon + "H2BIG Ledger" text.
    - **Dashboard Icon:** 2x2 grid.
    - **Customers Icon:** People group.
    - **POS/Sales Icon:** Cash register.
    - **Bottle Ledger Icon:** Connected nodes/route.
    - **Deliveries Icon:** Delivery truck.
    - **Inventory Icon:** Package/box.
    - **Admin Reports Icon:** Bar chart.
- **Role Visibility:**
    - **Admin:** All links visible.
    - **Staff:** (Dashboard, Customers, POS, Ledger, Deliveries).
    - **Rider:** Only (Rider Dashboard).
- **Active State:** JavaScript snippet to add the `bg-blue-600` class to the link matching `window.location.pathname`.
- **Profile Section (Bottom Left):** 
    - Dynamic Role Name (using the Shield/User icon) & Logout button (using the Exit/Door icon).
- **Cleanup:** Remove "New Transaction" placeholder.

#### 2. Login Page (`Auth/Login.cshtml`)
- **UI:** Centralized card with "H2BIG Ledger" branding.
- **Form:** Username and Password fields with Tailwind focus states.

#### 3. Admin Dashboard (`Admin/Dashboard.cshtml`)
- **Summary Cards:** 4 High-visibility cards (Revenue, Active Orders, Bottle Debt, Profit).
- **Sales Trends Chart:** Integrate **Chart.js** for the bar chart.
- **Urgent Debt Alerts:** Table showing the top 5 debtors.

#### 4. Staff Dashboard (`Staff/Dashboard.cshtml`)
- **Metrics:** Today's Sales, Pending Deliveries, Bottles In/Out (progress bar).
- **Upcoming Deliveries:** Table with color-coded status badges (LOADED, PENDING, DELAYED).

#### 5. Rider Dashboard (`Rider/Dashboard.cshtml`)
- **Goal:** Simplified one-page view for delivery tracking.
- **UI:** 2 Summary cards with progress indicators as seen in `RIDER/screen.png`.
- **Metrics:** 
    - **Out for Delivery:** Real-time count of bottles assigned to the rider.
    - **Total Bottles Delivered:** Cumulative successful completions.

#### 6. POS / Sales (`Sales/POS.cshtml`) ✅ [COMPLETED]
- **Mode Toggle:** Switch between "Walk-In" and "Delivery".
- **Product Grid:** Responsive grid with stock counts and +/- logic.
- **Order Summary:** Sidebar calculating Subtotal, Tax, and Total in real-time.
- **Rider Capacity Enforcement:** AJAX-based check ensuring no rider exceeds **20 bottles** (Gallon products).
- **AJAX Processing:** Smooth transaction handling with professional error modals for capacity violations.

#### 7. Bottle Ledger (`Bottle/Ledger.cshtml`) ✅ [COMPLETED]
- **Customer Search:** Global search bar.
- **History Table:** Row-by-row transaction log with "Running Balance" calculation logic in the Razor loop.

#### 8. Assigned Deliveries (`Delivery/Index.cshtml`) ✅ [COMPLETED]
- **Tabbed Interface:** Unified view for "Active Deliveries" and "Delivery History" (including Cancelled orders).
- **Inventory Restoration:** Automated logic to restore product `stock` when a delivery is cancelled.
- **Role-Protected Actions:** 
    - **Admin/Staff:** "Print Receipt" icon, "Complete Order", and "Cancel Order" functionality.
    - **Rider:** Restricted to "Mark Delivered" status updates.
- **Visual Feedback:** Color-coded status badges and detailed order summaries on each delivery card.

---

---

## 🛠️ UI Technical Standards
1. **Responsiveness:** Maintain Stitch grid behavior across devices.
2. **Interactivity:** Use vanilla JS and Fetch API for POS sidebar calculations and server-side validation.
3. **Icons & Branding:** Strictly extract and use the SVG icons and logo exactly as they appear in the `ADMIN/bottle_ledger` template for the global sidebar layout across all roles.
4. **Error Handling:** Premium message modals for business logic violations (e.g., Rider Capacity).

### Phase 4 — Employee Management & Polish 🔧 [IN-PROGRESS]
1.  **Employee Management Fix**: 
    *   Fix the delete functionality in `UsersController.cs`.
    *   Ensure proper cascading or status-based "Soft Delete" to prevent database errors if the user has existing sales/deliveries.
2.  **Validation & Security:** Implement role-based `[Authorize]` logic across all controllers.
3.  **Verification:** Complete end-to-end walkthrough for all Admin and Staff flows.

---

### 🚀 Next immediate steps for the new session:
- [ ] Open `Controllers/UsersController.cs` and check the `Delete` action logic.
- [ ] Verify the `Views/Users/Index.cshtml` delete button routing and confirmation modal.
- [ ] Test deleting an employee with and without active delivery assignments.
