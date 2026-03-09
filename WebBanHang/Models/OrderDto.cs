using System.Collections.Generic;

namespace WebBanHang.Models
{
    /// <summary>
    /// DTO đơn giản dùng để nhận dữ liệu đặt hàng từ client
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin thanh toán
    /// </summary>
    public class PaymentDto
    {
        public string Method { get; set; } = "COD"; // COD hoặc CARD
        public string? CardType { get; set; }
        public string? CardLast4 { get; set; }
    }

    /// <summary>
    /// DTO cho đơn hàng
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Thông tin khách hàng
        /// </summary>
        public required string CustomerName { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }

        /// <summary>
        /// Danh sách sản phẩm trong giỏ (id + quantity)
        /// </summary>
        public required List<OrderItemDto> Items { get; set; }
        
        /// <summary>
        /// Thông tin thanh toán
        /// </summary>
        public PaymentDto? Payment { get; set; }
    }
}
