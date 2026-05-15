using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using System.Data;

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

            ViewBag.SalesToday = _db.ExecuteScalar(@"
                SELECT COALESCE(SUM(s.total_amount), 0) 
                FROM sales s
                LEFT JOIN deliveries d ON s.id = d.sale_id
                WHERE DATE(s.date_time) = CURRENT_DATE()
                AND (s.type = 'Walk-In' OR d.status = 'Completed')") ?? 0;
            ViewBag.PendingDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Pending'") ?? 0;
            ViewBag.TotalDebt = _db.ExecuteScalar("SELECT COALESCE(SUM(bottle_debt), 0) FROM customers") ?? 0;
            ViewBag.TopDebtors = _db.ExecuteQuery("SELECT id, name, contact, bottle_debt FROM customers WHERE bottle_debt > 0 ORDER BY bottle_debt DESC LIMIT 5");

            // Fetch Last 7 Days Sales for Chart
            var chartDt = _db.ExecuteQuery(@"
                SELECT 
                    DATE_FORMAT(date_list.date, '%a') as DayName,
                    COALESCE(SUM(s.total_amount), 0) as Revenue
                FROM (
                    SELECT CURRENT_DATE() as date
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 1 DAY)
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 2 DAY)
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 3 DAY)
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 4 DAY)
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 5 DAY)
                    UNION SELECT DATE_SUB(CURRENT_DATE(), INTERVAL 6 DAY)
                ) as date_list
                LEFT JOIN sales s ON DATE(s.date_time) = date_list.date
                LEFT JOIN deliveries d ON s.id = d.sale_id
                AND (s.type = 'Walk-In' OR d.status = 'Completed')
                GROUP BY date_list.date
                ORDER BY date_list.date ASC");

            var dayNames = new List<string>();
            var revenues = new List<decimal>();
            foreach (System.Data.DataRow row in chartDt.Rows)
            {
                dayNames.Add(row["DayName"].ToString());
                revenues.Add(Convert.ToDecimal(row["Revenue"]));
            }
            ViewBag.ChartDays = System.Text.Json.JsonSerializer.Serialize(dayNames);
            ViewBag.ChartRevenues = System.Text.Json.JsonSerializer.Serialize(revenues);

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

            ViewBag.SalesToday = _db.ExecuteScalar(@"
                SELECT COALESCE(SUM(s.total_amount), 0) 
                FROM sales s
                LEFT JOIN deliveries d ON s.id = d.sale_id
                WHERE DATE(s.date_time) = CURRENT_DATE()
                AND (s.type = 'Walk-In' OR d.status = 'Completed')") ?? 0;
            
            ViewBag.SalesCount = _db.ExecuteScalar(@"
                SELECT COUNT(*) 
                FROM sales s
                LEFT JOIN deliveries d ON s.id = d.sale_id
                WHERE DATE(s.date_time) = CURRENT_DATE()
                AND (s.type = 'Walk-In' OR d.status = 'Completed')") ?? 0;
            ViewBag.PendingDeliveries = _db.ExecuteScalar("SELECT COUNT(*) FROM deliveries WHERE status = 'Pending'") ?? 0;

            ViewBag.BottlesOut = _db.ExecuteScalar(@"
                SELECT COALESCE(SUM(si.quantity), 0) 
                FROM sale_items si 
                JOIN sales s ON si.sale_id = s.id 
                WHERE DATE(s.date_time) = CURRENT_DATE()
                AND s.type = 'Delivery'") ?? 0;

            // Bottles In (Returned Today)
            ViewBag.BottlesIn = _db.ExecuteScalar(@"
                SELECT COALESCE(SUM(bottles_in), 0) 
                FROM bottle_ledger 
                WHERE DATE(date) = CURRENT_DATE()") ?? 0;

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

            // 1. Insert Remittance Record
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

            // 2. Calculate Bottle Debt Change
            // New Debt = Current Debt + Delivered - Returned
            var dtDelivery = _db.ExecuteQuery(@"
                SELECT s.customer_id, s.id as sale_id 
                FROM deliveries d 
                JOIN sales s ON d.sale_id = s.id 
                WHERE d.id = @did", new MySqlConnector.MySqlParameter[] { new MySqlConnector.MySqlParameter("@did", deliveryId) });

            if (dtDelivery.Rows.Count > 0)
            {
                int customerId = Convert.ToInt32(dtDelivery.Rows[0]["customer_id"]);
                int saleId = Convert.ToInt32(dtDelivery.Rows[0]["sale_id"]);

                // UPDATE: Subtract only the returned empties. 
                // (Bottles delivered were already added to debt at POS)
                _db.ExecuteNonQuery("UPDATE customers SET bottle_debt = bottle_debt - @returned WHERE id = @cid",
                    new MySqlConnector.MySqlParameter[] {
                        new MySqlConnector.MySqlParameter("@returned", declaredEmpties),
                        new MySqlConnector.MySqlParameter("@cid", customerId)
                    });

                // Create Ledger Entry for the return
                var currentBalance = _db.ExecuteScalar("SELECT bottle_debt FROM customers WHERE id = @cid", 
                    new MySqlConnector.MySqlParameter[] { new MySqlConnector.MySqlParameter("@cid", customerId) });

                _db.ExecuteNonQuery(@"
                    INSERT INTO bottle_ledger (customer_id, bottles_out, bottles_in, balance, transaction_id, date)
                    VALUES (@cid, 0, @bin, @bal, @tid, NOW())",
                    new MySqlConnector.MySqlParameter[] {
                        new MySqlConnector.MySqlParameter("@cid", customerId),
                        new MySqlConnector.MySqlParameter("@bin", declaredEmpties),
                        new MySqlConnector.MySqlParameter("@bal", currentBalance),
                        new MySqlConnector.MySqlParameter("@tid", saleId)
                    });

                // 3. Inventory Restoration: Add returned empties back to stock
                if (declaredEmpties > 0)
                {
                    // Find the products in this sale that are bottles (Gallon containers)
                    var dtItems = _db.ExecuteQuery(@"
                        SELECT si.product_id, si.quantity 
                        FROM sale_items si 
                        JOIN products p ON si.product_id = p.id 
                        WHERE si.sale_id = @sid AND p.name LIKE '%Gallon%'", 
                        new MySqlConnector.MySqlParameter[] { new MySqlConnector.MySqlParameter("@sid", saleId) });

                    int remainingEmpties = declaredEmpties;
                    foreach (DataRow item in dtItems.Rows)
                    {
                        if (remainingEmpties <= 0) break;

                        int productId = Convert.ToInt32(item["product_id"]);
                        int soldQty = Convert.ToInt32(item["quantity"]);
                        
                        // We attribute empties to products in the order they appear in the sale
                        int restoreQty = Math.Min(remainingEmpties, soldQty);
                        
                        _db.ExecuteNonQuery("UPDATE products SET stock = stock + @qty WHERE id = @pid",
                            new MySqlConnector.MySqlParameter[] {
                                new MySqlConnector.MySqlParameter("@qty", restoreQty),
                                new MySqlConnector.MySqlParameter("@pid", productId)
                            });
                        
                        remainingEmpties -= restoreQty;
                    }
                }
            }

            TempData["Success"] = "Remittance submitted successfully! Bottle debt has been updated.";
            return RedirectToAction("Index", "Delivery");
        }
    }
}
