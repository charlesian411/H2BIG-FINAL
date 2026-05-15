# H2BIG Management System - Demo Flow

This document outlines the step-by-step flow for demonstrating the H2BIG system.

---

## 1. Authentication & Role-Based Access
*   **Action**: Log in as different users to show the unique dashboards.
    *   **Admin**: Total overview (Revenue, Trends, Inventory, Reports).
    *   **Staff**: Operational focus (POS, Quick metrics, Pending deliveries).
    *   **Rider**: Delivery focus (Assigned tasks, Remittances).

---

## 2. Staff Operations: POS & Sales
*   **Feature: Walk-In Refill**
    *   Go to **POS/Sales** -> Select **Walk-In**.
    *   Process a sale.
    *   **Show**: Revenue updates instantly on dashboards (since walk-ins are "completed" immediately).
    *   **Show**: Printed Receipt generation.
*   **Feature: Delivery Assignment**
    *   Select **Delivery** -> Choose a **Customer** and **Rider**.
    *   **Show: Capacity Enforcer**: Try to assign more than 20 bottles to a rider. The system will block it!
    *   Process the sale.
    *   **Show**: The customer's **Bottle Debt** increases immediately on the Dashboard.

---

## 3. Rider Operations: Fulfillment & Remittance
*   **Feature: Assigned Deliveries**
    *   Log in as **Mike Rider**.
    *   See the new delivery appearing in the list.
*   **Feature: Delivery Process**
    *   Mark the order as **"Delivered"**.
*   **Feature: Rider Remittance (The Logic Core)**
    *   Navigate to the Remittance page.
    *   Declare: 10 bottles delivered, 5 empties collected, and the Cash amount.
    *   **Submit Remittance**.
    *   **DEMO THE LOGIC**:
        *   **Debt Update**: Show that the customer's debt decreased by the 5 empties returned.
        *   **Inventory Restoration**: Show the **Admin Inventory** — the 5 empty bottles are now back in your "Current Stock"!

---

## 4. Admin Management: Oversight & Strategy
*   **Feature: Real-Time Dashboard**
    *   **Show: KPI Cards**: Total Revenue, Active Deliveries, and Outstanding Bottle Debt.
    *   **Show: Interactive Sales Trends**: Hover over the bars to see the exact price for each day. Note the Day labels (Mon, Tue, etc.) at the bottom.
*   **Feature: Inventory Control**
    *   Go to **Inventory**.
    *   See the automatic stock updates and "Low Stock" alerts.
*   **Feature: Advanced Reporting**
    *   Go to **Admin Reports**.
    *   **Show: Revenue Logic**: Point out that revenue only counts once the Rider completes the order.
    *   **Show: Debt Alerts**: Highlighting customers with critical debt (now showing **Customer Names** instead of IDs).
*   **Feature: PDF Export**
    *   Click **Export PDF**.
    *   **Show**: The professional, itemized **Daily Sales Report** with the correct Transaction Numbers (#) and Date.

---

## 5. Security & Recovery: Cancellation Flow
*   **Feature: Order Cancellation**
    *   Pick a pending delivery and click **Cancel Order**.
    *   **Show: Complete Reversal**: 
        *   Stock is returned to the shelf.
        *   Customer's bottle debt is reverted to its previous number.
        *   The transaction is marked as Cancelled in History.

---

## 6. System Integrity: Database Recovery
*   **Feature: Bulletproof Schema**
    *   Show the `h2big_db.sql` file. 
    *   Explain that it is designed to be re-imported anytime to restore a perfect, clean system with all tables (`remittances`, `ledger`, etc.) and rules intact.
