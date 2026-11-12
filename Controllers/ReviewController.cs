using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using S_L.Models;

namespace S_L.Controllers
{
    public class ReviewController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(int productId, int rating, string comment)
        {
            // CHECK LOGIN
            if (Session["Email"] == null)
            {
                TempData["ReviewMessage"] =
                    "Please log in before submitting a review.";

                return RedirectToAction("Login", "Account");
            }

            // CHECK RATING
            if (rating < 1 || rating > 5)
            {
                TempData["ReviewMessage"] = "Please select a rating from 1 to 5.";

                return RedirectToAction("Details", "Shop",
                    new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ReviewMessage"] = "Please write a review.";

                return RedirectToAction("Details", "Shop",
                    new { id = productId });
            }

            string email = Session["Email"].ToString();

            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();

                // FIND CUSTOMER
                string customerQuery = @"
                    SELECT CustomerID
                    FROM Customers
                    WHERE Email = @Email";

                SqlCommand customerCommand =
                    new SqlCommand(customerQuery, connection);

                customerCommand.Parameters.AddWithValue("@Email", email);

                object customerResult = customerCommand.ExecuteScalar();

                if (customerResult == null)
                {
                    TempData["ReviewMessage"] = "Customer account not found.";

                    return RedirectToAction("Details", "Shop",
                        new { id = productId });
                }

                int customerId = Convert.ToInt32(customerResult);

                // CHECK IF CUSTOMER PURCHASED THE PRODUCT
                string purchaseQuery = @"
                    SELECT COUNT(*)
                    FROM Orders o
                    INNER JOIN OrderDetails od
                        ON o.OrderID = od.OrderID
                    WHERE o.CustomerID = @CustomerID
                    AND od.ProductID = @ProductID
                    AND o.Status <> 'Cancelled'";

                SqlCommand purchaseCommand =
                    new SqlCommand(purchaseQuery, connection);

                purchaseCommand.Parameters.AddWithValue("@CustomerID", customerId);
                purchaseCommand.Parameters.AddWithValue("@ProductID", productId);

                int purchased = Convert.ToInt32(
                    purchaseCommand.ExecuteScalar());

                if (purchased == 0)
                {
                    TempData["ReviewMessage"] =
                        "You must purchase this product before reviewing it.";

                    return RedirectToAction("Details", "Shop",
                        new { id = productId });
                }

                // CHECK EXISTING REVIEW
                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM Reviews
                    WHERE CustomerID = @CustomerID
                    AND ProductID = @ProductID";

                SqlCommand checkCommand =
                    new SqlCommand(checkQuery, connection);

                checkCommand.Parameters.AddWithValue("@CustomerID", customerId);
                checkCommand.Parameters.AddWithValue("@ProductID", productId);

                int existingReview = Convert.ToInt32(
                    checkCommand.ExecuteScalar());

                if (existingReview > 0)
                {
                    TempData["ReviewMessage"] =
                        "You have already reviewed this product.";

                    return RedirectToAction("Details", "Shop",
                        new { id = productId });
                }

                // SAVE REVIEW
                string insertQuery = @"
                    INSERT INTO Reviews
                    (CustomerID, ProductID, Rating, Comment)
                    VALUES
                    (@CustomerID, @ProductID, @Rating, @Comment)";

                SqlCommand insertCommand =
                    new SqlCommand(insertQuery, connection);

                insertCommand.Parameters.AddWithValue("@CustomerID", customerId);
                insertCommand.Parameters.AddWithValue("@ProductID", productId);
                insertCommand.Parameters.AddWithValue("@Rating", rating);
                insertCommand.Parameters.AddWithValue("@Comment", comment.Trim());

                insertCommand.ExecuteNonQuery();
            }

            TempData["ReviewMessage"] = "Thank you! Your review was submitted.";

            return RedirectToAction("Details", "Shop",
                new { id = productId });
        }
    }
}