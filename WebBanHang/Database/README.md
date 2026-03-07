# Database Setup - WebBanHangDB

## 🔥 LỖI SHADOW PROPERTIES (UserId1, OrderId1) - ĐỌC ĐÂY! 🔥

**Nếu bạn gặp lỗi:**
```
warn: The foreign key property 'Order.UserId1' was created in shadow state...
warn: The foreign key property 'OrderItem.OrderId1' was created in shadow state...
```

### ✅ **AUTO FIX - CHỈ CẦN 1 LỆNH!** ⭐

**Chạy script tự động (KHUYẾN NGHỊ):**
```cmd
cd WebBanHang\Database
AUTO_FIX_ALL.bat
```

**Hoặc PowerShell:**
```powershell
cd WebBanHang\Database
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\AUTO_FIX_ALL.ps1
```

**Script sẽ tự động:**
- ✓ Kiểm tra code đã fix chưa
- ✓ Dừng app nếu đang chạy
- ✓ Xóa toàn bộ cache
- ✓ Build lại project
- ✓ Chạy app (nếu chọn Y)
- ✓ Thông báo kết quả

**⏰ Thời gian:** ~2 phút (tự động)

📖 **Chi tiết:** Xem `QUICK_FIX.md`

---

## 📁 Files Trong Thư Mục

### **Scripts Tự Động:**
1. **`AUTO_FIX_ALL.bat`** ⭐ - Script tổng hợp fix tất cả (KHUYẾN NGHỊ)
2. **`AUTO_FIX_ALL.ps1`** - Phiên bản PowerShell với nhiều tính năng hơn
3. **`QUICK_FIX.md`** - Hướng dẫn nhanh

### **Database Setup:**
1. **`setup_database_complete.sql`** - Tạo DB mới
   - ❌ **XÓA toàn bộ dữ liệu cũ**
   - ✅ Tạo lại tất cả tables
   - ✅ Insert dữ liệu mẫu (admin, user1, user2, 7 products, 2 orders)
   - **Khi nào dùng**: Lần đầu setup HOẶC muốn reset về trạng thái ban đầu

2. **`MIGRATION_FIX_USERID1.sql`** - Migration an toàn ⭐
   - ✅ **GIỮ NGUYÊN tất cả dữ liệu**
   - ✅ Chỉ sửa lỗi cấu trúc database (xóa UserId1, thêm Foreign Key)
   - ✅ Tự động backup trước khi sửa
   - ✅ Có thể restore nếu cần
   - **Khi nào dùng**: Database có cột UserId1 sai (ít gặp)

---

## 🚀 Quick Start

### A. Tạo Database Mới (Lần đầu setup)

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối tới server: `.` (localhost)
3. Mở file: `setup_database_complete.sql`
4. Click **Execute** (F5)
5. Chạy app: `dotnet run`

**Tài khoản mặc định:**
- Admin: `admin@gmail.com` / `admin123`
- User1: `user1@gmail.com` / `user123` (có 2 orders mẫu)
- User2: `user2@gmail.com` / `user123`

### B. Fix Lỗi Shadow Properties (UserId1, OrderId1) ⭐

**CHỈ CẦN 1 LỆNH DUY NHẤT:**

```cmd
cd WebBanHang\Database
AUTO_FIX_ALL.bat
```

Nhấn `Y` để chạy app → XONG!

**📖 Chi tiết:** Xem file `QUICK_FIX.md`

---

## 📊 Database Structure

- **Users**: Tài khoản (Admin/User)
- **Products**: Sản phẩm
- **Orders**: Đơn hàng
- **OrderItems**: Chi tiết đơn hàng
- **UserCarts**: Giỏ hàng (JSON)

---

## 🔧 Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=WebBanHangDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

---

## ✅ Kiểm Tra Sau Khi Setup

```sql
-- Kiểm tra tables
USE WebBanHangDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Kiểm tra dữ liệu
SELECT 'Users' AS [Table], COUNT(*) AS Records FROM Users
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'Orders', COUNT(*) FROM Orders;
```

---

## 📞 Trợ Giúp

- **Lỗi Shadow Properties:** Xem `QUICK_FIX.md`
- **Setup Database:** Chạy `setup_database_complete.sql`
- **Auto Fix:** Chạy `AUTO_FIX_ALL.bat`
