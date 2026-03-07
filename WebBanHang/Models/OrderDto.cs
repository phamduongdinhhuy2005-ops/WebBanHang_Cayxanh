using System.Collections.Generic;

namespace WebBanHang.Models
{
    // DTO ??n gi?n dùng ?? nh?n d? li?u ??t hàng t? client
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDto
    {
        // Thông tin khách hàng (??n gi?n cho demo)
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Danh sách s?n ph?m trong gi? (id + quantity)
        public List<OrderItemDto> Items { get; set; }
    }
}
