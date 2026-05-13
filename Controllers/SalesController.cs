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

            // Fetch customers
            var dtCustomers = _db.ExecuteQuery("SELECT id, name FROM customers");
            ViewBag.Customers = dtCustomers;

            return View();
        }

        [HttpPost]
        public IActionResult Process(string type, int? customerId, decimal totalAmount, string cartJson)
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
                saleCmd.Parameters.AddWithValue("@cid", (customerId == 0 ? DBNull.Value : (object?)customerId));
                saleCmd.Parameters.AddWithValue("@total", totalAmount);
                saleCmd.Parameters.AddWithValue("@type", type);
                saleCmd.Parameters.AddWithValue("@sid", staffId);
                int saleId = Convert.ToInt32(saleCmd.ExecuteScalar());

                // 2. Insert items and update stock
                foreach (var item in cartItems)
                {
                    // Fetch current price
                    var productDt = _db.ExecuteQuery("SELECT price FROM products WHERE id = @pid", new MySqlParameter[] { new MySqlParameter("@pid", item.ProductId) });
                    decimal price = Convert.ToDecimal(productDt.Rows[0]["price"]);
                    decimal subtotal = price * item.Quantity;

                    string itemQuery = "INSERT INTO sale_items (sale_id, product_id, quantity, subtotal) VALUES (@sid, @pid, @qty, @sub)";
                    var itemCmd = new MySqlCommand(itemQuery, connection, transaction);
                    itemCmd.Parameters.AddWithValue("@sid", saleId);
                    itemCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@sub", subtotal);
                    itemCmd.ExecuteNonQuery();

                    string stockQuery = "UPDATE products SET stock = stock - @qty WHERE id = @pid";
                    var stockCmd = new MySqlCommand(stockQuery, connection, transaction);
                    stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    stockCmd.ExecuteNonQuery();
                }

                // 3. If Delivery, create delivery entry (Assign to a random active rider or first available)
                if (type == "Delivery")
                {
                    var riderDt = _db.ExecuteQuery("SELECT id FROM users WHERE role = 'Rider' AND status = 'Active' LIMIT 1");
                    if (riderDt.Rows.Count > 0)
                    {
                        int riderId = Convert.ToInt32(riderDt.Rows[0]["id"]);
                        string delQuery = "INSERT INTO deliveries (sale_id, rider_id, status) VALUES (@sid, @rid, 'Pending')";
                        var delCmd = new MySqlCommand(delQuery, connection, transaction);
                        delCmd.Parameters.AddWithValue("@sid", saleId);
                        delCmd.Parameters.AddWithValue("@rid", riderId);
                        delCmd.ExecuteNonQuery();
                    }
                }

                // 4. Update Bottle Ledger if customer exists
                if (customerId > 0)
                {
                    int totalQty = cartItems.Sum(i => i.Quantity);
                    string ledgerQuery = "INSERT INTO bottle_ledger (customer_id, bottles_out, bottles_in, transaction_id) VALUES (@cid, @qty, 0, @sid)";
                    var ledgerCmd = new MySqlCommand(ledgerQuery, connection, transaction);
                    ledgerCmd.Parameters.AddWithValue("@cid", customerId);
                    ledgerCmd.Parameters.AddWithValue("@qty", totalQty);
                    ledgerCmd.Parameters.AddWithValue("@sid", saleId);
                    ledgerCmd.ExecuteNonQuery();

                    // Update customer debt total
                    string debtQuery = "UPDATE customers SET bottle_debt = bottle_debt + @qty WHERE id = @cid";
                    var debtCmd = new MySqlCommand(debtQuery, connection, transaction);
                    debtCmd.Parameters.AddWithValue("@qty", totalQty);
                    debtCmd.Parameters.AddWithValue("@cid", customerId);
                    debtCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return RedirectToAction("POS", new { success = true });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ViewBag.Error = "Transaction failed: " + ex.Message;
                return RedirectToAction("POS", new { error = ex.Message });
            }
        }

        private class CartItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
