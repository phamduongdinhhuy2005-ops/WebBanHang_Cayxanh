using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
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
                try
                {
                    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userIdInt);
                    if (user != null)
                    {
                        // Priority: DisplayName → Email → "Khách hàng"
                        if (!string.IsNullOrWhiteSpace(user.DisplayName))
                        {
                            displayName = user.DisplayName;
                        }
                        else if (!string.IsNullOrWhiteSpace(user.Email))
                        {
                            // Use part before @ as default name
                            var atIndex = user.Email.IndexOf('@');
                            displayName = atIndex > 0 ? user.Email.Substring(0, atIndex) : user.Email;
                        }
                        else
                        {
                            displayName = "Khách hàng";
                        }
                        _logger.LogInformation("Checkout: Set displayName='{DisplayName}' for user {UserId}", displayName, userIdInt);
                    }
                    else
                    {
                        _logger.LogWarning("Checkout: User {UserId} not found in database", userIdInt);
                        displayName = "Khách hàng";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting user info for checkout");
                    displayName = "Khách hàng";
                }
            }
            else
            {
                _logger.LogWarning("Checkout: Invalid userId format: {UserId}", userId);
                displayName = "Khách hàng";
            }

            ViewData["Title"] = "Thanh toán";
            ViewData["UserDisplayName"] = displayName;
            _logger.LogInformation("Checkout Index: Passing displayName='{DisplayName}' to view", displayName);
            return View();
        }

        /// <summary>
        /// POST: /Checkout/EditOrder/{id}
        /// Update existing draft order via JSON body
        /// </summary>
        [HttpPost]
        [Route("Checkout/EditOrder/{id}")]
        public async Task<IActionResult> EditOrder(int id, [FromBody] OrderDto dto)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                return Unauthorized();

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt);

            if (order == null)
                return NotFound(new { error = "Không tìm thấy đơn hàng hoặc không có quyền." });

            if (dto == null)
                return BadRequest(new { error = "Dữ liệu không hợp lệ." });

            // Ensure only draft orders can be edited
            if (order.Notes == null || !order.Notes.StartsWith("[CHỜ XÁC NHẬN]"))
            {
                return BadRequest(new { error = "Chỉ có thể chỉnh sửa đơn hàng nháp." });
            }

            // Validate basic fields
            if (string.IsNullOrWhiteSpace(dto.CustomerName) || string.IsNullOrWhiteSpace(dto.Phone) || string.IsNullOrWhiteSpace(dto.Address))
                return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin giao hàng." });

            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest(new { error = "Đơn hàng phải có ít nhất một sản phẩm." });

            // Load products
            var ids = dto.Items.Select(i => i.Id).ToList();
            var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

            // Validate and rebuild order items
            _db.OrderItems.RemoveRange(order.OrderItems);
            decimal total = 0;
            foreach (var it in dto.Items)
            {
                var prod = products.FirstOrDefault(p => p.Id == it.Id);
                if (prod == null)
                    return BadRequest(new { error = $"Sản phẩm ID {it.Id} không tồn tại." });

                if (prod.StockQuantity < it.Quantity)
                    return BadRequest(new { error = $"Sản phẩm '{prod.Name}' không đủ tồn kho." });

                // parse price
                string priceStr = prod.Price ?? "0";
                priceStr = priceStr.Replace("VND", "").Replace(",", "").Replace(".", "").Trim();
                decimal.TryParse(priceStr, out decimal price);

                var oi = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = prod.Id,
                    ProductName = prod.Name ?? "",
                    ProductPrice = price,
                    Quantity = it.Quantity,
                    Subtotal = price * it.Quantity
                };
                _db.OrderItems.Add(oi);
                total += oi.Subtotal;
            }

            // Update order fields
            order.CustomerName = dto.CustomerName;
            order.CustomerPhone = dto.Phone;
            order.ShippingAddress = dto.Address;
            order.TotalAmount = total;
            order.UpdatedAt = DateTime.Now;
            if (dto.Payment != null)
            {
                order.Notes = dto.Payment.Method == "CARD" ? $"Thanh toán bằng thẻ {dto.Payment.CardType} (****{dto.Payment.CardLast4})" : "Thanh toán khi nhận hàng";
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, orderId = order.Id });
        }

        /// <summary>
        /// Nhận order dạng JSON, tạo đơn hàng tạm (Draft) và chuyển đến trang xác nhận
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

                // Create draft order (no stock deduction yet)
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

                    // Create Pending Order in Database (chưa xác nhận - chưa trừ stock)
                    var paymentInfo = order.Payment?.Method == "CARD" ? $"Thanh toán bằng thẻ {order.Payment.CardType}" : "Thanh toán khi nhận hàng";
                    var newOrder = new Order
                    {
                        UserId = userIdInt,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = "Pending", // Pending - nhưng chưa confirm (xem Notes)
                        CustomerName = order.CustomerName,
                        CustomerEmail = HttpContext.Session.GetString("_UserEmail"),
                        CustomerPhone = order.Phone,
                        ShippingAddress = order.Address,
                        Notes = $"[CHỜ XÁC NHẬN] {paymentInfo}", // Prefix để phân biệt chưa confirm
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

                    _logger.LogInformation("Draft order {OrderId} created for user {UserId}", newOrder.Id, userIdInt);

                    // Return orderId để redirect đến trang xác nhận
                    return Ok(new { success = true, orderId = newOrder.Id });
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error creating draft order. Inner: {Inner}", dbEx.InnerException?.Message);
                    return StatusCode(500, new { error = "Lỗi database khi tạo đơn hàng: " + (dbEx.InnerException?.Message ?? dbEx.Message) });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating draft order");
                    return StatusCode(500, new { error = "Lỗi khi tạo đơn hàng: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý đơn hàng");
                return StatusCode(500, new { error = "Có lỗi khi xử lý đơn hàng." });
            }
        }

        /// <summary>
        /// GET: /Checkout/Confirmation/{id}
        /// Trang xác nhận đơn hàng
        /// </summary>
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng.";
                return RedirectToAction("Login", "Auth");
            }

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory", "Auth");
            }

            // Chỉ cho phép xem đơn hàng chưa xác nhận
            if (order.Notes == null || !order.Notes.StartsWith("[CHỜ XÁC NHẬN]"))
            {
                TempData["InfoMessage"] = "Đơn hàng đã được xác nhận trước đó.";
                return RedirectToAction("Success", new { id });
            }

            ViewData["Title"] = $"Xác nhận đơn hàng #{order.Id}";
            return View(order);
        }

        /// <summary>
        /// POST: /Checkout/ConfirmOrder/{id}
        /// Xác nhận và hoàn tất đơn hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xác nhận đơn hàng.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt 
                        && o.Notes != null && o.Notes.StartsWith("[CHỜ XÁC NHẬN]"));

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc đơn hàng đã được xử lý.";
            return RedirectToAction("OrderHistory", "Auth");
                }

                // Validate stock again before confirming
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null && item.Product.StockQuantity < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' không đủ tồn kho. Vui lòng điều chỉnh đơn hàng.";
                        return RedirectToAction("Confirmation", new { id });
                    }
                }

                // Xác nhận đơn hàng - xóa prefix [CHỜ XÁC NHẬN]
                if (order.Notes != null && order.Notes.StartsWith("[CHỜ XÁC NHẬN] "))
                {
                    order.Notes = order.Notes.Substring("[CHỜ XÁC NHẬN] ".Length); // Remove prefix
                }
                order.Status = "Pending"; // Giữ nguyên Pending
                order.UpdatedAt = DateTime.Now;

                // Deduct stock
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity -= item.Quantity;
                        item.Product.UpdatedAt = DateTime.UtcNow;
                        _db.Products.Update(item.Product);
                    }
                }

                // Clear user cart
                var userCart = await _db.UserCarts.FirstOrDefaultAsync(c => c.UserId == userId);
                if (userCart != null)
                {
                    userCart.CartJson = "[]";
                    userCart.UpdatedAt = DateTime.Now;
                    _db.UserCarts.Update(userCart);
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} confirmed by user {UserId}", order.Id, userIdInt);
                TempData["SuccessMessage"] = "Đơn hàng đã được xác nhận thành công!";

                return RedirectToAction("Success", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi xác nhận đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("Confirmation", new { id });
            }
        }

        /// <summary>
        /// POST: /Checkout/CancelOrder/{id}
        /// Hủy đơn hàng Draft
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt 
                        && o.Notes != null && o.Notes.StartsWith("[CHỜ XÁC NHẬN]"));

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc đơn hàng đã được xác nhận.";
                    return RedirectToAction("Index");
                }

                // Delete order items
                _db.OrderItems.RemoveRange(order.OrderItems);

                // Delete order
                _db.Orders.Remove(order);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Draft order {OrderId} cancelled by user {UserId}", id, userIdInt);
                TempData["InfoMessage"] = "Đơn hàng đã được hủy.";

                return RedirectToAction("OrderHistory", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                TempData["ErrorMessage"] = "Có lỗi khi hủy đơn hàng.";
                return RedirectToAction("Confirmation", new { id });
            }
        }

        /// <summary>
        /// GET: /Checkout/Success/{id}
        /// Trang hiển thị đơn hàng đã xác nhận thành công
        /// </summary>
        public async Task<IActionResult> Success(int id)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng.";
                return RedirectToAction("Login", "Auth");
            }

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory", "Auth");
            }

            if (order.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Đơn hàng chưa được xác nhận.";
                return RedirectToAction("OrderHistory", "Auth");
            }

            ViewData["Title"] = "Đặt hàng thành công";
            return View(order);
        }

        /// <summary>
        /// GET: /Checkout/EditOrder/{id}
        /// Hiển thị trang chỉnh sửa đơn hàng
        /// </summary>
        public async Task<IActionResult> EditOrder(int id)
        {
            var userId = HttpContext.Session.GetString("_UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userIdInt);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.");
            }

            // Ensure only draft orders can be edited
            if (order.Notes == null || !order.Notes.StartsWith("[CHỜ XÁC NHẬN]"))
            {
                TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa đơn hàng nháp.";
                return RedirectToAction("OrderHistory", "Auth");
            }

            return View(order);
        }
    }
}
