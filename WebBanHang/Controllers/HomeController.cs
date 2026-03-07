using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    /// <summary>
    /// Controller quản lý trang chủ và các trang chung
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor với Dependency Injection
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Xử lý đăng ký nhận tin qua email từ trang Privacy
        /// POST: /Home/Subscribe
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subscribe(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["SubscribeError"] = "Email không hợp lệ.";
                    return RedirectToAction("Privacy");
                }

                var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                var filePath = Path.Combine(dataDir, "subscribers.csv");
                var line = $"{DateTime.UtcNow:O},{email}";
                System.IO.File.AppendAllLines(filePath, new[] { line });

                TempData["SubscribeSuccess"] = "Cảm ơn! Email của bạn đã được đăng ký.";
                return RedirectToAction("Privacy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký email: {Email}", email);
                TempData["SubscribeError"] = "Có lỗi xảy ra, vui lòng thử lại sau.";
                return RedirectToAction("Privacy");
            }
        }

        /// <summary>
        /// Trang chủ - GET: /
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Trang Chính sách bảo mật - GET: /Home/Privacy
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Trang Liên hệ - GET: /Home/Contact
        /// </summary>
        public IActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Xử lý form liên hệ - POST: /Home/Contact
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
                {
                    TempData["ContactError"] = "Vui lòng điền đầy đủ tên, email và nội dung.";
                    return RedirectToAction("Contact");
                }

                var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                var filePath = Path.Combine(dataDir, "messages.csv");
                var line = $"{DateTime.UtcNow:O},\"{name.Replace("\"", "''")}\",\"{email}\",\"{subject?.Replace("\"", "''")}\",\"{message.Replace("\"", "''")}\"";
                System.IO.File.AppendAllLines(filePath, new[] { line });

                TempData["ContactSuccess"] = "Cảm ơn! Tin nhắn của bạn đã được gửi. Chúng tôi sẽ phản hồi sớm.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi liên hệ: {Email}", email);
                TempData["ContactError"] = "Có lỗi xảy ra, vui lòng thử lại sau.";
                return RedirectToAction("Contact");
            }
        }

        /// <summary>
        /// Trang hiển thị lỗi - GET: /Home/Error
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


