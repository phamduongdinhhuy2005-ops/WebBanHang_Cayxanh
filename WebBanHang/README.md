# 🛒 WebBanHang - 36Store

Website bán hàng điện tử được xây dựng với ASP.NET Core 8.0 MVC và Bootstrap 5.

---

## 🚀 Quick Start

### 1. Setup Database
```bash
# Mở SQL Server Management Studio
# Chạy file: WebBanHang\Database\setup_database_complete.sql
```

### 2. Run Application
```bash
cd WebBanHang
dotnet restore
dotnet run
```

### 3. Login
```
Admin: admin@gmail.com / admin123
User: user1@gmail.com / user123
```

Sau đó mở trình duyệt vào: `https://localhost:xxxx`

---

## 📁 Cấu Trúc Dự Án

```
WebBanHang/
├── Controllers/          # Xử lý logic điều khiển
│   ├── API/             # API Controllers (Products, Cart)
│   ├── AuthController   # Authentication
│   ├── HomeController   # Trang chủ
│   ├── ProductController# Sản phẩm
│   └── CheckoutController# Thanh toán
│
├── Models/              # Các model dữ liệu
│   ├── Product.cs
│   ├── UserAccount.cs
│   ├── Order.cs
│   └── ViewModels/
│
├── Services/            # Logic nghiệp vụ
│   └── ProductService.cs
│
├── Views/               # Giao diện Razor
│   ├── Home/
│   ├── Product/
│   ├── Auth/
│   ├── Checkout/
│   └── Shared/
│
├── Data/                # Database context
│   └── ApplicationDbContext.cs
│
├── Database/            # SQL scripts
│   └── setup_database_complete.sql
│
└── wwwroot/            # Static files
    ├── css/            # Style sheets
    ├── js/             # JavaScript
    └── lib/            # Libraries (Bootstrap, jQuery)
```

---

## ✨ Tính Năng

- ✅ Authentication (đăng nhập/đăng ký)
- ✅ Quản lý sản phẩm
- ✅ Giỏ hàng
- ✅ Thanh toán
- ✅ Quản lý đơn hàng
- ✅ Admin panel
- ✅ Responsive design (mobile-friendly)
- ✅ RESTful API

---

## 🔧 Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server + Entity Framework Core
- **Frontend**: Bootstrap 5.3, JavaScript ES6+
- **Icons**: Font Awesome 6.4

---

## 📡 API Endpoints

```
GET  /api/products              - Lấy tất cả sản phẩm
GET  /api/products?category=laptop - Lọc theo danh mục
GET  /api/products?search=asus  - Tìm kiếm
POST /api/cart/add              - Thêm vào giỏ hàng
GET  /api/cart                  - Lấy giỏ hàng
```

---

## 🗄️ Database

**Tables:**
- Users (Tài khoản)
- Products (Sản phẩm)
- Orders (Đơn hàng)
- OrderItems (Chi tiết đơn hàng)
- UserCarts (Giỏ hàng)

**Setup:** Xem `Database/README.md`

---

## 📞 Liên Hệ

- Email: support@shopexample.vn
- Phone: 0363-636-360
- Address: Thanh Hóa, Việt Nam

---

**Happy Coding!** 🚀


