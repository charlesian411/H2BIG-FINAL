using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;

namespace H2BIG.Controllers
{
    public class InventoryController : Controller
    {
        private readonly DatabaseHelper _db;
        public InventoryController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

            var dt = _db.ExecuteQuery("SELECT * FROM products");
            return View(dt);
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            _db.ExecuteNonQuery("INSERT INTO products (name, price, stock, default_qty) VALUES (@name, @price, @stock, @dqty)",
                new MySqlParameter[] {
                    new MySqlParameter("@name", product.Name),
                    new MySqlParameter("@price", product.Price),
                    new MySqlParameter("@stock", product.Stock),
                    new MySqlParameter("@dqty", product.DefaultQty)
                });
            return RedirectToAction("Index");
        }
 
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _db.ExecuteNonQuery("UPDATE products SET name = @name, price = @price, stock = @stock, default_qty = @dqty WHERE id = @id",
                new MySqlParameter[] {
                    new MySqlParameter("@id", product.Id),
                    new MySqlParameter("@name", product.Name),
                    new MySqlParameter("@price", product.Price),
                    new MySqlParameter("@stock", product.Stock),
                    new MySqlParameter("@dqty", product.DefaultQty)
                });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // First, delete related sale items to avoid foreign key constraint errors
            _db.ExecuteNonQuery("DELETE FROM sale_items WHERE product_id = @id", new MySqlParameter[] { new MySqlParameter("@id", id) });
            
            // Then delete the product
            _db.ExecuteNonQuery("DELETE FROM products WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", id) });
            
            return RedirectToAction("Index");
        }
    }

    public class ReportController : Controller
    {
        private readonly DatabaseHelper _db;
        public ReportController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

            // Sales Velocity (Only count Walk-In or Completed Deliveries)
            string salesFilter = " JOIN sales s ON d.sale_id = s.id WHERE (s.type = 'Walk-In' OR d.status = 'Completed')";
            
            string baseSalesQuery = @"
                SELECT COALESCE(SUM(s.total_amount), 0) 
                FROM sales s
                LEFT JOIN deliveries d ON s.id = d.sale_id
                WHERE (s.type = 'Walk-In' OR d.status = 'Completed') AND ";

            ViewBag.TotalSalesToday = _db.ExecuteScalar(baseSalesQuery + "DATE(s.date_time) = CURRENT_DATE()") ?? 0;
            ViewBag.TotalSalesWeekly = _db.ExecuteScalar(baseSalesQuery + "s.date_time >= DATE_SUB(NOW(), INTERVAL 7 DAY)") ?? 0;
            ViewBag.TotalSalesMonthly = _db.ExecuteScalar(baseSalesQuery + "s.date_time >= DATE_SUB(NOW(), INTERVAL 30 DAY)") ?? 0;

            // Logistics
            ViewBag.PendingDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Pending'") ?? 0;
            ViewBag.CompletedDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Delivered'") ?? 0;
            
            // Inventory Performance
            var mostSoldDt = _db.ExecuteQuery(@"
                SELECT p.name, SUM(si.quantity) as total_qty 
                FROM sale_items si 
                JOIN products p ON si.product_id = p.id 
                GROUP BY si.product_id 
                ORDER BY total_qty DESC 
                LIMIT 1");
            ViewBag.MostSoldProduct = mostSoldDt.Rows.Count > 0 ? mostSoldDt.Rows[0]["name"] : "N/A";

            // Financial health
            ViewBag.TotalBottleDebt = _db.ExecuteScalar("SELECT COALESCE(SUM(bottle_debt), 0) FROM customers") ?? 0;

            // Debt Alerts
            var debtAlerts = _db.ExecuteQuery(@"
                SELECT id, name, bottle_debt, 
                CASE 
                    WHEN bottle_debt > 10 THEN 'Critical'
                    WHEN bottle_debt > 5 THEN 'Warning'
                    ELSE 'Watch'
                END as Status
                FROM customers 
                WHERE bottle_debt > 0 
                ORDER BY bottle_debt DESC 
                LIMIT 10");
            
            ViewBag.DebtAlerts = debtAlerts;

            // Chart Data: YTD Profit & Loss
            var chartDataDt = _db.ExecuteQuery(@"
                SELECT 
                    m.month,
                    (SELECT COALESCE(SUM(s.total_amount), 0) 
                     FROM sales s 
                     LEFT JOIN deliveries d ON s.id = d.sale_id
                     WHERE MONTH(s.date_time) = m.month AND YEAR(s.date_time) = YEAR(CURRENT_DATE())
                     AND (s.type = 'Walk-In' OR d.status = 'Completed')) as Revenue,
                    (SELECT COALESCE(SUM(amount), 0) FROM expenses WHERE MONTH(date) = m.month AND YEAR(date) = YEAR(CURRENT_DATE())) as Expenses
                FROM (
                    SELECT 1 as month UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 
                    UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 
                    UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12
                ) m
                ORDER BY m.month;
            ");

            var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var revenues = new List<decimal>();
            var expenses = new List<decimal>();

            foreach (DataRow row in chartDataDt.Rows)
            {
                revenues.Add(Convert.ToDecimal(row["Revenue"]));
                expenses.Add(Convert.ToDecimal(row["Expenses"]));
            }

            ViewBag.ChartMonths = System.Text.Json.JsonSerializer.Serialize(months);
            ViewBag.ChartRevenues = System.Text.Json.JsonSerializer.Serialize(revenues);
            ViewBag.ChartExpenses = System.Text.Json.JsonSerializer.Serialize(expenses);

            return View();
        }
        public IActionResult ExportDailySales()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

            var dt = _db.ExecuteQuery(@"
                SELECT 
                    s.id as SaleId, 
                    s.date_time, 
                    COALESCE(c.name, 'Walk-In') as CustomerName, 
                    p.name as ProductName, 
                    si.quantity, 
                    p.price, 
                    si.subtotal,
                    s.type
                FROM sales s
                JOIN sale_items si ON s.id = si.sale_id
                JOIN products p ON si.product_id = p.id
                LEFT JOIN customers c ON s.customer_id = c.id
                LEFT JOIN deliveries d ON s.id = d.sale_id
                WHERE DATE(s.date_time) = CURRENT_DATE()
                AND (s.type = 'Walk-In' OR d.status = 'Completed')
                ORDER BY s.date_time ASC");

            ViewBag.ReportDate = DateTime.Now.ToString("MMMM dd, yyyy");
            return View("DailySalesReport", dt);
        }
    }
}
