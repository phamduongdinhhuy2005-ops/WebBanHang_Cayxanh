using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebBanHang.Models;
using WebBanHang.Models.ViewModels;
using WebBanHang.Data;
using System.Security.Cryptography;
using System.Text;

namespace WebBanHang.Controllers
{
    /// <summary>
    /// Controller xử lý đăng nhập, đăng ký và quản lý tài khoản
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _db;
        private const string SessionKeyUserId = "_UserId";
        private const string SessionKeyUserEmail = "_UserEmail";
        private const string SessionKeyUserDisplayName = "_UserDisplayName";
        private const string SessionKeyUserRole = "_UserRole";

        public AuthController(ILogger<AuthController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        #region Helper Methods

        /// <summary>
        /// Lưu thông tin user vào Session
        /// </summary>
        private void SetUserSession(UserAccount user)
        {
            HttpContext.Session.SetString(SessionKeyUserId, user.Id.ToString());
            HttpContext.Session.SetString(SessionKeyUserEmail, user.Email);
            HttpContext.Session.SetString(SessionKeyUserDisplayName, user.DisplayName ?? string.Empty);
            HttpContext.Session.SetString(SessionKeyUserRole, user.Role);
        }

        /// <summary>
        /// Xóa thông tin user khỏi Session
        /// </summary>
        private void ClearUserSession()
        {
            HttpContext.Session.Remove(SessionKeyUserId);
            HttpContext.Session.Remove(SessionKeyUserEmail);
            HttpContext.Session.Remove(SessionKeyUserDisplayName);
            HttpContext.Session.Remove(SessionKeyUserRole);
        }

        /// <summary>
        /// Lấy thông tin user hiện tại từ Session
        /// </summary>
        private async Task<UserAccount?> GetCurrentUserAsync()
        {
            var userIdStr = HttpContext.Session.GetString(SessionKeyUserId);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;

            try
            {
                return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hash mật khẩu bằng SHA256 (output HEX string để match với DB)
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        /// <summary>
        /// Lấy tên hiển thị mặc định từ email
        /// </summary>
        private string GetDefaultDisplayName(string email)
        {
            if (string.IsNullOrEmpty(email))
                return "User";

            var atIndex = email.IndexOf('@');
            if (atIndex > 0)
                return email.Substring(0, atIndex);

            return email;
        }

        /// <summary>
        /// Kiểm tra email/username có chứa từ khóa admin không
        /// </summary>
        private bool ContainsAdminKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var keywords = new[] { "admin", "administrator", "root", "system" };
            var lowerText = text.ToLower();

            return keywords.Any(k => lowerText.Contains(k));
        }

        #endregion

        #region Register

        /// <summary>
        /// GET: /Auth/Register
        /// Hiển thị trang đăng ký
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// POST: /Auth/Register
        /// Xử lý đăng ký tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email có chứa từ khóa admin
                if (ContainsAdminKeywords(model.Email))
                {
                    ModelState.AddModelError("Email", "Email không được chứa các từ khóa: admin, administrator, root, system");
                    return View(model);
                }

                // Kiểm tra username (phần trước @)
                var username = model.Email.Split('@')[0];
                if (ContainsAdminKeywords(username))
                {
                    ModelState.AddModelError("Email", "Tên người dùng không được chứa các từ khóa: admin, administrator, root, system");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại trong database
                var existingUser = await _db.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký");
                    return View(model);
                }

                // Tạo tài khoản mới
                var newUser = new UserAccount
                {
                    Email = model.Email.ToLower().Trim(),
                    Password = HashPassword(model.Password),
                    DisplayName = GetDefaultDisplayName(model.Email),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Lưu vào database
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                SetUserSession(newUser);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                _logger.LogInformation("User {Email} registered successfully with ID {UserId}", newUser.Email, newUser.Id);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
                return View(model);
            }
        }

        #endregion

        #region Login

        /// <summary>
        /// GET: /Auth/Login
        /// Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Auth/Login
        /// Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var hashedPassword = HashPassword(model.Password);

                // Query từ database
                var user = await _db.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == model.Email.ToLower() &&
                    u.Password == hashedPassword);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                    _logger.LogWarning("Failed login attempt for email {Email}", model.Email);
                    return View(model);
                }

                // Lưu session
                SetUserSession(user);

                _logger.LogInformation("User {Email} (ID: {UserId}) logged in successfully", user.Email, user.Id);
                TempData["SuccessMessage"] = $"Chào mừng {user.DisplayName}!";

                // Redirect về trang trước đó hoặc trang chủ
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại.");
                return View(model);
            }
        }

        #endregion

        #region Logout

        /// <summary>
        /// POST: /Auth/Logout
        /// Đăng xuất
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var email = HttpContext.Session.GetString(SessionKeyUserEmail);
            ClearUserSession();

            _logger.LogInformation("User {Email} logged out", email);
            TempData["InfoMessage"] = "Đã đăng xuất";

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Account Profile

        /// <summary>
        /// GET: /Auth/Profile
        /// Hiển thị trang thông tin tài khoản
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", new { returnUrl = "/Auth/Profile" });
            }

            var model = new AccountProfileViewModel
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        /// <summary>
        /// POST: /Auth/Profile
        /// Cập nhật thông tin tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AccountProfileViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            try
            {
                user.DisplayName = model.DisplayName.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString(SessionKeyUserDisplayName, user.DisplayName);

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                _logger.LogInformation("User {Email} updated profile", user.Email);

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {Email}", user.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra. Vui lòng thử lại.");
                model.Email = user.Email;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }
        }

        #endregion

        #region Change Password

        /// <summary>
        /// GET: /Auth/ChangePassword
        /// Hiển thị trang đổi mật khẩu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", new { returnUrl = "/Auth/ChangePassword" });
            }

            return View();
        }

        /// <summary>
        /// POST: /Auth/ChangePassword
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra mật khẩu hiện tại
                var currentPasswordHash = HashPassword(model.CurrentPassword);
                if (user.Password != currentPasswordHash)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.Password = HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                _logger.LogInformation("User {Email} changed password", user.Email);

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {Email}", user.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra. Vui lòng thử lại.");
                return View(model);
            }
        }

        #endregion

        #region Order History (Sample Data)

        /// <summary>
        /// GET: /Auth/OrderHistory
        /// Hiển thị lịch sử mua hàng từ database
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", new { returnUrl = "/Auth/OrderHistory" });
            }

            try
            {
                // Query lịch sử đơn hàng từ database
                var orders = await _db.Orders
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                ViewData["UserEmail"] = user.Email;
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order history for user {Email}", user.Email);
                TempData["ErrorMessage"] = "Có lỗi khi tải lịch sử đơn hàng.";
                return View(new List<Order>());
            }
        }

        #endregion
    }
}
