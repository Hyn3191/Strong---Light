using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace S_L.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal
        {
            get { return Price * Quantity; }
        }
    }

    public class CustomerOrder
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public List<CartItem> Items { get; set; }
    }

    // REVIEW MODEL
    public class Review
    {
        public int ReviewID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    public class ShopData
    {
        public List<Customer> Customers { get; set; }
        public List<Product> Products { get; set; }
        public List<CustomerOrder> Orders { get; set; }

        public ShopData()
        {
            Customers = new List<Customer>();
            Products = new List<Product>();
            Orders = new List<CustomerOrder>();
        }
    }
}