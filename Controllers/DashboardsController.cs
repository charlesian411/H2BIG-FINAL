using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;

namespace H2BIG.Controllers
{
    public class AdminController : Controller
    {
        private readonly DatabaseHelper _db;
        public AdminController(DatabaseHelper db) => _db = db;

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

            ViewBag.SalesToday = _db.ExecuteScalar("SELECT COALESCE(SUM(total_amount), 0) FROM sales WHERE DATE(date_time) = CURRENT_DATE()") ?? 0;
            ViewBag.PendingDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Pending'") ?? 0;
            ViewBag.TotalDebt = _db.ExecuteScalar("SELECT COALESCE(SUM(bottle_debt), 0) FROM customers") ?? 0;
            ViewBag.TopDebtors = _db.ExecuteQuery("SELECT id, name, contact, bottle_debt FROM customers WHERE bottle_debt > 0 ORDER BY bottle_debt DESC LIMIT 5");

            // Fetch recent Delivery transactions
            ViewBag.RecentDeliveries = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Delivery'
                ORDER BY s.date_time DESC
                LIMIT 5");

            // Fetch recent Walk-In transactions
            ViewBag.RecentWalkIns = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Walk-In'
                ORDER BY s.date_time DESC
                LIMIT 5");

            return View();
        }
    }

    public class StaffController : Controller
    {
        private readonly DatabaseHelper _db;
        public StaffController(DatabaseHelper db) => _db = db;

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Staff") return RedirectToAction("Login", "Auth");

            ViewBag.SalesToday = _db.ExecuteScalar("SELECT COALESCE(SUM(total_amount), 0) FROM sales WHERE DATE(date_time) = CURRENT_DATE()") ?? 0;
            ViewBag.PendingDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Pending'") ?? 0;

            // Fetch recent Delivery transactions
            ViewBag.RecentDeliveries = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Delivery'
                ORDER BY s.date_time DESC
                LIMIT 5");

            // Fetch recent Walk-In transactions
            ViewBag.RecentWalkIns = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Walk-In'
                ORDER BY s.date_time DESC
                LIMIT 5");

            return View();
        }
    }

    public class RiderController : Controller
    {
        private readonly DatabaseHelper _db;
        public RiderController(DatabaseHelper db) => _db = db;

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            if (role != "Rider") return RedirectToAction("Login", "Auth");

            ViewBag.DeliveriesToMake = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE rider_id = @rid AND status = 'Pending'", new MySqlConnector.MySqlParameter[] { new MySqlConnector.MySqlParameter("@rid", userId) }) ?? 0;
            
            // Join with sales to get the transaction date
            ViewBag.CompletedDeliveries = _db.ExecuteScalar(@"
                SELECT COUNT(*) 
                FROM deliveries d
                JOIN sales s ON d.sale_id = s.id
                WHERE d.rider_id = @rid AND d.status = 'Delivered' AND DATE(s.date_time) = CURRENT_DATE()", 
                new MySqlConnector.MySqlParameter[] { new MySqlConnector.MySqlParameter("@rid", userId) }) ?? 0;

            return View();
        }
    }
}
