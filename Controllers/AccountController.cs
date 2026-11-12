using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.Mvc;
using S_L.Models;

namespace S_L.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AccountModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim();

            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();

                // CHECK IF EMAIL ALREADY EXISTS
                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM Customers
                    WHERE Email = @Email";

                using (SqlCommand checkCommand =
                    new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", email);

                    int exists = Convert.ToInt32(
                        checkCommand.ExecuteScalar());

                    if (exists > 0)
                    {
                        ModelState.AddModelError(
                            "Email", "Email already registered.");

                        return View(model);
                    }
                }

                // CREATE PASSWORD HASH
                byte[] salt = new byte[16];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string hash;

                using (var pbkdf = new Rfc2898DeriveBytes(
                    model.Password, salt, 10000))
                {
                    hash = Convert.ToBase64String(
                        pbkdf.GetBytes(32));
                }

                string passwordHash =
                    Convert.ToBase64String(salt) + ":" + hash;

                // SAVE CUSTOMER TO SQL SERVER
                string insertQuery = @"
                    INSERT INTO Customers
                    (FullName, Email, PasswordHash, Role)
                    VALUES
                    (@FullName, @Email, @PasswordHash, 'Customer')";

                using (SqlCommand insertCommand =
                    new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue(
                        "@FullName", model.FullName.Trim());

                    insertCommand.Parameters.AddWithValue(
                        "@Email", email);

                    insertCommand.Parameters.AddWithValue(
                        "@PasswordHash", passwordHash);

                    insertCommand.ExecuteNonQuery();
                }
            }

            TempData["Message"] =
                "Registration successful! Please login.";

            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your email and password.";
                return View();
            }

            string fullName = null;
            string storedHash = null;
            int customerId = 0;

            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT CustomerID, FullName, PasswordHash
                    FROM Customers
                    WHERE Email = @Email";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Email", email.Trim());

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customerId = Convert.ToInt32(
                                reader["CustomerID"]);

                            fullName = reader["FullName"].ToString();

                            storedHash = reader["PasswordHash"].ToString();
                        }
                    }
                }
            }

            // CHECK PASSWORD
            bool passwordIsCorrect = false;

            if (!string.IsNullOrEmpty(storedHash))
            {
                string[] parts = storedHash.Split(':');

                if (parts.Length == 2)
                {
                    try
                    {
                        byte[] salt = Convert.FromBase64String(parts[0]);

                        using (var pbkdf = new Rfc2898DeriveBytes(
                            password, salt, 10000))
                        {
                            string enteredHash =
                                Convert.ToBase64String(
                                    pbkdf.GetBytes(32));

                            passwordIsCorrect =
                                enteredHash == parts[1];
                        }
                    }
                    catch
                    {
                        passwordIsCorrect = false;
                    }
                }
            }

            if (passwordIsCorrect)
            {
                Session["CustomerID"] = customerId;
                Session["User"] = fullName;
                Session["Email"] = email.Trim();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}