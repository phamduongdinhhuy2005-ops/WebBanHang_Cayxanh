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
        public async Task<IActionResult> Index()
        {
            // Kiểm tra trạng thái đăng nhập
            var userId = HttpContext.Session.GetString("_UserId");
            var userEmail = HttpContext.Session.GetString("_UserEmail");
            var userRole = HttpContext.Session.GetString("_UserRole");

            _logger.LogInformation("Checkout Index: UserId={UserId}, Email={Email}, Role={Role}", userId ?? "null", userEmail ?? "null", userRole ?? "null");

            // Chặn admin truy cập checkout
            if (userRole == "Admin")
            {
                _logger.LogWarning("Checkout access denied: Admin users cannot checkout");
                TempData["ErrorMessage"] = "Tài khoản Admin không thể thực hiện thanh toán";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(userId))
            {
                // Chưa đăng nhập → chuyển hướng tới Login với returnUrl
                _logger.LogWarning("Checkout access denied: User not logged in");
                TempData["InfoMessage"] = "Vui lòng đăng nhập để tiếp tục thanh toán";
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Checkout" });
            }

            // Get user DisplayName from database
            string displayName = "";
            if (int.TryParse(userId, out int userIdInt))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userIdInt);
                if (user != null)
                {
                    displayName = user.DisplayName ?? user.Email ?? "";
                }
            }

            ViewData["Title"] = "Thanh toán";
            ViewData["UserDisplayName"] = displayName;
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
                var userRole = HttpContext.Session.GetString("_UserRole");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("PlaceOrder denied: User not logged in");
                    return Unauthorized(new { error = "Vui lòng đăng nhập để thanh toán." });
                }

                // Chặn admin đặt hàng
                if (userRole == "Admin")
                {
                    _logger.LogWarning("PlaceOrder denied: Admin users cannot place orders");
                    return Forbid();
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

                // Parse userId to int
                if (!int.TryParse(userId, out int userIdInt))
                {
                    _logger.LogError("Invalid userId format: {UserId}", userId);
                    return BadRequest(new { error = "User ID không hợp lệ." });
                }

                // Use transaction to ensure atomic operations
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

                        // Calculate total amount
                        decimal totalAmount = 0;
                        foreach (var it in validItems)
                        {
                            var prod = products.First(p => p.Id == it.Id);

                            // Parse price from string (remove VND, commas)
                            string priceStr = prod.Price ?? "0";
                            priceStr = priceStr.Replace("VND", "").Replace(",", "").Replace(".", "").Trim();
                            if (decimal.TryParse(priceStr, out decimal price))
                            {
                                totalAmount += price * it.Quantity;
                            }
                        }

                        // Create Order in Database
                        var newOrder = new Order
                        {
                            UserId = userIdInt,
                            OrderDate = DateTime.Now,
                            TotalAmount = totalAmount,
                            Status = "Pending",
                            CustomerName = order.CustomerName,
                            CustomerEmail = HttpContext.Session.GetString("_UserEmail"),
                            CustomerPhone = order.Phone,
                            ShippingAddress = order.Address,
                            Notes = null,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _db.Orders.Add(newOrder);
                        await _db.SaveChangesAsync();

                        // Create OrderItems
                        foreach (var it in validItems)
                        {
                            var prod = products.First(p => p.Id == it.Id);

                            // Parse price
                            string priceStr = prod.Price ?? "0";
                            priceStr = priceStr.Replace("VND", "").Replace(",", "").Replace(".", "").Trim();
                            decimal.TryParse(priceStr, out decimal price);

                            var orderItem = new OrderItem
                            {
                                OrderId = newOrder.Id,
                                ProductId = prod.Id,
                                ProductName = prod.Name ?? "Unknown",
                                ProductPrice = price,
                                Quantity = it.Quantity,
                                Subtotal = price * it.Quantity
                            };

                            _db.OrderItems.Add(orderItem);
                        }

                        await _db.SaveChangesAsync();

                        // Deduct stock
                        foreach (var it in validItems)
                        {
                            var prod = products.First(p => p.Id == it.Id);
                            prod.StockQuantity -= it.Quantity;
                            prod.UpdatedAt = DateTime.UtcNow;
                            _db.Products.Update(prod);
                        }

                        await _db.SaveChangesAsync();

                        // Clear user cart from database
                        var userCart = await _db.UserCarts.FirstOrDefaultAsync(c => c.UserId == userId);
                        if (userCart != null)
                        {
                            userCart.CartJson = "[]";
                            userCart.UpdatedAt = DateTime.Now;
                            _db.UserCarts.Update(userCart);
                            await _db.SaveChangesAsync();
                        }

                        await tx.CommitAsync();

                        _logger.LogInformation("Order {OrderId} created successfully for user {UserId}", newOrder.Id, userIdInt);
                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync();
                        _logger.LogError(ex, "Error during checkout transaction");
                        return StatusCode(500, new { error = "Lỗi khi xử lý đơn hàng: " + ex.Message });
                    }
                }

                return Ok(new { success = true, message = "Đơn hàng đã được đặt thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý đơn hàng");
                return StatusCode(500, new { error = "Có lỗi khi xử lý đơn hàng." });
            }
        }
    }
}
