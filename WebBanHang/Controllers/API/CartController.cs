using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBanHang.Data;
using WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace WebBanHang.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext db, ILogger<CartController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private string? GetUserId()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
                return id;
            }
            return null;
        }

        [HttpGet("load")]
        [Authorize]
        public async Task<IActionResult> Load()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var entry = await _db.UserCarts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (entry == null) return Ok(new { items = new object[0] });
            try
            {
                return Ok(new { cartJson = entry.CartJson });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart for {UserId}", userId);
                return StatusCode(500);
            }
        }

        public class SaveCartDto
        {
            public object? Cart { get; set; }
        }

        public class RemoveCartDto
        {
            public int ProductId { get; set; }
        }

        [HttpPost("save")]
        [Authorize]
        public async Task<IActionResult> Save([FromBody] SaveCartDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            try
            {
                var json = dto?.Cart == null ? string.Empty : JsonSerializer.Serialize(dto.Cart);
                var entry = await _db.UserCarts.FirstOrDefaultAsync(u => u.UserId == userId);
                if (entry == null)
                {
                    entry = new UserCart { UserId = userId, CartJson = json, UpdatedAt = DateTime.UtcNow };
                    _db.UserCarts.Add(entry);
                }
                else
                {
                    entry.CartJson = json;
                    entry.UpdatedAt = DateTime.UtcNow;
                    _db.UserCarts.Update(entry);
                }
                await _db.SaveChangesAsync();
                return Ok(new { saved = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving cart for {UserId}", userId);
                return StatusCode(500);
            }
        }

        [HttpPost("remove")]
        [Authorize]
        public async Task<IActionResult> Remove([FromBody] RemoveCartDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            try
            {
                var entry = await _db.UserCarts.FirstOrDefaultAsync(u => u.UserId == userId);
                if (entry == null || string.IsNullOrEmpty(entry.CartJson))
                {
                    // nothing to remove
                    return Ok(new { removed = false });
                }

                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var arr = JsonSerializer.Deserialize<List<CartItemDto>>(entry.CartJson, options) ?? new List<CartItemDto>();
                    var before = arr.Count;
                    arr = arr.Where(i => i.Id != dto.ProductId).ToList();
                    var after = arr.Count;
                    if (after != before)
                    {
                        entry.CartJson = JsonSerializer.Serialize(arr);
                        entry.UpdatedAt = DateTime.UtcNow;
                        _db.UserCarts.Update(entry);
                        await _db.SaveChangesAsync();
                        return Ok(new { removed = true });
                    }
                    return Ok(new { removed = false });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse or update cart JSON for user {UserId}", userId);
                    return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for {UserId}", userId);
                return StatusCode(500);
            }
        }

        private class CartItemDto { public int Id { get; set; } public int Quantity { get; set; } }
    }
}
