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
            ViewBag.ActivePage = "Dashboard";

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
            ViewBag.ActivePage = "Dashboard";

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
            ViewBag.ActivePage = "Dashboard";

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

        public IActionResult Remittance(int deliveryId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            if (role != "Rider") return RedirectToAction("Login", "Auth");

            // Calculate Expected Cash: total_amount for this specific delivery
            var expectedCash = _db.ExecuteScalar(@"
                SELECT COALESCE(s.total_amount, 0) 
                FROM deliveries d
                JOIN sales s ON d.sale_id = s.id
                WHERE d.id = @did AND d.rider_id = @rid", 
                new MySqlConnector.MySqlParameter[] { 
                    new MySqlConnector.MySqlParameter("@did", deliveryId),
                    new MySqlConnector.MySqlParameter("@rid", userId) 
                }) ?? 0;

            // Calculate Expected Bottles Delivered: Sum of quantity in sale_items for this specific delivery
            var expectedBottles = _db.ExecuteScalar(@"
                SELECT COALESCE(SUM(si.quantity), 0)
                FROM deliveries d
                JOIN sales s ON d.sale_id = s.id
                JOIN sale_items si ON s.id = si.sale_id
                JOIN products p ON si.product_id = p.id
                WHERE d.id = @did AND d.rider_id = @rid AND p.name LIKE '%Gallon%'",
                new MySqlConnector.MySqlParameter[] { 
                    new MySqlConnector.MySqlParameter("@did", deliveryId),
                    new MySqlConnector.MySqlParameter("@rid", userId) 
                }) ?? 0;

            var expectedEmpties = expectedBottles;

            ViewBag.ExpectedCash = expectedCash;
            ViewBag.ExpectedBottles = expectedBottles;
            ViewBag.ExpectedEmpties = expectedEmpties;
            ViewBag.DeliveryId = deliveryId;
            ViewBag.RiderName = HttpContext.Session.GetString("UserName"); // Fixed: matches AuthController session key
            ViewBag.Date = DateTime.Now.ToString("MMMM dd, yyyy");
            ViewBag.ActivePage = "Remittance";

            return View();
        }

        [HttpPost]
        public IActionResult SubmitRemittance(int deliveryId, int expectedBottles, int expectedEmpties, decimal expectedCash, int declaredBottles, int declaredEmpties, decimal declaredCash)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            if (role != "Rider") return RedirectToAction("Login", "Auth");

            _db.ExecuteNonQuery(@"
                INSERT INTO remittances (rider_id, delivery_id, date, expected_bottles, expected_empties, expected_cash, declared_bottles, declared_empties, declared_cash, status)
                VALUES (@rid, @did, CURRENT_DATE(), @eb, @ee, @ec, @db, @de, @dc, 'Pending Check-In')",
                new MySqlConnector.MySqlParameter[] {
                    new MySqlConnector.MySqlParameter("@rid", userId),
                    new MySqlConnector.MySqlParameter("@did", deliveryId),
                    new MySqlConnector.MySqlParameter("@eb", expectedBottles),
                    new MySqlConnector.MySqlParameter("@ee", expectedEmpties),
                    new MySqlConnector.MySqlParameter("@ec", expectedCash),
                    new MySqlConnector.MySqlParameter("@db", declaredBottles),
                    new MySqlConnector.MySqlParameter("@de", declaredEmpties),
                    new MySqlConnector.MySqlParameter("@dc", declaredCash)
                });

            TempData["Success"] = "Remittance submitted successfully! You can now mark the delivery as delivered.";
            return RedirectToAction("Index", "Delivery");
        }
    }
}
