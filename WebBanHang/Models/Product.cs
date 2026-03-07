namespace WebBanHang.Models
{
    /// <summary>
    /// Model đại diện cho sản phẩm trong hệ thống
    /// </summary>
    public class Product
    {
        /// <summary>
        /// ID duy nhất của sản phẩm
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tên sản phẩm
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả ngắn gọn về sản phẩm
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Giá bán (định dạng tiền tệ để hiển thị)
        /// </summary>
        public string Price { get; set; } = string.Empty;

        /// <summary>
        /// Giá gốc trước khi giảm giá (nếu có)
        /// </summary>
        public string? OriginalPrice { get; set; }

        /// <summary>
        /// Phần trăm giảm giá (0-100)
        /// </summary>
        public int? Discount { get; set; }

        /// <summary>
        /// Mô tả chi tiết về sản phẩm
        /// </summary>
        public string LongDescription { get; set; } = string.Empty;

        /// <summary>
        /// Thông số kỹ thuật (phân cách bởi dấu ;)
        /// </summary>
        public string Specifications { get; set; } = string.Empty;

        /// <summary>
        /// URL hình ảnh sản phẩm
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Danh mục sản phẩm (laptop, phukien, khuyenmai)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng tồn kho hiện tại
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// Ngày tạo sản phẩm
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật cuối cùng
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}



