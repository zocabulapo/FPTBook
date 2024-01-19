using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WEBFPTBOOK.Models;

namespace WEBFPTBOOK.Controllers
{
    public class CartController : Controller
    {
        DataDataContext data = new DataDataContext();

        // GET: Cart
        public List<Cart> GetCart()
        {
            List<Cart> lstCart = Session["Cart"] as List<Cart> ?? new List<Cart>();
            Session["Cart"] = lstCart;
            return lstCart;
        }

        public ActionResult AddCart(int iBookID, string strURL)
        {
            List<Cart> lstCart = GetCart();
            Cart product = lstCart.Find(n => n.IBookID == iBookID);

            if (product == null)
            {
                product = new Cart(iBookID);
                lstCart.Add(product);
            }
            else
            {
                product.IQuatity++;
            }

            return Redirect(strURL);
        }

        private int TotalQuantity()
        {
            List<Cart> lstCart = Session["Cart"] as List<Cart>;
            return lstCart?.Sum(n => n.IQuatity) ?? 0;
        }

        private double TotalPrice()
        {
            List<Cart> lstCart = Session["Cart"] as List<Cart>;
            return lstCart?.Sum(n => n.ITotal) ?? 0;
        }

        public ActionResult Cart()
        {
            List<Cart> lstCart = GetCart();

            if (lstCart.Count == 0)
            {
                return RedirectToAction("Index", "FPTBook");
            }

            ViewBag.TotalQuantity = TotalQuantity();
            ViewBag.TotalPrice = TotalPrice();
            return View(lstCart);
        }

        public ActionResult CartPartial()
        {
            ViewBag.TotalQuantity = TotalQuantity();
            ViewBag.TotalPrice = TotalPrice();
            return PartialView();
        }

        public ActionResult DeleteCart(int iBookID)
        {
            List<Cart> lstCart = GetCart();
            Cart product = lstCart.SingleOrDefault(n => n.IBookID == iBookID);

            if (product != null)
            {
                lstCart.RemoveAll(n => n.IBookID == iBookID);
            }

            if (lstCart.Count == 0)
            {
                return RedirectToAction("Index", "FPTBook");
            }

            return RedirectToAction("Cart");
        }

        public ActionResult UpdateCart(int iBookID, FormCollection f)
        {
            List<Cart> lstCart = GetCart();
            Cart product = lstCart.SingleOrDefault(n => n.IBookID == iBookID);

            if (product != null)
            {
                int quantity;
                if (int.TryParse(f["txtQuatity"], out quantity))
                {
                    product.IQuatity = quantity;
                }
            }

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public ActionResult Order()
        {
            if (Session["Username"] == null || Session["Username"].ToString() == "")
            {
                return RedirectToAction("Login", "Username");
            }

            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "FPTBook");
            }

            List<Cart> lstCart = GetCart();
            ViewBag.TotalQuantity = TotalQuantity();
            ViewBag.TotalPrice = TotalPrice();
            return View(lstCart);
        }

        [HttpPost]
        public ActionResult Order(FormCollection collection)
        {
            // Check login
            if (Session["Username"] == null || Session["Username"].ToString() == "")
            {
                return RedirectToAction("Login", "User");
            }

            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "FPTBook");
            }

            // Get cart
            List<Cart> lstCart = GetCart();

            // Retrieve customer from session
            Customer cus = new Customer();
            if (cus == null)
            {
                // Handle the case where customer information is not available
                // Redirect to login or handle accordingly
                return RedirectToAction("Login", "User");
            }

            // Create order
            Order ord = new Order
            {
                CustomerID = (int)Session["id"],
                OrderDate = DateTime.Now,
                DeliDate = DateTime.ParseExact(collection["DeliveryDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture),
                DeliStatus = collection["DeliStatus"] == "on", // Convert checkbox value to boolean
                ComplePay = collection["ComplePay"] == "on" // Convert checkbox value to boolean
            };

            // Add order to database
            data.Orders.InsertOnSubmit(ord);
            data.SubmitChanges();

            // Add order details
            foreach (var item in lstCart)
            {
                // Check if OrderID has been generated
                if (ord.OrderID > 0)
                {
                    OrderDetail ctdh = new OrderDetail
                    {
                        OrderID = ord.OrderID,
                        BookID = item.IBookID,
                        Quality = item.IQuatity,
                        Price = (decimal)item.IPrice
                    };

                    // Add order detail to database
                    data.OrderDetails.InsertOnSubmit(ctdh);
                    data.SubmitChanges(); // Submit changes for each order detail
                }
                else
                {
                    // Handle the case where OrderID is not generated
                    // Log or handle accordingly
                    ModelState.AddModelError("", "Error placing order. Please try again.");
                    return View(lstCart);
                }
            }

            // Clear the cart after the order is successfully placed
            Session["Cart"] = null;

            return RedirectToAction("AgreeToOrder", "Cart");
        }


        public ActionResult AgreeToOrder()
        {
            return View();
        }
    }
}
