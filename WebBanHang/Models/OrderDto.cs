using System.Collections.Generic;

namespace WebBanHang.Models
{
    /// <summary>
    /// DTO ??n gi?n dùng ?? nh?n d? li?u ??t hàng t? client
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO cho ??n hàng
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Thông tin khách hàng (??n gi?n cho demo)
        /// </summary>
        public required string CustomerName { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }

        /// <summary>
        /// Danh sách s?n ph?m trong gi? (id + quantity)
        /// </summary>
        public required List<OrderItemDto> Items { get; set; }
    }
}
