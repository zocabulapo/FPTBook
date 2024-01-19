using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEBFPTBOOK.Models;

namespace WEBFPTBOOK.Controllers
{
    public class AdminController : Controller
    {
        private DataDataContext data = new DataDataContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BookManage()
        {
            var books = data.Books.ToList();
            return View(books);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["Username"];
            var password = collection["Password"];

            if (string.IsNullOrEmpty(username))
            {
                ViewData["Error1"] = "Please enter username!!!";
            }

            if (string.IsNullOrEmpty(password))
            {
                ViewData["Error2"] = "Please enter password!!!";
            }
            else
            {
                Admin admin = data.Admins.SingleOrDefault(n => n.UserAdmin == username && n.PassAdmin == password);

                if (admin != null)
                {
                    ViewBag.Notify = "Login successfully";
                    Session["Username"] = admin;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Notify = "Username and Password are incorrect";
                }
            }

            return View();
        }

        public ActionResult OrderManage()
        {
            var orders = data.Orders.ToList();
            return View(orders);
        }

        public ActionResult OrderDetail(int orderId)
        {
            var orderDetails = data.OrderDetails.Where(od => od.OrderID == orderId).ToList();
            return View(orderDetails);
        }

        [HttpGet]
        public ActionResult AddBook()
        {
            ViewBag.TopicID = new SelectList(data.Topics.OrderBy(n => n.TopicName), "TopicID", "TopicName");
            ViewBag.PubID = new SelectList(data.Publishers.OrderBy(n => n.PubName), "PubID", "PubName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBook(Book bookpic, Book book, HttpPostedFileBase fileupload)
        {
            ViewBag.TopicID = new SelectList(data.Topics.OrderBy(n => n.TopicName), "TopicID", "TopicName");
            ViewBag.PubID = new SelectList(data.Publishers.OrderBy(n => n.PubName), "PubID", "PubName");

            if (fileupload == null)
            {
                ViewBag.Notify = "Select Image input";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/product_imgs"), Path.GetFileName(fileName));

                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Notify = "Image already exists";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }

                    bookpic.BookPic = fileName;
                    data.Books.InsertOnSubmit(bookpic);
                    data.SubmitChanges();
                }
                return RedirectToAction("BookManage");
            }
        }

        [HttpGet]
        public ActionResult DeleteAll(int id)
        {
            var book = data.Books.SingleOrDefault(n => n.BookID == id);
            ViewBag.BookID = book?.BookID;

            if (book == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(book);
        }

        [HttpPost, ActionName("DeleteAll")]
        public ActionResult AgreeDelete(int id)
        {
            var book = data.Books.SingleOrDefault(n => n.BookID == id);
            ViewBag.BookID = book?.BookID;

            if (book == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            data.Books.DeleteOnSubmit(book);
            data.SubmitChanges();
            return RedirectToAction("BookManage");
        }

        [HttpGet]
        public ActionResult EditBook(int id)
        {
            using (var context = new DataDataContext())
            {
                var book = context.Books.Where(x => x.BookID == id).SingleOrDefault();
                return View(book);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditBook(int id, Book model, HttpPostedFileBase fileupload)
        {
            using (var context = new DataDataContext())
            {
                var book = context.Books.FirstOrDefault(x => x.BookID == id);

                if (book == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                else
                {
                    book.BookName = model.BookName;
                    book.Price = model.Price;
                    book.BookDesc = model.BookDesc;
                    book.BookDesc = model.BookDesc;

                    if (fileupload != null)
                    {
                        var fileName = Path.GetFileName(fileupload.FileName);
                        var path = Path.Combine(Server.MapPath("~/product_imgs"), Path.GetFileName(fileName));

                        if (System.IO.File.Exists(path))
                        {
                            ViewBag.Notify = "Image already exists";
                        }
                        else
                        {
                            fileupload.SaveAs(path);
                        }

                        model.BookPic = fileName;
                    }

                    book.BookPic = model.BookPic;
                    book.DayUpdate = model.DayUpdate;
                    book.Quality = model.Quality;
                    book.TopicID = model.TopicID;
                    book.PubID = model.PubID;

                    context.SubmitChanges();
                }

                return RedirectToAction("BookManage");
            }
        }

        [HttpGet]
        public ActionResult Publisher()
        {
            var publishers = data.Publishers.ToList();
            return View(publishers);
        }

        public ActionResult AddPublisher()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPublisher(Publisher pub)
        {
            data.Publishers.InsertOnSubmit(pub);
            data.SubmitChanges();
            return RedirectToAction("Publisher");
        }

        public ActionResult DeletePubliser(int id)
        {
            var publisher = data.Publishers.SingleOrDefault(n => n.PubID == id);
            data.Publishers.DeleteOnSubmit(publisher);
            data.SubmitChanges();
            return RedirectToAction("Publisher");
        }

        public ActionResult EditPublisher(int id)
        {
            return View(data.Publishers.SingleOrDefault(n => n.PubID == id));
        }

        [HttpPost]
        public ActionResult EditPublisher(Publisher pub, int id)
        {
            using (var context = new DataDataContext())
            {
                var publisher = context.Publishers.FirstOrDefault(x => x.PubID == id);

                if (publisher == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                else
                {
                    publisher.PubName = pub.PubName;
                    publisher.Address = pub.Address;
                    publisher.Phone = pub.Phone;
                    context.SubmitChanges();
                }
            }

            return RedirectToAction("Publisher");
        }

        [HttpGet]
        public ActionResult Topic()
        {
            var topics = data.Topics.ToList();
            return View(topics);
        }

        public ActionResult AddTopic()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddTopic(Topic tp)
        {
            data.Topics.InsertOnSubmit(tp);
            data.SubmitChanges();
            return RedirectToAction("Topic");
        }

        public ActionResult DeleteTopic(int id)
        {
            var topic = data.Topics.SingleOrDefault(n => n.TopicID == id);
            data.Topics.DeleteOnSubmit(topic);
            data.SubmitChanges();
            return RedirectToAction("Topic");
        }

        public ActionResult EditTopic(int id)
        {
            return View(data.Topics.SingleOrDefault(n => n.TopicID == id));
        }

        [HttpPost]
        public ActionResult EditTopic(Topic tp, int id)
        {
            using (var context = new DataDataContext())
            {
                var topic = context.Topics.FirstOrDefault(x => x.TopicID == id);

                if (topic == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                else
                {
                    topic.TopicName = tp.TopicName;
                    context.SubmitChanges();
                }

                return RedirectToAction("Topic");
            }
        }

        public ActionResult CustomerManage()
        {
            var customers = data.Customers.ToList();
            return View(customers);
        }

        public ActionResult DeleteCustomer(int id)
        {
            var customer = data.Customers.SingleOrDefault(n => n.CustomerID == id);
            data.Customers.DeleteOnSubmit(customer);
            data.SubmitChanges();
            return RedirectToAction("CustomerManage");
        }
    }
}
