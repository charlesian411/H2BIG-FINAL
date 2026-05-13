using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;

namespace H2BIG.Controllers
{
    public class AuthController : Controller
    {
        private readonly DatabaseHelper _db;

        public AuthController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to respective dashboard
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(role))
            {
                return RedirectToDashboard(role);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            string query = "SELECT * FROM users WHERE username = @username AND password = @password AND status = 'Active' LIMIT 1";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password)
            };

            var dt = _db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                HttpContext.Session.SetString("UserId", row["id"].ToString()!);
                HttpContext.Session.SetString("UserName", row["fullname"].ToString()!);
                HttpContext.Session.SetString("UserRole", row["role"].ToString()!);

                // Update TimeIn on login
                _db.ExecuteNonQuery("UPDATE users SET time_in = NOW() WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", row["id"]) });

                return RedirectToDashboard(row["role"].ToString()!);
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                // Update TimeOut on logout
                _db.ExecuteNonQuery("UPDATE users SET time_out = NOW() WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", userId) });
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToDashboard(string role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Staff" => RedirectToAction("Dashboard", "Staff"),
                "Rider" => RedirectToAction("Dashboard", "Rider"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
