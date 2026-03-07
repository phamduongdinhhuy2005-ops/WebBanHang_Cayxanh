-- =====================================================================================================================
-- SCRIPT MASTER: TẠO TOÀN BỘ DATABASE CHO HỆ THỐNG BÁN HÀNG 36STORE
-- =====================================================================================================================
-- Database: WebBanHangDB
-- Description: Tạo database hoàn chỉnh với tất cả tables (Users, Products, Orders, OrderItems, UserCarts)
-- Author: 36Store Development Team
-- Date: 2026
-- Version: 2.0 (Unified Script)
-- =====================================================================================================================

USE master;
GO

-- =====================================================================================================================
-- BƯỚC 1: TẠO/XÓA VÀ TẠO LẠI DATABASE
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 1: Tạo Database WebBanHangDB';
PRINT '=====================================================';
GO

IF DB_ID('WebBanHangDB') IS NOT NULL
BEGIN
    PRINT 'Database WebBanHangDB đã tồn tại. Đang xóa database cũ...';
    ALTER DATABASE WebBanHangDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WebBanHangDB;
    PRINT '✓ Database cũ đã được xóa.';
END
GO

CREATE DATABASE WebBanHangDB
COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

PRINT '✓ Database WebBanHangDB đã được tạo thành công!';
PRINT '';
GO

USE WebBanHangDB;
GO

-- =====================================================================================================================
-- BƯỚC 2: TẠO BẢNG USERS (TÀI KHOẢN NGƯỜI DÙNG)
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 2: Tạo bảng Users';
PRINT '=====================================================';
GO

CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100) NULL,
    Password NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Primary Key
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    
    -- Unique Constraint
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    
    -- Check Constraints
    CONSTRAINT CK_Users_Email_Format CHECK (Email LIKE '%@%.%'),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'User'))
);
GO

-- Index để tối ưu tìm kiếm
CREATE NONCLUSTERED INDEX IX_Users_Email ON dbo.Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_Role ON dbo.Users(Role);
GO

PRINT '✓ Bảng Users đã được tạo thành công!';
PRINT '';
GO

-- =====================================================================================================================
-- BƯỚC 3: TẠO BẢNG PRODUCTS (SẢN PHẨM)
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 3: Tạo bảng Products';
PRINT '=====================================================';
GO

CREATE TABLE dbo.Products (
    Id INT IDENTITY(1,1) NOT NULL,
    
    -- Thông tin cơ bản
    Name NVARCHAR(250) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    
    -- Giá cả (lưu dạng text để hiển thị định dạng tiền tệ)
    Price NVARCHAR(50) NULL,
    OriginalPrice NVARCHAR(50) NULL,
    Discount INT NULL,
    
    -- Thông tin chi tiết
    LongDescription NVARCHAR(MAX) NULL,
    Specifications NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Category NVARCHAR(200) NULL,
    
    -- Quản lý tồn kho
    StockQuantity INT NOT NULL DEFAULT 0,
    
    -- Timestamps
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    -- Concurrency token
    RowVersion ROWVERSION,
    
    -- Primary Key
    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    
    -- Check Constraints
    CONSTRAINT CK_Products_Discount CHECK (Discount >= 0 AND Discount <= 100),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);
GO

-- Indexes để tối ưu filter/search
CREATE NONCLUSTERED INDEX IX_Products_Category ON dbo.Products(Category);
CREATE NONCLUSTERED INDEX IX_Products_Discount ON dbo.Products(Discount) WHERE Discount IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Products_StockQuantity ON dbo.Products(StockQuantity);
CREATE NONCLUSTERED INDEX IX_Products_Name ON dbo.Products(Name);
GO

PRINT '✓ Bảng Products đã được tạo thành công!';
PRINT '';
GO

-- =====================================================================================================================
-- BƯỚC 4: TẠO BẢNG ORDERS (ĐỠN HÀNG)
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 4: Tạo bảng Orders';
PRINT '=====================================================';
GO

CREATE TABLE dbo.Orders (
    Id INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    
    -- Thông tin khách hàng
    CustomerName NVARCHAR(100) NULL,
    CustomerEmail NVARCHAR(255) NULL,
    CustomerPhone NVARCHAR(20) NULL,
    ShippingAddress NVARCHAR(500) NULL,
    
    -- Ghi chú
    Notes NVARCHAR(1000) NULL,
    
    -- Timestamps
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Primary Key
    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
    
    -- Foreign Key
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) 
        REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Check Constraints
    CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Completed'))
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX IX_Orders_UserId ON dbo.Orders(UserId);
CREATE NONCLUSTERED INDEX IX_Orders_OrderDate ON dbo.Orders(OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_Orders_Status ON dbo.Orders(Status);
GO

PRINT '✓ Bảng Orders đã được tạo thành công!';
PRINT '';
GO

-- =====================================================================================================================
-- BƯỚC 5: TẠO BẢNG ORDERITEMS (CHI TIẾT ĐƠN HÀNG)
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 5: Tạo bảng OrderItems';
PRINT '=====================================================';
GO

CREATE TABLE dbo.OrderItems (
    Id INT IDENTITY(1,1) NOT NULL,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    ProductPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    Subtotal DECIMAL(18,2) NOT NULL,
    
    -- Primary Key
    CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (Id),
    
    -- Foreign Keys
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) 
        REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) 
        REFERENCES dbo.Products(Id) ON DELETE NO ACTION,
    
    -- Check Constraints
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_Price CHECK (ProductPrice >= 0),
    CONSTRAINT CK_OrderItems_Subtotal CHECK (Subtotal >= 0)
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
CREATE NONCLUSTERED INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
GO

PRINT '✓ Bảng OrderItems đã được tạo thành công!';
PRINT '';
GO

-- =====================================================================================================================
-- BƯỚC 6: TẠO BẢNG USERCARTS (GIỎ HÀNG)
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 6: Tạo bảng UserCarts';
PRINT '=====================================================';
GO

CREATE TABLE dbo.UserCarts (
    Id INT IDENTITY(1,1) NOT NULL,
    UserId NVARCHAR(255) NOT NULL,
    CartJson NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Primary Key
    CONSTRAINT PK_UserCarts PRIMARY KEY CLUSTERED (Id),
    
    -- Unique Constraint
    CONSTRAINT UQ_UserCarts_UserId UNIQUE (UserId)
);
GO

-- Index
CREATE NONCLUSTERED INDEX IX_UserCarts_UserId ON dbo.UserCarts(UserId);
GO

PRINT '✓ Bảng UserCarts đã được tạo thành công!';
PRINT '';
GO

-- =====================================================================================================================
-- BƯỚC 7: INSERT DỮ LIỆU MẪU - USERS
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 7: Insert dữ liệu Users';
PRINT '=====================================================';
GO

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Admin account
    INSERT INTO dbo.Users (Email, DisplayName, Password, Role, CreatedAt, UpdatedAt)
    VALUES 
    (
        'admin@gmail.com', 
        N'Administrator', 
        '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', -- admin123
        'Admin',
        GETDATE(),
        GETDATE()
    );
    
    -- Test users
    INSERT INTO dbo.Users (Email, DisplayName, Password, Role, CreatedAt, UpdatedAt)
    VALUES 
    (
        'user1@gmail.com',
        N'Nguyễn Văn A',
        'e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446', -- user123
        'User',
        GETDATE(),
        GETDATE()
    ),
    (
        'user2@gmail.com',
        N'Trần Thị B',
        'e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446', -- user123
        'User',
        GETDATE(),
        GETDATE()
    );
    
    COMMIT TRANSACTION;
    PRINT '✓ Đã tạo 3 tài khoản: admin@gmail.com, user1@gmail.com, user2@gmail.com';
    PRINT '';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✗ Lỗi khi insert Users: ' + ERROR_MESSAGE();
END CATCH;
GO

-- =====================================================================================================================
-- BƯỚC 8: INSERT DỮ LIỆU MẪU - PRODUCTS
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 8: Insert dữ liệu Products';
PRINT '=====================================================';
GO

BEGIN TRY
    BEGIN TRANSACTION;
    
    INSERT INTO dbo.Products
    (Name, Description, Price, OriginalPrice, Discount,
     LongDescription, Specifications, ImageUrl, Category,
     StockQuantity, CreatedAt)
    
    SELECT
        p.Name,
        p.Description,
        p.Price,
        p.OriginalPrice,
        
        -- Chỉ set Discount nếu có giảm giá thực sự
        CASE
            WHEN calc.OriginalNum > calc.PriceNum AND p.Discount > 0
            THEN p.Discount
            ELSE NULL
        END,
        
        p.LongDescription,
        p.Specifications,
        p.ImageUrl,
        
        -- Tự động thêm tag "khuyenmai" nếu có giảm giá
        CASE
            WHEN calc.OriginalNum > calc.PriceNum AND p.Discount > 0
            THEN p.Category + N',khuyenmai'
            ELSE p.Category
        END,
        
        p.StockQuantity,
        SYSUTCDATETIME()
    
    FROM
    (
        VALUES
        ------------------------------------------------
        -- LAPTOP
        ------------------------------------------------
        (
             N'ASUS Zenbook 14 OLED (UM3406KA)',
             N'Ultrabook 1.2kg, OLED 3K 120Hz, Ryzen AI 7 mạnh mẽ, pin 75Wh bền bỉ',
             N'29,990,000 VND',
             N'32,990,000 VND',
             9,
             N'Ultrabook cao cấp siêu nhẹ 1.2kg, Ryzen AI 7 mạnh mẽ, OLED 3K 120Hz, pin 75Wh bền bỉ.',
             N'CPU: Ryzen AI 7;RAM: 16GB;SSD: 512GB;OLED 3K 120Hz',
             N'https://dlcdnwebimgs.asus.com/gain/45542e0a-cfe1-4df7-840a-8898de542793/w185/w184/fwebp',
             N'laptop',
             136
        ),
        (
             N'Dell XPS 13 Plus',
             N'Laptop cao cấp, Intel Core i7, màn hình 13.4" FHD+',
             N'35,000,000 VND',
             N'40,000,000 VND',
             12,
             N'Laptop Dell XPS 13 Plus với thiết kế hiện đại, Intel Core i7 Gen 12, RAM 16GB, SSD 512GB.',
             N'CPU: Intel Core i7-1260P;RAM: 16GB LPDDR5;SSD: 512GB NVMe;Display: 13.4" FHD+',
             N'https://i.dell.com/is/image/DellContent/content/dam/ss2/product-images/dell-client-products/notebooks/xps-notebooks/xps-13-9315/media-gallery/notebook-xps-13-9315-nt-blue-gallery-4.psd?fmt=png-alpha&pscan=auto&scl=1&hei=402&wid=606&qlt=100,1&resMode=sharp2&size=606,402&chrss=full',
             N'laptop',
             85
        ),
        (
             N'MacBook Air M2',
             N'Chip M2 mạnh mẽ, màn hình Liquid Retina 13.6"',
             N'28,990,000 VND',
             N'32,990,000 VND',
             12,
             N'MacBook Air M2 2023 với chip Apple M2, RAM 8GB, SSD 256GB, thiết kế mỏng nhẹ.',
             N'CPU: Apple M2;RAM: 8GB;SSD: 256GB;Display: 13.6" Liquid Retina',
             N'https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/mba13-midnight-select-202402?wid=904&hei=840&fmt=jpeg&qlt=90&.v=1708367688034',
             N'laptop',
             120
        ),
        
        ------------------------------------------------
        -- PHỤ KIỆN
        ------------------------------------------------
        (
             N'ROG Astral LC GeForce RTX 5090 32GB GDDR7 OC Edition',
             N'RTX 5090 32GB GDDR7, 21760 CUDA, hỗ trợ 8K',
             N'107,991,000 VND',
             N'119,990,000 VND',
             10,
             N'Card đồ họa 32GB GDDR7, 21760 CUDA, hỗ trợ 8K, tản nhiệt nước AIO.',
             N'RTX 5090;32GB GDDR7;512-bit;AIO',
             N'https://dlcdnwebimgs.asus.com/gain/89CC6037-6B3D-4BAA-A17C-8B843D1D92E0/w250/fwebp',
             N'phukien',
             202
        ),
        (
             N'Logitech MX Master 3S',
             N'Chuột không dây cao cấp, 8000 DPI, Silent Click',
             N'2,490,000 VND',
             N'2,990,000 VND',
             16,
             N'Chuột không dây Logitech MX Master 3S với cảm biến 8000 DPI, Silent Click, kết nối Bluetooth.',
             N'DPI: 8000;Battery: 70 days;Connectivity: Bluetooth, USB-C',
             N'https://resource.logitech.com/w_692,c_lpad,ar_4:3,q_auto,f_auto,dpr_1.0/d_transparent.gif/content/dam/logitech/en/products/mice/mx-master-3s/gallery/mx-master-3s-mouse-top-view-graphite.png',
             N'phukien',
             450
        ),
        (
             N'Keychron K8 Pro',
             N'Bàn phím cơ không dây, Hot-swappable, RGB',
             N'3,290,000 VND',
             N'3,290,000 VND',
             0,
             N'Bàn phím cơ Keychron K8 Pro với switch hot-swap, kết nối không dây, RGB.',
             N'Layout: TKL;Switch: Hot-swappable;Connectivity: Wireless, USB-C',
             N'https://cdn.shopify.com/s/files/1/0059/0630/1017/files/Keychron-K8-Pro-QMK-VIA-wireless-mechanical-keyboard-for-Mac-Windows-iOS-Gateron-switch-red-with-hot-swappable-RGB-backlight-aluminum-frame-tenkeyless-layout-for-Mac-Windows-Linux_1800x1800.jpg',
             N'phukien',
             300
        ),
        
        ------------------------------------------------
        -- KHÔNG GIẢM GIÁ
        ------------------------------------------------
        (
            N'HP Pavilion 15',
            N'Laptop văn phòng, Intel Core i5, 8GB RAM',
            N'15,990,000 VND',
            N'15,990,000 VND',
            0,
            N'Laptop HP Pavilion 15 phù hợp văn phòng, học tập với Intel Core i5, RAM 8GB, SSD 256GB.',
            N'CPU: Intel Core i5-1235U;RAM: 8GB;SSD: 256GB;Display: 15.6" FHD',
            N'https://ssl-product-images.www8-hp.com/digmedialib/prodimg/lowres/c08260267.png',
            N'laptop',
            144
        )
    ) AS p
    (
        Name, Description, Price, OriginalPrice, Discount,
        LongDescription, Specifications, ImageUrl, Category,
        StockQuantity
    )
    CROSS APPLY
    (
        SELECT
            TRY_CAST(REPLACE(REPLACE(p.Price, ',', ''), ' VND', '') AS BIGINT) AS PriceNum,
            TRY_CAST(REPLACE(REPLACE(p.OriginalPrice, ',', ''), ' VND', '') AS BIGINT) AS OriginalNum
    ) AS calc;
    
    COMMIT TRANSACTION;
    PRINT '✓ Đã thêm ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' sản phẩm';
    PRINT '';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✗ Lỗi khi insert Products: ' + ERROR_MESSAGE();
END CATCH;
GO

-- =====================================================================================================================
-- BƯỚC 9: INSERT DỮ LIỆU MẪU - ORDERS
-- =====================================================================================================================
PRINT '=====================================================';
PRINT 'BƯỚC 9: Insert dữ liệu Orders';
PRINT '=====================================================';
GO

BEGIN TRY
    BEGIN TRANSACTION;
    
    DECLARE @User1Id INT = (SELECT Id FROM dbo.Users WHERE Email = 'user1@gmail.com');
    
    INSERT INTO dbo.Orders (UserId, OrderDate, TotalAmount, Status, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, CreatedAt, UpdatedAt)
    VALUES 
    (
        @User1Id,
        DATEADD(DAY, -7, GETDATE()),
        29990000,
        'Completed',
        N'Nguyễn Văn A',
        'user1@gmail.com',
        '0123456789',
        N'123 Đường ABC, Quận 1, TP.HCM',
        DATEADD(DAY, -7, GETDATE()),
        DATEADD(DAY, -2, GETDATE())
    ),
    (
        @User1Id,
        DATEADD(DAY, -3, GETDATE()),
        107991000,
        'Shipped',
        N'Nguyễn Văn A',
        'user1@gmail.com',
        '0123456789',
        N'123 Đường ABC, Quận 1, TP.HCM',
        DATEADD(DAY, -3, GETDATE()),
        DATEADD(DAY, -1, GETDATE())
    );
    
    -- Thêm OrderItems
    DECLARE @Order1Id INT = (SELECT TOP 1 Id FROM dbo.Orders WHERE UserId = @User1Id ORDER BY OrderDate ASC);
    DECLARE @Order2Id INT = (SELECT TOP 1 Id FROM dbo.Orders WHERE UserId = @User1Id ORDER BY OrderDate DESC);
    
    INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductName, ProductPrice, Quantity, Subtotal)
    VALUES 
    (@Order1Id, 1, N'ASUS Zenbook 14 OLED (UM3406KA)', 29990000, 1, 29990000),
    (@Order2Id, 4, N'ROG Astral LC GeForce RTX 5090 32GB GDDR7 OC Edition', 107991000, 1, 107991000);
    
    COMMIT TRANSACTION;
    PRINT '✓ Đã tạo 2 đơn hàng mẫu cho user1@gmail.com';
    PRINT '';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✗ Lỗi khi insert Orders: ' + ERROR_MESSAGE();
END CATCH;
GO

-- =====================================================================================================================
-- BƯỚC 10: HIỂN THỊ THÔNG TIN DATABASE
-- =====================================================================================================================
PRINT '';
PRINT '=====================================================================================================================';
PRINT '                                    ✓ HOÀN THÀNH TẠO DATABASE WEBBANHANGDB                                         ';
PRINT '=====================================================================================================================';
PRINT '';
PRINT 'THÔNG TIN DATABASE:';
PRINT '-------------------';
PRINT '• Database Name: WebBanHangDB';
PRINT '• Collation: SQL_Latin1_General_CP1_CI_AS';
PRINT '• Tables: Users, Products, Orders, OrderItems, UserCarts';
PRINT '';
PRINT 'TÀI KHOẢN ADMIN:';
PRINT '----------------';
PRINT '• Email: admin@gmail.com';
PRINT '• Password: admin123';
PRINT '• Role: Admin';
PRINT '';
PRINT 'TÀI KHOẢN USER MẪU:';
PRINT '-------------------';
PRINT '• user1@gmail.com / user123 (có 2 đơn hàng)';
PRINT '• user2@gmail.com / user123';
PRINT '';
PRINT 'CONNECTION STRING:';
PRINT '------------------';
PRINT 'Data Source=.;Initial Catalog=WebBanHangDB;Integrated Security=True;TrustServerCertificate=True;';
PRINT '';
PRINT 'THỐNG KÊ DỮ LIỆU:';
PRINT '-----------------';
GO

SELECT 
    'Users' AS TableName, 
    COUNT(*) AS RecordCount 
FROM dbo.Users
UNION ALL
SELECT 'Products', COUNT(*) FROM dbo.Products
UNION ALL
SELECT 'Orders', COUNT(*) FROM dbo.Orders
UNION ALL
SELECT 'OrderItems', COUNT(*) FROM dbo.OrderItems
UNION ALL
SELECT 'UserCarts', COUNT(*) FROM dbo.UserCarts;
GO

PRINT '';
PRINT 'DANH SÁCH USERS:';
PRINT '----------------';
GO

SELECT 
    Id,
    Email,
    DisplayName,
    Role,
    FORMAT(CreatedAt, 'dd/MM/yyyy HH:mm') AS Created
FROM dbo.Users
ORDER BY Id;
GO

PRINT '';
PRINT 'DANH SÁCH PRODUCTS (TOP 5):';
PRINT '---------------------------';
GO

SELECT TOP 5
    Id,
    Name,
    Price,
    Discount,
    Category,
    StockQuantity
FROM dbo.Products
ORDER BY Id;
GO

PRINT '';
PRINT '=====================================================================================================================';
PRINT '✓ Script đã chạy hoàn tất! Database sẵn sàng sử dụng.';
PRINT '=====================================================================================================================';
PRINT '';
GO
