# H2BIG Technical Documentation (Teacher's Guide)

This guide explains how the core logic of the H2BIG system works. Use this to answer technical questions during your demo.

---

## 1. System Architecture
*   **Framework**: ASP.NET Core MVC (Model-View-Controller).
*   **Database**: MySQL (XAMPP).
*   **Data Access**: We use a `DatabaseHelper` class. Instead of writing long database connection code everywhere, we use `_db.ExecuteQuery` or `_db.ExecuteNonQuery` for clean, reusable code.

---

## 2. Authentication & Sessions
*   **Logic**: When a user logs in, we store their `UserId` and `UserRole` in a **Session**.
*   **Teacher's Question**: *"How do you prevent a Rider from seeing the Admin page?"*
*   **Answer**: "Every controller has a check: `if (HttpContext.Session.GetString("UserRole") != "Admin")`. If the role doesn't match, the system redirects them to the login page."

---

## 3. The POS Logic (Sales)
*   **Transaction Flow**: When a sale is processed:
    1.  A record is created in the `sales` table.
    2.  Each item is added to `sale_items`.
    3.  **Stock Update**: The system automatically subtracts the quantity from the `products` table.
    4.  **Bottle Debt**: If it's a customer (not Walk-In), the system adds the number of bottles to their `bottle_debt` in the `customers` table immediately.

---

## 4. Delivery & Cancellation Logic
*   **Status Updates**: Deliveries move through three states: `Pending` -> `Delivered` -> `Completed`.
*   **Cancellation Reversal**: If an order is cancelled:
    *   **Inventory**: We run an `UPDATE` query that adds the bottles back to the `stock` in the `products` table.
    *   **Debt**: We run an `UPDATE` query that subtracts the bottles from the `bottle_debt` of the customer.
    *   **Goal**: This ensures that a cancelled order leaves no trace on your inventory or debt.

---

## 5. Rider Remittance (The "Smart" Part)
*   **Calculation**: `New Debt = Old Debt - Empties Collected`.
*   **Inventory Restoration**: When a rider returns an empty bottle, it is "dispatched" back into the shop.
*   **Teacher's Question**: *"How do you know which product stock to increase when a rider returns a generic empty?"*
*   **Answer**: "The system looks at the original sale for that delivery. It identifies which 'Gallon' products were sold and adds the returned quantity back to those specific products."

---

## 6. Dashboard & Reporting Logic
*   **Revenue Accuracy**: In the **Admin Reports**, we only count revenue if `s.type = 'Walk-In'` OR `d.status = 'Completed'`.
*   **Why?**: We don't want to count money that hasn't been collected yet (Pending Deliveries).
*   **Interactive Chart**: We use **ApexCharts.js**. It takes the JSON data sent from the C# controller and renders it into a dynamic, hoverable bar chart.
*   **PDF Export**: This isn't a complex library. It is a **Printable View** designed with CSS. When clicked, it triggers the browser's print engine, which is the most reliable way to generate a formatted PDF.

---

## 7. Database Integrity
*   **Primary Keys & Foreign Keys**: We use `sale_id` and `customer_id` as foreign keys to link tables together. This prevents "orphan" data and ensures that if a customer is deleted, their sales records are handled correctly.
*   **ENUMs**: We use `ENUM` for statuses (like 'Pending', 'Completed'). This prevents typos in the database and ensures only valid statuses can be saved.
