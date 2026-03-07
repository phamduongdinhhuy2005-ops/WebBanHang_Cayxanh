# 🌿 WebBanHang - Cây Xanh (36Store)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Website bán hàng trực tuyến chuyên về cây xanh và phụ kiện làm vườn, được xây dựng với **ASP.NET Core 8.0 MVC** và **Bootstrap 5**.

---

## 🚀 Quick Start

### **Bước 1: Clone Repository**
```bash
git clone https://github.com/phamduongdinhhuy2005-ops/WebBanHang_Cayxanh.git
cd WebBanHang_Cayxanh
```

### **Bước 2: Setup Database**
1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối tới server: `.` (localhost)
3. Mở file: `WebBanHang\Database\setup_database_complete.sql`
4. Nhấn **Execute (F5)**

### **Bước 3: Chạy Ứng Dụng**
```bash
cd WebBanHang
dotnet restore
dotnet run
```

### **Bước 4: Truy Cập**
- **URL:** http://localhost:5000 hoặc https://localhost:5001
- **Admin:** admin@gmail.com / admin123
- **User:** user1@gmail.com / user123

---

## ✨ Tính Năng Chính

### 🛍️ **Khách Hàng**
- ✅ Xem danh sách sản phẩm với phân trang, tìm kiếm, lọc
- ✅ Chi tiết sản phẩm (ảnh, giá, mô tả, tồn kho)
- ✅ Giỏ hàng (thêm/xóa/cập nhật số lượng)
- ✅ Thanh toán đơn hàng
- ✅ Xem lịch sử đơn hàng
- ✅ Quản lý thông tin cá nhân

### 👨‍💼 **Admin**
- ✅ Dashboard quản trị
- ✅ Quản lý sản phẩm (CRUD)
- ✅ Quản lý đơn hàng (xem, cập nhật trạng thái)
- ✅ Quản lý người dùng
- ✅ Thống kê doanh thu

### 🎨 **Giao Diện**
- ✅ Responsive design (mobile, tablet, desktop)
- ✅ Modern UI/UX với Bootstrap 5
- ✅ Hệ màu thống nhất, chuyên nghiệp
- ✅ Icons Font Awesome 6.4
- ✅ Smooth animations & transitions

---

## 🏗️ Cấu Trúc Dự Án

```
WebBanHang_Cayxanh/
│
├── .github/                    # GitHub workflows
│
├── WebBanHang/                 # Main project
│   ├── Controllers/            # MVC Controllers + API Controllers
│   │   ├── API/
│   │   │   ├── ProductsController.cs
│   │   │   └── CartController.cs
│   │   ├── HomeController.cs
│   │   ├── ProductController.cs
│   │   ├── AuthController.cs
│   │   ├── CheckoutController.cs
│   │   └── AdminController.cs
│   │
│   ├── Models/                 # Domain models
│   │   ├── Product.cs
│   │   ├── UserAccount.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── UserCart.cs
│   │   └── ViewModels/
│   │
│   ├── Services/               # Business logic
│   │   └── ProductService.cs
│   │
│   ├── Data/                   # EF Core DbContext
│   │   └── ApplicationDbContext.cs
│   │
│   ├── Views/                  # Razor views
│   │   ├── Home/
│   │   ├── Product/
│   │   ├── Auth/
│   │   ├── Checkout/
│   │   ├── Admin/
│   │   └── Shared/
│   │
│   ├── wwwroot/                # Static files
│   │   ├── css/
│   │   │   ├── color-system.css
│   │   │   ├── bootstrap-overrides.css
│   │   │   ├── custom.css
│   │   │   └── ...
│   │   ├── js/
│   │   │   ├── products.js
│   │   │   ├── product-enhancements.js
│   │   │   └── ...
│   │   └── images/
│   │
│   ├── Database/               # SQL scripts & auto-fix tools
│   │   ├── setup_database_complete.sql
│   │   ├── MIGRATION_FIX_USERID1.sql
│   │   ├── AUTO_FIX_ALL.bat
│   │   ├── AUTO_FIX_ALL.ps1
│   │   └── README.md
│   │
│   ├── Program.cs              # Application entry point
│   └── appsettings.json        # Configuration
│
└── README.md                   # This file
```

---

## 🛠️ Công Nghệ Sử Dụng

### **Backend**
- **Framework:** ASP.NET Core 8.0 MVC
- **Database:** SQL Server (LocalDB/Express/Full)
- **ORM:** Entity Framework Core 8.0
- **Authentication:** Session-based auth
- **API:** RESTful API with JSON responses

### **Frontend**
- **CSS Framework:** Bootstrap 5.3.2
- **JavaScript:** Vanilla JS (ES6+)
- **Icons:** Font Awesome 6.4.0
- **Fonts:** Google Fonts (Inter)

### **Tools**
- **IDE:** Visual Studio 2022 / VS Code
- **Database Tool:** SQL Server Management Studio (SSMS)
- **Version Control:** Git / GitHub

---

## 📡 API Documentation

### **Products API**
```http
GET /api/products
GET /api/products?category={category}
GET /api/products?search={keyword}
GET /api/products?page={page}&pageSize={size}
GET /api/products?sort={sort}
```

**Query Parameters:**
- `category`: laptop, phukien, khuyenmai, all (default)
- `search`: Tìm kiếm theo tên/mô tả
- `page`: Số trang (default: 1)
- `pageSize`: Số sản phẩm/trang (default: 8, 0 = all)
- `sort`: price_asc, price_desc, name_asc, name_desc, newest

**Response:**
```json
{
  "items": [...],
  "total": 100,
  "page": 1,
  "pageSize": 8,
  "totalPages": 13
}
```

### **Cart API**
```http
POST /api/cart/add
POST /api/cart/remove
POST /api/cart/update
GET  /api/cart/load
POST /api/cart/save
```

---

## 🎨 Color System

Dự án sử dụng hệ màu thống nhất:

```css
--color-light: #F9F7F7      /* Nền chính */
--color-surface: #DBE2EF    /* Cards/Surface */
--color-primary: #3F72AF    /* Màu chính (buttons, links) */
--color-dark: #112D4E       /* Text, header, footer */
```

Các biến thể sáng/tối được định nghĩa trong `wwwroot/css/color-system.css`

---

## 🔧 Configuration

### **Database Connection String**

Chỉnh sửa trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=WebBanHangDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### **Session Configuration**

Session timeout mặc định: **30 phút**

Cấu hình trong `Program.cs`:
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

---

## 🐛 Troubleshooting

### **Lỗi Shadow Properties (UserId1, OrderId1)**

Nếu gặp warnings:
```
warn: The foreign key property 'Order.UserId1' was created in shadow state...
```

**Giải pháp:** Chạy script tự động fix

```bash
cd WebBanHang\Database
AUTO_FIX_ALL.bat
```

Script sẽ tự động:
- Dừng app nếu đang chạy
- Xóa cache (bin, obj, Migrations, NuGet)
- Build lại project
- Chạy app

Chi tiết: Xem `WebBanHang\Database\QUICK_FIX.md`

### **Lỗi Database Connection**

1. Kiểm tra SQL Server đã chạy chưa
2. Kiểm tra connection string trong `appsettings.json`
3. Đảm bảo database `WebBanHangDB` đã được tạo

### **Lỗi Build**

```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📝 Database Schema

### **Tables**

- **Users**: Tài khoản người dùng (admin/user)
- **Products**: Sản phẩm
- **Orders**: Đơn hàng
- **OrderItems**: Chi tiết đơn hàng
- **UserCarts**: Giỏ hàng (JSON)

### **Sample Data**

Database script tạo sẵn:
- 1 tài khoản admin
- 2 tài khoản user
- 7 sản phẩm mẫu (laptop, phụ kiện)
- 2 đơn hàng mẫu

---

## 🚀 Deployment

### **Build for Production**

```bash
dotnet publish -c Release -o ./publish
```

### **Deploy to IIS**

1. Install .NET 8.0 Runtime trên server
2. Copy folder `publish` lên server
3. Cấu hình IIS Application Pool (.NET CLR Version: No Managed Code)
4. Trỏ Physical Path đến folder publish
5. Cập nhật connection string trong `appsettings.json`

---

## 📜 License

MIT License - Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

## 👨‍💻 Author

**Phạm Dương Đình Huy**
- GitHub: [@phamduongdinhhuy2005-ops](https://github.com/phamduongdinhhuy2005-ops)

---

## 🙏 Acknowledgments

- ASP.NET Core Team
- Bootstrap Team
- Font Awesome
- Google Fonts

---

## 📞 Support

Nếu gặp vấn đề:
1. Xem [Database/QUICK_FIX.md](WebBanHang/Database/QUICK_FIX.md)
2. Xem [Database/README.md](WebBanHang/Database/README.md)
3. Tạo issue trên GitHub

---

**⭐ Star project nếu bạn thấy hữu ích!**
