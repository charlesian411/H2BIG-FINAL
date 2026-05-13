# Login Page Flow

On the login page, the Admin, Staff, and Rider each have their own dashboard. When the Admin logs in using their account, they will be directed to the Admin Dashboard. The same applies to the Staff and Rider — upon logging in, they will each be directed to their respective dashboards.

---

# Admin Dashboard

In the Admin Dashboard, the admin can view the total sales, active orders, outstanding bottle debt, and total revenue of their system/business. They can also see recent activity or transactions from the POS/Sales. At the bottom of the page, there is a table displaying Recent Sales Transactions.

---

# Admin Customer Management

In the Customer Management section, the admin can see the total active customers, the total bottle debt of each customer, and the number of new customers for the current month.

The search bar allows the admin to search for customers by name, and the Add New Customer button allows them to add a new customer by entering their full name, contact number, and address.

After adding a customer's details, their information will be displayed in the table. If the customer is new, their bottle debt will automatically be set to 0, since they have not yet placed any orders. Each row in the table also includes Edit and Delete icons under the Action column.

---

# Admin Product Inventory

In the Product Inventory section, the admin can manage all products. Using the search bar, the admin can search for a specific product by name. New products can be added by clicking the Add New Product button, and once added, they will appear in the table. Each product entry also includes Edit and Delete icons under the Action column.

---

# Admin User & Employee Management

In the Admin User Management section, there should be a table containing information about all users, including Admins, Staff, and Riders. The table should include the following columns: User ID, Full Name, Username, Role, Status (Active or Inactive), Time In, Time Out (displaying the time the user clocked in and out), and Actions (which includes Edit and Delete icons). There should also be an Add New User button to add new users, whether Staff or Riders.

---

# Staff Dashboard

In the Staff Dashboard, the staff can view Today's Sales Count, Pending Deliveries, and Bottles In/Out (Returned Bottles as "In" and Dispatched Bottles as "Out"). The table displays Upcoming Deliveries sourced from the POS/Sales.

---

# Staff Customer Management *(Same as Admin Customer Management)*

In the Customer Management section, the staff can see the total active customers, the total bottle debt of each customer, and the number of new customers for the current month.

The search bar allows the staff to search for customers by name, and the Add New Customer button allows them to add a new customer by entering their full name, contact number, and address.

After adding a customer's details, their information will be displayed in the table. If the customer is new, their bottle debt will automatically be set to 0, since they have not yet placed any orders. Each row in the table also includes Edit and Delete icons under the Action column.

---

# POS/Sales and Staff Assigned Deliveries Flow

Inside POS/Sales, Walk-In and Delivery transactions are handled differently.

For Walk-In transactions, there is no search bar. The default customer name shown in the Order Summary is "Walk-In Customer." After processing the sale, a large receipt will be printed with the name "Walk-In Customer." Walk-in orders do not go through the Assigned Deliveries flow — the buyer transacts directly.

For Delivery transactions, a search bar is available. When the staff searches for a customer's name, their details will appear on the right side inside the Order Summary, even before any products have been added.

When processing a delivery sale, a modal should appear allowing the staff to select which rider to assign for the delivery. The modal should also indicate whether the rider is available or currently has an ongoing delivery.

After processing the sale, the assigned rider's name should be displayed on the Deliveries/Assigned Deliveries page. Each delivery entry should include an icon button for printing the receipt, as well as buttons for Details, Mark as Delivered, and Cancel (to cancel the order if needed).

---

# Admin Bottle Ledger (formerly Bottle Tracking)

In the Bottle Ledger section, there is a search bar for searching a customer by name or ID. After searching, the customer's Transaction History table is displayed, showing the Date & Time of each order, the Transaction ID, Bottles Out, Bottles In, Running Balance (the number of bottles still not returned), and the names of the Staff and Rider who processed the transaction.

Above the table, the admin can also view the Total Bottles Out, Total Bottles Returned, and Current Bottle Debt.

---

# Admin Reports

The Admin Reports page provides a system overview and executive metrics.

**Summary Metrics:**
- **Total Sales (Today):** Displays the total revenue generated for the current day, including a percentage comparison (e.g., +8.2%).
- **Pending Deliveries:** Shows the number of active routes or deliveries currently in progress.
- **Total Bottle Debt:** Displays the total number of units outstanding across all customers.

**Visualizations & Alerts:**
- **Profit & Loss (YTD) Chart:** A visual representation of Revenue versus Operating Expenses throughout the year.
- **Debt Alerts Table:** A high-priority table highlighting customers with significant bottle debt. Columns include Customer ID, Units, and Status (Critical, Warning, or Watch).
- **View Full Ledger Link:** A quick navigation link at the bottom of the Debt Alerts section that takes the admin directly to the Bottle Ledger page.

**Tools:**
- **Export PDF Button:** Allows the admin to generate a PDF report of the current view.
- **Date Range Picker:** Allows the admin to filter the metrics and charts based on a specific time period.

----

**DASHBOARD SIDEBAR**

ADMIN SIDEBAR DASHBOARD FROM TOP TO BOTTOM:

Admin DASHBOARD

CUSTOMERS

POS/SALES

BOTTLE LEDGER

ASSIGNED DELIVERIES

INVENTORY (ONLY CAN ADMIN ACCESS THIS)

ADMIN REPORTS (ONLY CAN ADMIN ACCESS THIS)

STAFF SIDEBAR DASHBOARD FROM TOP TO BOTTOM:

Staff DASHBOARD

CUSTOMERS

POS/SALES

BOTTLE LEDGER

ASSIGNED DELIVERIES

---

> **Note:** If a "New Transaction" option appears in the sidebar of the Stitch UI, please remove it, as it is not needed.
