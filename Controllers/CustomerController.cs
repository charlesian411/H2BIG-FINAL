using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;

namespace H2BIG.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DatabaseHelper _db;

        public CustomerController(DatabaseHelper db)
        {
            _db = db;
        }

        public IActionResult Index(string search)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToAction("Login", "Auth");

            string query = "SELECT * FROM customers";
            MySqlParameter[]? parameters = null;

            if (!string.IsNullOrEmpty(search))
            {
                query += " WHERE name LIKE @search";
                parameters = new MySqlParameter[] { new MySqlParameter("@search", $"%{search}%") };
            }

            var dt = _db.ExecuteQuery(query, parameters);
            var customers = new List<Customer>();

            foreach (DataRow row in dt.Rows)
            {
                customers.Add(new Customer
                {
                    Id = (int)row["id"],
                    Name = row["name"].ToString()!,
                    Contact = row["contact"]?.ToString(),
                    Address = row["address"]?.ToString(),
                    BottleDebt = (int)row["bottle_debt"],
                    CreatedAt = (DateTime)row["created_at"]
                });
            }

            // Metrics for view
            ViewBag.TotalCustomers = _db.ExecuteScalar("SELECT COUNT(*) FROM customers") ?? 0;
            ViewBag.TotalDebt = _db.ExecuteScalar("SELECT COALESCE(SUM(bottle_debt), 0) FROM customers") ?? 0;
            ViewBag.NewCustomers = _db.ExecuteScalar("SELECT COUNT(*) FROM customers WHERE MONTH(created_at) = MONTH(CURRENT_DATE()) AND YEAR(created_at) = YEAR(CURRENT_DATE())") ?? 0;

            return View(customers);
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            string query = "INSERT INTO customers (name, contact, address, bottle_debt) VALUES (@name, @contact, @address, 0)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@name", customer.Name),
                new MySqlParameter("@contact", customer.Contact),
                new MySqlParameter("@address", customer.Address)
            };

            _db.ExecuteNonQuery(query, parameters);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            string query = "UPDATE customers SET name = @name, contact = @contact, address = @address WHERE id = @id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@id", customer.Id),
                new MySqlParameter("@name", customer.Name),
                new MySqlParameter("@contact", customer.Contact),
                new MySqlParameter("@address", customer.Address)
            };

            _db.ExecuteNonQuery(query, parameters);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _db.ExecuteNonQuery("DELETE FROM customers WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", id) });
            return RedirectToAction("Index");
        }
    }
}
