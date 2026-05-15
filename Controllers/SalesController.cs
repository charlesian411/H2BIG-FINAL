using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;
using System.Text.Json;

namespace H2BIG.Controllers
{
    public class SalesController : Controller
    {
        private readonly DatabaseHelper _db;
        public SalesController(DatabaseHelper db) => _db = db;

        public IActionResult POS()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToAction("Login", "Auth");

            // Fetch products
            var dtProducts = _db.ExecuteQuery("SELECT * FROM products WHERE stock > 0");
            ViewBag.Products = dtProducts;

            // Fetch customers with address
            var dtCustomers = _db.ExecuteQuery("SELECT id, name, address FROM customers");
            ViewBag.Customers = dtCustomers;

            // Fetch active riders with their current bottle load
            var dtRiders = _db.ExecuteQuery(@"
                SELECT u.id, u.fullname, 
                       COALESCE((
                           SELECT SUM(si.quantity)
                           FROM deliveries d
                           JOIN sale_items si ON d.sale_id = si.sale_id
                           JOIN products p ON si.product_id = p.id
                           WHERE d.rider_id = u.id 
                           AND d.status IN ('Pending', 'Delivered')
                           AND p.name LIKE '%Gallon%'
                       ), 0) as current_load
                FROM users u 
                WHERE u.role = 'Rider' AND u.status = 'Active'");
            ViewBag.Riders = dtRiders;

            return View();
        }

        [HttpPost]
        public IActionResult Process(string type, int? customerId, int? riderId, decimal totalAmount, string cartJson)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Session expired" });
            int staffId = int.Parse(userIdStr);

            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            if (cartItems == null || cartItems.Count == 0) return Json(new { success = false, message = "Cart is empty" });

            using var connection = _db.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert into sales
                string saleQuery = "INSERT INTO sales (customer_id, total_amount, type, staff_id) VALUES (@cid, @total, @type, @sid); SELECT LAST_INSERT_ID();";
                var saleCmd = new MySqlCommand(saleQuery, connection, transaction);
                saleCmd.Parameters.AddWithValue("@cid", (customerId == 0 || customerId == null ? DBNull.Value : (object)customerId));
                saleCmd.Parameters.AddWithValue("@total", totalAmount);
                saleCmd.Parameters.AddWithValue("@type", type);
                saleCmd.Parameters.AddWithValue("@sid", staffId);
                int saleId = Convert.ToInt32(saleCmd.ExecuteScalar());

                // 2. Insert items and update stock
                foreach (var item in cartItems)
                {
                    // Fetch price using same connection/transaction
                    var priceCmd = new MySqlCommand("SELECT price FROM products WHERE id = @pid", connection, transaction);
                    priceCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    decimal price = Convert.ToDecimal(priceCmd.ExecuteScalar());
                    decimal subtotal = price * item.Quantity;

                    var itemCmd = new MySqlCommand("INSERT INTO sale_items (sale_id, product_id, quantity, subtotal) VALUES (@sid, @pid, @qty, @sub)", connection, transaction);
                    itemCmd.Parameters.AddWithValue("@sid", saleId);
                    itemCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@sub", subtotal);
                    itemCmd.ExecuteNonQuery();

                    // Only decrease stock for Deliveries (Walk-ins bring their own bottles)
                    if (type != "Walk-In")
                    {
                        var stockCmd = new MySqlCommand("UPDATE products SET stock = stock - @qty WHERE id = @pid", connection, transaction);
                        stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        stockCmd.Parameters.AddWithValue("@pid", item.ProductId);
                        stockCmd.ExecuteNonQuery();
                    }
                }

                // 3. If Delivery, create delivery entry
                if (type == "Delivery")
                {
                    var delCmd = new MySqlCommand("INSERT INTO deliveries (sale_id, rider_id, status) VALUES (@sid, @rid, 'Pending')", connection, transaction);
                    delCmd.Parameters.AddWithValue("@sid", saleId);
                    delCmd.Parameters.AddWithValue("@rid", riderId ?? 0);
                    delCmd.ExecuteNonQuery();
                }

                // 4. Update Bottle Ledger if customer exists
                if (customerId > 0)
                {
                    int totalQty = cartItems.Sum(i => i.Quantity);
                    
                    // Update customer debt first
                    var debtCmd = new MySqlCommand("UPDATE customers SET bottle_debt = bottle_debt + @qty WHERE id = @cid", connection, transaction);
                    debtCmd.Parameters.AddWithValue("@qty", totalQty);
                    debtCmd.Parameters.AddWithValue("@cid", customerId);
                    debtCmd.ExecuteNonQuery();

                    // Now insert into ledger with the updated balance
                    var ledgerCmd = new MySqlCommand(@"
                        INSERT INTO bottle_ledger (customer_id, bottles_out, bottles_in, balance, transaction_id) 
                        VALUES (@cid, @qty, 0, (SELECT bottle_debt FROM customers WHERE id = @cid), @sid)", connection, transaction);
                    ledgerCmd.Parameters.AddWithValue("@cid", customerId);
                    ledgerCmd.Parameters.AddWithValue("@qty", totalQty);
                    ledgerCmd.Parameters.AddWithValue("@sid", saleId);
                    ledgerCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                
                if (type == "Walk-In")
                {
                    return RedirectToAction("Receipt", new { id = saleId });
                }
                
                TempData["Success"] = "Sale processed successfully!";
                return RedirectToAction("Index", "Delivery");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return RedirectToAction("POS", new { error = "Transaction failed: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Receipt(int id)
        {
            var saleDt = _db.ExecuteQuery($@"
                SELECT s.*, u.fullname as cashier_name, c.name as customer_name, c.id as customer_id_label, c.bottle_debt 
                FROM sales s 
                LEFT JOIN users u ON s.staff_id = u.id 
                LEFT JOIN customers c ON s.customer_id = c.id 
                WHERE s.id = {id}");
            
            if (saleDt.Rows.Count == 0) return NotFound();
            
            var itemsDt = _db.ExecuteQuery($@"
                SELECT si.*, p.name as product_name, p.price as current_price
                FROM sale_items si 
                JOIN products p ON si.product_id = p.id 
                WHERE si.sale_id = {id}");

            // Fetch bottle movement for this transaction
            var ledgerDt = _db.ExecuteQuery($"SELECT bottles_out, bottles_in FROM bottle_ledger WHERE transaction_id = {id}");
            
            ViewBag.Sale = saleDt.Rows[0];
            ViewBag.Items = itemsDt;
            ViewBag.Ledger = ledgerDt.Rows.Count > 0 ? ledgerDt.Rows[0] : null;
            
            return View();
        }

        [HttpGet]
        public IActionResult DeliveryHistory()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToAction("Login", "Auth");
            var dt = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Delivery'
                ORDER BY s.date_time DESC");
            ViewData["Title"] = "Delivery Transaction History";
            ViewBag.ActivePage = "Dashboard";
            return View("History", dt);
        }

        [HttpGet]
        public IActionResult WalkInHistory()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToAction("Login", "Auth");
            var dt = _db.ExecuteQuery(@"
                SELECT s.id, s.date_time, COALESCE(c.name, 'Walk-In Customer') as CustomerName, s.total_amount, s.type
                FROM sales s
                LEFT JOIN customers c ON s.customer_id = c.id
                WHERE s.type = 'Walk-In'
                ORDER BY s.date_time DESC");
            ViewData["Title"] = "Walk-In Transaction History";
            ViewBag.ActivePage = "Dashboard";
            return View("History", dt);
        }

        private class CartItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
