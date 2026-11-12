using S_L.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace S_L.Controllers
{
    public class ShopController : Controller
    {
        // DISPLAY ALL PRODUCTS
        public ActionResult Index()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = Database.GetConnection())
            {
                string query = @"
                    SELECT
                        p.ProductID,
                        p.ProductName,
                        p.Description,
                        p.Price,
                        p.Category,
                        p.Image,
                        p.Stock,
                        p.IsAvailable,
                        ISNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))), 0) AS AverageRating,
                        COUNT(r.ReviewID) AS ReviewCount
                    FROM Products p
                    LEFT JOIN Reviews r
                        ON p.ProductID = r.ProductID
                        AND r.IsVisible = 1
                    WHERE p.IsAvailable = 1
                    GROUP BY
                        p.ProductID,
                        p.ProductName,
                        p.Description,
                        p.Price,
                        p.Category,
                        p.Image,
                        p.Stock,
                        p.IsAvailable
                    ORDER BY p.ProductID DESC";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(ReadProduct(reader));
                        }
                    }
                }
            }

            return View(products);
        }

        // DISPLAY PRODUCT DETAILS AND REVIEWS
        public ActionResult Details(int id)
        {
            Product product = null;

            using (SqlConnection connection = Database.GetConnection())
            {
                string query = @"
                    SELECT
                        p.ProductID,
                        p.ProductName,
                        p.Description,
                        p.Price,
                        p.Category,
                        p.Image,
                        p.Stock,
                        p.IsAvailable,
                        ISNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))), 0) AS AverageRating,
                        COUNT(r.ReviewID) AS ReviewCount
                    FROM Products p
                    LEFT JOIN Reviews r
                        ON p.ProductID = r.ProductID
                        AND r.IsVisible = 1
                    WHERE p.ProductID = @ProductID
                        AND p.IsAvailable = 1
                    GROUP BY
                        p.ProductID,
                        p.ProductName,
                        p.Description,
                        p.Price,
                        p.Category,
                        p.Image,
                        p.Stock,
                        p.IsAvailable";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", id);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = ReadProduct(reader);
                        }
                    }
                }
            }

            // PRODUCT NOT FOUND
            if (product == null)
            {
                return HttpNotFound("Product not found.");
            }

            // LOAD CUSTOMER REVIEWS
            List<Review> reviews = new List<Review>();

            using (SqlConnection connection = Database.GetConnection())
            {
                string query = @"
                    SELECT
                        r.ReviewID,
                        r.CustomerID,
                        r.ProductID,
                        c.FullName AS CustomerName,
                        r.Rating,
                        r.Comment,
                        r.ReviewDate
                    FROM Reviews r
                    INNER JOIN Customers c
                        ON r.CustomerID = c.CustomerID
                    WHERE r.ProductID = @ProductID
                        AND r.IsVisible = 1
                    ORDER BY r.ReviewDate DESC";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", id);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reviews.Add(new Review
                            {
                                ReviewID = Convert.ToInt32(reader["ReviewID"]),
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                CustomerName = reader["CustomerName"].ToString(),
                                Rating = Convert.ToInt32(reader["Rating"]),
                                Comment = reader["Comment"] == DBNull.Value
                                    ? ""
                                    : reader["Comment"].ToString(),
                                ReviewDate = Convert.ToDateTime(reader["ReviewDate"])
                            });
                        }
                    }
                }
            }

            ViewBag.Reviews = reviews;

            return View(product);
        }

        // CONVERT DATABASE DATA INTO PRODUCT MODEL
        private Product ReadProduct(SqlDataReader reader)
        {
            return new Product
            {
                ProductID = Convert.ToInt32(reader["ProductID"]),
                ProductName = reader["ProductName"].ToString(),

                Description = reader["Description"] == DBNull.Value
                    ? ""
                    : reader["Description"].ToString(),

                Price = Convert.ToDecimal(reader["Price"]),

                Category = reader["Category"] == DBNull.Value
                    ? ""
                    : reader["Category"].ToString(),

                Image = reader["Image"] == DBNull.Value
                    ? ""
                    : reader["Image"].ToString(),

                Stock = Convert.ToInt32(reader["Stock"]),
                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                AverageRating = Convert.ToDecimal(reader["AverageRating"]),
                ReviewCount = Convert.ToInt32(reader["ReviewCount"])
            };
        }
    }
}