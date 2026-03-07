using System;
using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    /// <summary>
    /// Model đại diện cho tài khoản người dùng
    /// </summary>
    public class UserAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Tên hiển thị")]
        [MaxLength(100, ErrorMessage = "Tên hiển thị không được vượt quá 100 ký tự")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
