using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebBanHang.Controllers
{
    /// <summary>
    /// Admin Controller - Quản lý Users và Products
    /// Chỉ dành cho tài khoản có Role = "Admin"
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _db;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        #region Helper Methods

        /// <summary>
        /// Kiểm tra xem user hiện tại có phải Admin không
        /// </summary>
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("_UserRole");
            return role == "Admin";
        }

        /// <summary>
        /// Hash password bằng SHA256 (hex lowercase)
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// GET: /Admin/Index
        /// Trang Dashboard của Admin
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Quản trị hệ thống";
            
            // Thống kê
            ViewData["TotalUsers"] = await _db.Users.CountAsync();
            ViewData["TotalProducts"] = await _db.Products.CountAsync();
            ViewData["TotalOrders"] = await _db.Orders.CountAsync();
            ViewData["LowStockProducts"] = await _db.Products.CountAsync(p => p.StockQuantity < 10);

            return View();
        }

        #endregion

        #region User Management

        /// <summary>
        /// GET: /Admin/Users
        /// Danh sách tài khoản người dùng
        /// </summary>
        public async Task<IActionResult> Users(int page = 1)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Quản lý tài khoản";
            ViewData["CurrentPage"] = page;

            var query = _db.Users.AsQueryable();

            // Pagination
            const int pageSize = 5;
            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            ViewData["TotalPages"] = totalPages;
            ViewData["TotalUsers"] = totalUsers;

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(users);
        }

        /// <summary>
        /// GET: /Admin/CreateUser
        /// Form tạo tài khoản mới
        /// </summary>
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Tạo tài khoản mới";
            return View();
        }

        /// <summary>
        /// POST: /Admin/CreateUser
        /// Xử lý tạo tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string displayName, string password, string role)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    TempData["ErrorMessage"] = "Email và mật khẩu không được để trống.";
                    return RedirectToAction("CreateUser");
                }

                if (password.Length < 6)
                {
                    TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                    return RedirectToAction("CreateUser");
                }

                // Check email exists
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = $"Email {email} đã được sử dụng.";
                    return RedirectToAction("CreateUser");
                }

                // Create user
                var newUser = new UserAccount
                {
                    Email = email,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName,
                    Password = HashPassword(password),
                    Role = role == "Admin" ? "Admin" : "User",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo tài khoản {email} thành công!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                TempData["ErrorMessage"] = "Có lỗi khi tạo tài khoản.";
                return RedirectToAction("CreateUser");
            }
        }

        /// <summary>
        /// GET: /Admin/EditUser/{id}
        /// Form chỉnh sửa tài khoản
        /// </summary>
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Users");
            }

            ViewData["Title"] = "Chỉnh sửa tài khoản";
            return View(user);
        }

        /// <summary>
        /// POST: /Admin/EditUser/{id}
        /// Cập nhật thông tin tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, string email, string displayName, string role, string? newPassword)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction("Users");
                }

                // Update fields
                user.Email = email;
                user.DisplayName = displayName;
                user.Role = role == "Admin" ? "Admin" : "User";
                user.UpdatedAt = DateTime.Now;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    if (newPassword.Length < 6)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                        return RedirectToAction("EditUser", new { id });
                    }
                    user.Password = HashPassword(newPassword);
                }

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi cập nhật tài khoản.";
                return RedirectToAction("EditUser", new { id });
            }
        }

        /// <summary>
        /// POST: /Admin/DeleteUser/{id}
        /// Xóa tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction("Users");
                }

                // Prevent deleting yourself
                var currentUserId = HttpContext.Session.GetString("_UserId");
                if (user.Id.ToString() == currentUserId)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản của chính bạn.";
                    return RedirectToAction("Users");
                }

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa tài khoản {user.Email}";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi xóa tài khoản.";
                return RedirectToAction("Users");
            }
        }

        #endregion

        #region Product Management

        /// <summary>
        /// GET: /Admin/Products
        /// Danh sách sản phẩm
        /// </summary>
        public async Task<IActionResult> Products(string? search, string? category, int page = 1)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Quản lý sản phẩm";
            ViewData["Search"] = search;
            ViewData["Category"] = category;
            ViewData["CurrentPage"] = page;

            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.Contains(category));
            }

            // Pagination
            const int pageSize = 5;
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewData["TotalPages"] = totalPages;
            ViewData["TotalProducts"] = totalProducts;

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(products);
        }

        /// <summary>
        /// GET: /Admin/CreateProduct
        /// Form tạo sản phẩm mới
        /// </summary>
        public IActionResult CreateProduct()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Thêm sản phẩm mới";
            return View();
        }

        /// <summary>
        /// POST: /Admin/CreateProduct
        /// Tạo sản phẩm mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    TempData["ErrorMessage"] = "Tên sản phẩm không được để trống.";
                    return View(product);
                }

                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thêm sản phẩm '{product.Name}' thành công!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                TempData["ErrorMessage"] = "Có lỗi khi tạo sản phẩm.";
                return View(product);
            }
        }

        /// <summary>
        /// GET: /Admin/EditProduct/{id}
        /// Form chỉnh sửa sản phẩm
        /// </summary>
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Products");
            }

            ViewData["Title"] = "Chỉnh sửa sản phẩm";
            return View(product);
        }

        /// <summary>
        /// POST: /Admin/EditProduct/{id}
        /// Cập nhật sản phẩm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var existingProduct = await _db.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Products");
                }

                // Update fields
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.LongDescription = product.LongDescription;
                existingProduct.Price = product.Price;
                existingProduct.OriginalPrice = product.OriginalPrice;
                existingProduct.Discount = product.Discount;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Category = product.Category;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Specifications = product.Specifications;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                _db.Products.Update(existingProduct);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi cập nhật sản phẩm.";
                return View(product);
            }
        }

        /// <summary>
        /// POST: /Admin/DeleteProduct/{id}
        /// Xóa sản phẩm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Products");
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa sản phẩm '{product.Name}'";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi xóa sản phẩm. Có thể sản phẩm đang có trong đơn hàng.";
                return RedirectToAction("Products");
            }
        }

        #endregion

        #region Order Management

        /// <summary>
        /// GET: /Admin/Orders
        /// Quản lý đơn hàng
        /// </summary>
        public async Task<IActionResult> Orders(string? status, int page = 1)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Quản lý đơn hàng";
            ViewData["Status"] = status;
            ViewData["CurrentPage"] = page;

            var query = _db.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Pagination
            const int pageSize = 5;
            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            ViewData["TotalPages"] = totalPages;
            ViewData["TotalOrders"] = totalOrders;

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>
        /// GET: /Admin/OrderDetail/{id}
        /// Chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> OrderDetail(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders");
            }

            ViewData["Title"] = $"Chi tiết đơn hàng #{order.Id}";
            return View(order);
        }

        /// <summary>
        /// POST: /Admin/UpdateOrderStatus/{id}
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var order = await _db.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Orders");
                }

                order.Status = status;
                order.UpdatedAt = DateTime.Now;

                _db.Orders.Update(order);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.Id} thành '{status}'";
                return RedirectToAction("OrderDetail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi cập nhật trạng thái đơn hàng.";
                return RedirectToAction("Orders");
            }
        }

        #endregion
    }
}
