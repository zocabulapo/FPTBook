using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using WEBFPTBOOK.Models;

namespace WEBFPTBOOK.Controllers
{
    public class UserController : Controller
    {
        // Tạo đối tượng quản lý dữ liệu
        DataDataContext data = new DataDataContext();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: USER
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customer cus)
        {
            cus.Password = EncodePassword(cus.Password);
            // Thêm giá trị vào cơ sở dữ liệu
            data.Customers.InsertOnSubmit(cus);
            data.SubmitChanges();
            return RedirectToAction("Index", "FPTBook");
        }

        public static string EncodePassword(string originalPassword)
        {
            // Khai báo biến
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            // Khởi tạo MD5CryptoServiceProvider, lấy bytes cho mật khẩu gốc và tính toán hash (mật khẩu đã mã hóa)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = System.Text.ASCIIEncoding.Default.GetBytes(originalPassword);
            encodedBytes = md5.ComputeHash(originalBytes);

            // Chuyển đổi các bytes đã mã hóa trở lại chuỗi có thể 'đọc được'
            return BitConverter.ToString(encodedBytes);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["UserName"];
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
                Customer cus = data.Customers.FirstOrDefault(n => n.UserName == username && n.Password == password);

                if (cus != null)
                {
                    ViewBag.Notify = "Login successfully";

                    // Đặt dữ liệu vào Session
                    Session["Username"] = cus.UserName;
                    Session["id"] = cus.CustomerID;
                    Debug.WriteLine($"Session[Username]: {Session["Username"]}");
                    Debug.WriteLine($"Session[id]: {Session["id"]}");


                    return RedirectToAction("Index", "FPTBook");
                }
                else
                {
                    ViewBag.Notify = "Username and Password are incorrect";
                }
            }
            return View();
        }

        // GET: /User/EditUser
        public ActionResult EditCus(int id)
        {
            return View(data.Customers.SingleOrDefault(n => n.CustomerID == id));
        }

        [HttpPost]
        public ActionResult EditCus(Customer cus, int id)
        {
            using (var context = new DataDataContext())
            {
                var data = context.Customers.FirstOrDefault(x => x.CustomerID == id);
                if (data == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                else
                {
                    data.FullName = cus.FullName;
                    data.UserName = cus.UserName;
                    data.Password = cus.Password;
                    data.Email = cus.Email;
                    data.Address = cus.Address;
                    data.Phone = cus.Phone;
                    data.Birthday = cus.Birthday;
                    context.SubmitChanges();
                }
            }

            return RedirectToAction("Edit");
        }

        public ActionResult Logout()
        {
            // Xóa tất cả dữ liệu khỏi Session khi đăng xuất
            Session.Clear();
            return RedirectToAction("Index", "FPTBook");
        }
    }
}
