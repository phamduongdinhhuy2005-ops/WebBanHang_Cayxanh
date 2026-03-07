using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WebBanHang.Models
{
    /// <summary>
    /// Model đại diện cho đơn hàng
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã người dùng")]
        public int UserId { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái")]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Tên khách hàng")]
        [MaxLength(100)]
        public string? CustomerName { get; set; }

        [Display(Name = "Email khách hàng")]
        [MaxLength(255)]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Số điện thoại")]
        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        [Display(Name = "Ghi chú")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserAccount? User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    /// <summary>
    /// Model đại diện cho chi tiết đơn hàng
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Tên sản phẩm")]
        [MaxLength(255)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Giá sản phẩm")]
        [DataType(DataType.Currency)]
        public decimal ProductPrice { get; set; }

        [Required]
        [Display(Name = "Số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Thành tiền")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
