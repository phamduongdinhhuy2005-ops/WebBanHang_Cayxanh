# Database Setup - WebBanHangDB

## Quick Start

### 1. Chạy SQL Script
```
File: setup_database_complete.sql
Server: . (localhost)
Authentication: Windows
```

### 2. Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=WebBanHangDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### 3. Default Accounts
```
Admin: admin@gmail.com / admin123
User1: user1@gmail.com / user123
User2: user2@gmail.com / user123
```

## Database Structure
- Users (3 records)
- Products (7 records)
- Orders (2 records)
- OrderItems (2 records)
- UserCarts (0 records)

---

**Note:** Script tự động xóa và tạo lại database nếu đã tồn tại.
