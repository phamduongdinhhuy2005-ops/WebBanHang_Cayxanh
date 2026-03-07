using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebBanHang.Controllers
{
    /// <summary>
    /// Controller đơn giản cho trang Thanh toán
    /// </summary>
    public class CheckoutController : Controller
    {
        private readonly ILogger<CheckoutController> _logger;
        private readonly WebBanHang.Data.ApplicationDbContext _db;

        public CheckoutController(ILogger<CheckoutController> logger, WebBanHang.Data.ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Hiển thị trang Checkout - Yêu cầu đăng nhập
        /// </summary>
        public IActionResult Index()
        {
            // Kiểm tra trạng thái đăng nhập
            var userId = HttpContext.Session.GetString("_UserId");
            var userEmail = HttpContext.Session.GetString("_UserEmail");

            _logger.LogInformation("Checkout Index: UserId={UserId}, Email={Email}", userId ?? "null", userEmail ?? "null");

            if (string.IsNullOrEmpty(userId))
            {
                // Chưa đăng nhập → chuyển hướng tới Login với returnUrl
                _logger.LogWarning("Checkout access denied: User not logged in");
                TempData["InfoMessage"] = "Vui lòng đăng nhập để tiếp tục thanh toán";
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Checkout" });
            }

            ViewData["Title"] = "Thanh toán";
            return View();
        }

        /// <summary>
        /// Nhận order dạng JSON, kiểm tra tồn kho và xử lý thanh toán
        /// POST: /Checkout/PlaceOrder
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto order)
        {
            try
            {
                // Check login status
                var userId = HttpContext.Session.GetString("_UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("PlaceOrder denied: User not logged in");
                    return Unauthorized(new { error = "Vui lòng đăng nhập để thanh toán." });
                }

                // Basic validation
                if (order == null || order.Items == null || order.Items.Count == 0)
                    return BadRequest(new { error = "Giỏ hàng rỗng hoặc dữ liệu không hợp lệ." });

                if (string.IsNullOrWhiteSpace(order.CustomerName))
                    return BadRequest(new { error = "Vui lòng nhập họ tên." });

                if (string.IsNullOrWhiteSpace(order.Phone) || order.Phone.Length < 7)
                    return BadRequest(new { error = "Số điện thoại không hợp lệ." });

                if (string.IsNullOrWhiteSpace(order.Address))
                    return BadRequest(new { error = "Vui lòng nhập địa chỉ giao hàng." });

                // Filter out invalid items (id <= 0)
                var validItems = order.Items.Where(i => i.Id > 0).ToList();
                if (validItems.Count == 0)
                    return BadRequest(new { error = "Không có sản phẩm hợp lệ trong giỏ hàng." });

                if (validItems.Count != order.Items.Count)
                {
                    _logger.LogWarning("Removed {Count} invalid items from order", order.Items.Count - validItems.Count);
                }

                // Use transaction to ensure atomic stock update
                using (var tx = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var ids = validItems.Select(i => i.Id).ToList();
                        var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

                        // Validate stock availability
                        foreach (var it in validItems)
                        {
                            var prod = products.FirstOrDefault(p => p.Id == it.Id);
                            if (prod == null)
                                return BadRequest(new { error = $"Sản phẩm ID {it.Id} không tồn tại." });

                            if (prod.StockQuantity < it.Quantity)
                                return BadRequest(new { error = $"Sản phẩm '{prod.Name}' không đủ tồn kho. (Còn: {prod.StockQuantity})" });
                        }

                        // Deduct stock
                        foreach (var it in validItems)
                        {
                            var prod = products.First(p => p.Id == it.Id);
                            prod.StockQuantity -= it.Quantity;
                            _db.Products.Update(prod);
                        }

                        await _db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync();
                        _logger.LogError(ex, "Error during checkout stock update");
                        return StatusCode(500, new { error = "Lỗi khi cập nhật tồn kho." });
                    }
                }

                // Persist order to CSV (after stock updated)
                var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                var filePath = Path.Combine(dataDir, "orders.csv");
                var itemsJson = JsonSerializer.Serialize(order.Items);
                var safeName = (order.CustomerName ?? string.Empty).Replace('"', '\'');
                var safePhone = (order.Phone ?? string.Empty).Replace('"', '\'');
                var safeAddress = (order.Address ?? string.Empty).Replace('"', '\'');
                var safeItems = itemsJson.Replace('"', '\'');
                var line = $"{DateTime.UtcNow:O},\"{safeName}\",\"{safePhone}\",\"{safeAddress}\",\"{safeItems}\"";
                await System.IO.File.AppendAllLinesAsync(filePath, new[] { line });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý đơn hàng");
                return StatusCode(500, new { error = "Có lỗi khi xử lý đơn hàng." });
            }
        }
    }
}
