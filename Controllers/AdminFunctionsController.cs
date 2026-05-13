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
            _db.ExecuteNonQuery("INSERT INTO products (name, price, stock) VALUES (@name, @price, @stock)",
                new MySqlParameter[] {
                    new MySqlParameter("@name", product.Name),
                    new MySqlParameter("@price", product.Price),
                    new MySqlParameter("@stock", product.Stock)
                });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _db.ExecuteNonQuery("UPDATE products SET name = @name, price = @price, stock = @stock WHERE id = @id",
                new MySqlParameter[] {
                    new MySqlParameter("@id", product.Id),
                    new MySqlParameter("@name", product.Name),
                    new MySqlParameter("@price", product.Price),
                    new MySqlParameter("@stock", product.Stock)
                });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
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

            // Sales Velocity
            ViewBag.TotalSalesToday = _db.ExecuteScalar("SELECT COALESCE(SUM(total_amount), 0) FROM sales WHERE DATE(date_time) = CURRENT_DATE()") ?? 0;
            ViewBag.TotalSalesWeekly = _db.ExecuteScalar("SELECT COALESCE(SUM(total_amount), 0) FROM sales WHERE date_time >= DATE_SUB(NOW(), INTERVAL 7 DAY)") ?? 0;
            ViewBag.TotalSalesMonthly = _db.ExecuteScalar("SELECT COALESCE(SUM(total_amount), 0) FROM sales WHERE date_time >= DATE_SUB(NOW(), INTERVAL 30 DAY)") ?? 0;

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

            return View();
        }
    }
}
