using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using S_L.Models;

namespace S_L.Controllers
{
    public class CartController : Controller
    {
        private List<CartItem> GetCart()
        {
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<CartItem>();
            }

            return (List<CartItem>)Session["Cart"];
        }

        // DISPLAY CART
        public ActionResult Index()
        {
            return View(GetCart());
        }

        // ADD PRODUCT TO CART
        public ActionResult Add(int id)
        {
            Product product = null;

            using (SqlConnection connection = Database.GetConnection())
            {
                string query = @"
                    SELECT ProductID, ProductName, Price, Stock
                    FROM Products
                    WHERE ProductID = @ID AND IsAvailable = 1";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    product = new Product
                    {
                        ProductID = Convert.ToInt32(reader["ProductID"]),
                        ProductName = reader["ProductName"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]),
                        Stock = Convert.ToInt32(reader["Stock"])
                    };
                }

                reader.Close();
            }

            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index", "Shop");
            }

            if (product.Stock <= 0)
            {
                TempData["Message"] = "This product is out of stock.";
                return RedirectToAction("Details", "Shop",
                    new { id = id });
            }

            List<CartItem> cart = GetCart();

            CartItem existing = cart.FirstOrDefault(
                x => x.ProductId == product.ProductID);

            if (existing != null)
            {
                if (existing.Quantity < product.Stock)
                {
                    existing.Quantity++;
                }
                else
                {
                    TempData["Message"] = "Maximum available stock reached.";
                }
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // UPDATE QUANTITY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, int quantity)
        {
            List<CartItem> cart = GetCart();

            CartItem item = cart.FirstOrDefault(
                x => x.ProductId == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    int stock = 0;

                    using (SqlConnection connection = Database.GetConnection())
                    {
                        string query = @"
                            SELECT Stock FROM Products
                            WHERE ProductID = @ID
                            AND IsAvailable = 1";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@ID", id);

                        connection.Open();

                        object result = command.ExecuteScalar();

                        if (result != null)
                            stock = Convert.ToInt32(result);
                    }

                    if (quantity <= stock)
                    {
                        item.Quantity = quantity;
                    }
                    else
                    {
                        TempData["Message"] =
                            "Requested quantity exceeds available stock.";
                    }
                }
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // REMOVE PRODUCT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            List<CartItem> cart = GetCart();

            CartItem item = cart.FirstOrDefault(
                x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }
    }
}