# 🤝 Project Handoff Summary: H2BIG Management System

This document provides all the necessary context for the next session on a new machine.

## 🛠️ Current Status
The **Delivery & Operations** workflow is now 100% stable and feature-complete. We have established a high-standard UI/UX pattern using **Stitch-inspired** aesthetics and **AJAX-based** server interactions.

## 🌟 Key Accomplishments
1.  **POS / Sales**:
    *   Strict **20-bottle capacity enforcement** per rider.
    *   AJAX submission logic using `Fetch API` for smooth, no-reload sales.
    *   **Premium Error Modals**: Custom UI for capacity violations instead of raw JSON.
2.  **Deliveries**:
    *   **Tabbed Interface**: Unified "Active Deliveries" and "Delivery History".
    *   **Inventory Restoration**: If an order is **Cancelled**, stock is automatically returned to the products table.
    *   **Staff Printing**: Direct "Print Receipt" icon on delivery cards for Admin/Staff roles.
3.  **Database Patterns**:
    *   Using `MySqlConnector` with `DatabaseHelper.cs`.
    *   Inventory column is named `stock` (NOT `current_stock`).
    *   "Gallon" product detection is based on string matching (`LIKE '%Gallon%'`).

## 🛑 Current Blockers / Next Tasks
1.  **Employee Management (Admin)**: 
    *   **ISSUE**: Users cannot currently be deleted from the Admin/Users index.
    *   **FIX NEEDED**: Check `UsersController.cs` for the Delete action. It likely needs to handle foreign key constraints (e.g., if a rider has assigned sales, we should probably change their status to 'Inactive' instead of deleting them).
2.  **Security**: 
    *   Controllers need `[Authorize]` attributes to ensure only logged-in users with the right roles can access specific pages.

## 💡 Technical Patterns for Next Pass
- **AJAX Pattern**: Use the `Fetch API` pattern found in `POS.cshtml` for future forms to maintain the premium "single-page" feel.
- **Error Handling**: Use the `showError(msg)` modal pattern established in `POS.cshtml` for any business logic validation errors.
- **SQL Transactions**: Always use `connection.BeginTransaction()` for multi-step operations like the `ProcessSale` in `SalesController.cs`.

---
*Created on 2026-05-15 to ensure a seamless transition.*
