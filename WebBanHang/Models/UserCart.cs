using System;

namespace WebBanHang.Models
{
    public class UserCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // user unique id or name
        public string CartJson { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
