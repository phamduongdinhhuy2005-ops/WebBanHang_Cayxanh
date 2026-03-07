using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Controllers
{
    /// <summary>
    /// Controller quản lý trang sản phẩm
    /// </summary>
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Constructor với Dependency Injection
        /// </summary>
        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách sản phẩm - GET: /Product
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}


