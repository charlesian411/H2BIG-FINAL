using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;

namespace H2BIG.Controllers
{
    public class BottleController : Controller
    {
        private readonly DatabaseHelper _db;
        public BottleController(DatabaseHelper db) => _db = db;

        public IActionResult Ledger(int? customerId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToAction("Login", "Auth");

            if (!customerId.HasValue) return View(new List<BottleLedger>());

            string query = @"
                SELECT bl.*, u_staff.fullname as StaffName, u_rider.fullname as RiderName 
                FROM bottle_ledger bl
                JOIN sales s ON bl.transaction_id = s.id
                JOIN users u_staff ON s.staff_id = u_staff.id
                LEFT JOIN users u_rider ON s.rider_id = u_rider.id
                WHERE bl.customer_id = @cid
                ORDER BY bl.date DESC";
            
            var dt = _db.ExecuteQuery(query, new MySqlParameter[] { new MySqlParameter("@cid", customerId.Value) });
            var ledger = new List<BottleLedger>();

            foreach (DataRow row in dt.Rows)
            {
                ledger.Add(new BottleLedger
                {
                    Id = (int)row["id"],
                    CustomerId = (int)row["customer_id"],
                    BottlesOut = (int)row["bottles_out"],
                    BottlesIn = (int)row["bottles_in"],
                    Balance = (int)row["balance"],
                    TransactionId = (int)row["transaction_id"],
                    Date = (DateTime)row["date"]
                });
            }

            var customerDt = _db.ExecuteQuery("SELECT * FROM customers WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", customerId.Value) });
            if (customerDt.Rows.Count > 0)
            {
                ViewBag.Customer = customerDt.Rows[0];
            }
            else
            {
                ViewBag.Customer = null;
            }

            return View(ledger);
        }
    }

    public class DeliveryController : Controller
    {
        private readonly DatabaseHelper _db;
        public DeliveryController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login", "Auth");

            string query = @"
                SELECT d.*, s.total_amount, s.date_time, c.name as CustomerName, c.address, r.fullname as RiderName
                FROM deliveries d
                JOIN sales s ON d.sale_id = s.id
                JOIN customers c ON s.customer_id = c.id
                JOIN users r ON d.rider_id = r.id";

            if (role == "Rider")
            {
                query += " WHERE d.rider_id = @rid";
            }

            var dt = _db.ExecuteQuery(query, role == "Rider" ? new MySqlParameter[] { new MySqlParameter("@rid", userId) } : null);
            return View(dt);
        }

        [HttpPost]
        public IActionResult MarkAsDelivered(int deliveryId)
        {
            _db.ExecuteNonQuery("UPDATE deliveries SET status = 'Delivered' WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", deliveryId) });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelDelivery(int deliveryId)
        {
            _db.ExecuteNonQuery("UPDATE deliveries SET status = 'Cancelled' WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", deliveryId) });
            return RedirectToAction("Index");
        }
    }
}
