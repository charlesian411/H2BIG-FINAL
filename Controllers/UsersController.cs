using Microsoft.AspNetCore.Mvc;
using H2BIG.Data;
using H2BIG.Models;
using MySqlConnector;
using System.Data;

namespace H2BIG.Controllers
{
    public class UsersController : Controller
    {
        private readonly DatabaseHelper _db;
        public UsersController(DatabaseHelper db) => _db = db;

        public IActionResult Index(string search)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

            string query = "SELECT * FROM users";
            MySqlParameter[]? parameters = null;

            if (!string.IsNullOrEmpty(search))
            {
                query += " WHERE fullname LIKE @search OR username LIKE @search";
                parameters = new MySqlParameter[] { new MySqlParameter("@search", $"%{search}%") };
            }

            var dt = _db.ExecuteQuery(query, parameters);
            return View(dt);
        }

        [HttpPost]
        public IActionResult Create(string fullname, string username, string password, string role)
        {
            _db.ExecuteNonQuery("INSERT INTO users (fullname, username, password, role) VALUES (@f, @u, @p, @r)",
                new MySqlParameter[] {
                    new MySqlParameter("@f", fullname),
                    new MySqlParameter("@u", username),
                    new MySqlParameter("@p", password),
                    new MySqlParameter("@r", role)
                });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(int id, string fullname, string username, string role)
        {
            _db.ExecuteNonQuery("UPDATE users SET fullname = @f, username = @u, role = @r WHERE id = @id",
                new MySqlParameter[] {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@f", fullname),
                    new MySqlParameter("@u", username),
                    new MySqlParameter("@r", role)
                });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _db.ExecuteNonQuery("DELETE FROM users WHERE id = @id", new MySqlParameter[] { new MySqlParameter("@id", id) });
            return RedirectToAction("Index");
        }
    }
}
