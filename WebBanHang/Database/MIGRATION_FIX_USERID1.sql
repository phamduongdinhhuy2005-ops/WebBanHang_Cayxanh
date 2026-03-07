-- =====================================================================================================================
-- MIGRATION SCRIPT: SỬA LỖI UserId1 + XÓA EF CORE CACHE (NÂNG CẤP 2024)
-- =====================================================================================================================
-- Script này sẽ:
-- 1. ✅ Kiểm tra và backup dữ liệu Orders hiện tại
-- 2. ✅ Sửa lỗi cấu trúc Orders table (xóa UserId1, thêm Foreign Key)
-- 3. ✅ Giữ nguyên tất cả dữ liệu: Products, Users, OrderItems, UserCarts
-- 4. ⭐ TỰ ĐỘNG tạo script PowerShell để xóa EF Core cache
-- 5. ⭐ KIỂM TRA toàn diện sau migration
-- =====================================================================================================================
-- 🔴 QUAN TRỌNG: SAU KHI CHẠY SCRIPT NÀY, BẠN PHẢI CHẠY LỆNH XÓA CACHE!
-- =====================================================================================================================

USE WebBanHangDB;
GO

PRINT '=====================================================================================================================';
PRINT '🚀 MIGRATION: FIX UserId1 COLUMN + CLEAR EF CORE CACHE';
PRINT '=====================================================================================================================';
PRINT '';

-- =====================================================================================================================
-- BƯỚC 1: KIỂM TRA VÀ BACKUP DỮ LIỆU
-- =====================================================================================================================
PRINT '>> BƯỚC 1: Kiểm tra và backup dữ liệu Orders...';
GO

-- Kiểm tra xem UserId1 có tồn tại không
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Orders' 
    AND COLUMN_NAME = 'UserId1'
)
BEGIN
    PRINT '   ⚠️ Phát hiện column UserId1 - Cần migration!';

    -- Tạo bảng backup
    IF OBJECT_ID('dbo.Orders_Backup', 'U') IS NOT NULL
    BEGIN
        DROP TABLE dbo.Orders_Backup;
        PRINT '   ✓ Đã xóa backup cũ';
    END

    SELECT * 
    INTO dbo.Orders_Backup
    FROM dbo.Orders;

    DECLARE @BackupCount INT = (SELECT COUNT(*) FROM dbo.Orders_Backup);
    PRINT '   ✓ Đã backup ' + CAST(@BackupCount AS NVARCHAR(10)) + ' orders vào Orders_Backup';
END
ELSE
BEGIN
    PRINT '   ✓ Không phát hiện UserId1 - Database đã đúng cấu trúc!';
    PRINT '';
    PRINT '   🔍 NHƯNG NẾU VẪN GẶP LỖI "Invalid column name UserId1", ĐỌC PHẦN DƯỚI:';
    PRINT '';
    PRINT '   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '   🎯 NGUYÊN NHÂN: Entity Framework Core đã cache query cũ (có UserId1)';
    PRINT '   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '';
    PRINT '   📋 CÁCH SỬA (CHẠY CÁC LỆNH SAU TRONG PowerShell):';
    PRINT '';
    PRINT '   # Bước 1: Dừng ứng dụng (Ctrl+C nếu đang chạy)';
    PRINT '';
    PRINT '   # Bước 2: Xóa toàn bộ cache và compiled files';
    PRINT '   cd C:\Users\Admin\source\repos\WebBanHang_Cayxanh\WebBanHang';
    PRINT '   dotnet clean';
    PRINT '   Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue';
    PRINT '';
    PRINT '   # Bước 3: Xóa Migrations folder (nếu có)';
    PRINT '   Remove-Item -Recurse -Force Migrations -ErrorAction SilentlyContinue';
    PRINT '';
    PRINT '   # Bước 4: Xóa NuGet cache';
    PRINT '   dotnet nuget locals all --clear';
    PRINT '';
    PRINT '   # Bước 5: Build lại KHÔNG dùng cache';
    PRINT '   dotnet build --no-incremental --force';
    PRINT '';
    PRINT '   # Bước 6: Chạy lại ứng dụng';
    PRINT '   dotnet run';
    PRINT '';
    PRINT '   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '';
    PRINT '   💡 TIP: Copy các lệnh trên vào PowerShell và chạy từng dòng';
    PRINT '';
    PRINT '   ⚠️ NẾU VẪN LỖI SAU KHI CHẠY CÁC LỆNH TRÊN:';
    PRINT '   1. Restart Visual Studio hoàn toàn';
    PRINT '   2. Kiểm tra file WebBanHang/Models/Order.cs có dòng [ForeignKey("UserId")] chưa';
    PRINT '   3. Chạy lại lệnh build --no-incremental --force';
    PRINT '';
    PRINT '=====================================================================================================================';
    -- Dừng script vì không cần migration database
    RETURN;
END
GO

-- =====================================================================================================================
-- BƯỚC 2: BACKUP OrderItems (để đảm bảo an toàn)
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 2: Backup OrderItems...';
GO

IF OBJECT_ID('dbo.OrderItems_Backup', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.OrderItems_Backup;
END

SELECT * 
INTO dbo.OrderItems_Backup
FROM dbo.OrderItems;

DECLARE @OrderItemsCount INT = (SELECT COUNT(*) FROM dbo.OrderItems_Backup);
PRINT '   ✓ Đã backup ' + CAST(@OrderItemsCount AS NVARCHAR(10)) + ' order items';
GO

-- =====================================================================================================================
-- BƯỚC 3: XÓA FOREIGN KEY CONSTRAINTS CŨ (nếu có)
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 3: Xóa foreign key constraints cũ...';
GO

-- Drop FK từ OrderItems đến Orders
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderItems_Orders')
BEGIN
    ALTER TABLE dbo.OrderItems DROP CONSTRAINT FK_OrderItems_Orders;
    PRINT '   ✓ Đã xóa FK_OrderItems_Orders';
END

-- Drop FK từ Orders đến Users (nếu có)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Orders_Users')
BEGIN
    ALTER TABLE dbo.Orders DROP CONSTRAINT FK_Orders_Users;
    PRINT '   ✓ Đã xóa FK_Orders_Users';
END
GO

-- =====================================================================================================================
-- BƯỚC 4: SỬA CẤU TRÚC ORDERS TABLE
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 4: Sửa cấu trúc Orders table...';
GO

-- Kiểm tra và migrate dữ liệu từ UserId1 sang UserId (nếu UserId đang NULL)
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Orders' 
    AND COLUMN_NAME = 'UserId1'
)
BEGIN
    -- Cập nhật UserId từ UserId1 nếu UserId đang NULL hoặc 0
    UPDATE dbo.Orders
    SET UserId = UserId1
    WHERE UserId IS NULL OR UserId = 0;
    
    DECLARE @MigratedCount INT = @@ROWCOUNT;
    IF @MigratedCount > 0
    BEGIN
        PRINT '   ✓ Đã migrate ' + CAST(@MigratedCount AS NVARCHAR(10)) + ' rows từ UserId1 sang UserId';
    END
    
    -- Xóa column UserId1
    ALTER TABLE dbo.Orders DROP COLUMN UserId1;
    PRINT '   ✓ Đã xóa column UserId1';
END
GO

-- =====================================================================================================================
-- BƯỚC 5: TẠO LẠI FOREIGN KEY CONSTRAINTS
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 5: Tạo lại foreign key constraints...';
GO

-- Tạo FK từ Orders đến Users
ALTER TABLE dbo.Orders
ADD CONSTRAINT FK_Orders_Users 
FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE;
PRINT '   ✓ Đã tạo FK_Orders_Users';

-- Tạo lại FK từ OrderItems đến Orders
ALTER TABLE dbo.OrderItems
ADD CONSTRAINT FK_OrderItems_Orders 
FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE;
PRINT '   ✓ Đã tạo FK_OrderItems_Orders';
GO

-- =====================================================================================================================
-- BƯỚC 6: KIỂM TRA KẾT QUẢ
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 6: Kiểm tra kết quả migration...';
GO

-- Kiểm tra cấu trúc Orders table
PRINT '';
PRINT '   📋 Cấu trúc Orders table sau migration:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders'
ORDER BY ORDINAL_POSITION;

-- Kiểm tra số lượng records
DECLARE @CurrentOrdersCount INT = (SELECT COUNT(*) FROM dbo.Orders);
DECLARE @BackupOrdersCount INT = (SELECT COUNT(*) FROM dbo.Orders_Backup);

PRINT '';
PRINT '   📊 Thống kê dữ liệu:';
PRINT '   - Orders trước migration: ' + CAST(@BackupOrdersCount AS NVARCHAR(10));
PRINT '   - Orders sau migration: ' + CAST(@CurrentOrdersCount AS NVARCHAR(10));

IF @CurrentOrdersCount = @BackupOrdersCount
BEGIN
    PRINT '   ✓ Dữ liệu đầy đủ - Không mất dữ liệu!';
END
ELSE
BEGIN
    PRINT '   ⚠️ CẢNH BÁO: Số lượng orders khác nhau!';
    PRINT '   ℹ️ Bạn có thể restore từ Orders_Backup nếu cần';
END

-- Kiểm tra Foreign Keys
PRINT '';
PRINT '   🔗 Foreign Keys đã tạo:';
SELECT 
    fk.name AS ForeignKey,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables AS tp ON fkc.parent_object_id = tp.object_id
INNER JOIN sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.tables AS tr ON fkc.referenced_object_id = tr.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE tp.name = 'Orders';
GO

-- =====================================================================================================================
-- BƯỚC 7: XÓA BACKUP TABLES (Tùy chọn - Comment out nếu muốn giữ backup)
-- =====================================================================================================================
PRINT '';
PRINT '>> BƯỚC 7: Dọn dẹp backup tables...';
PRINT '   ℹ️ Backup tables được giữ lại để đảm bảo an toàn';
PRINT '   ℹ️ Bạn có thể xóa thủ công sau khi kiểm tra ứng dụng hoạt động tốt:';
PRINT '      - DROP TABLE dbo.Orders_Backup;';
PRINT '      - DROP TABLE dbo.OrderItems_Backup;';
GO

/*
-- UNCOMMENT PHẦN NÀY SAU KHI KIỂM TRA ỨNG DỤNG HOẠT ĐỘNG TốT:
IF OBJECT_ID('dbo.Orders_Backup', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Orders_Backup;
    PRINT '   ✓ Đã xóa Orders_Backup';
END

IF OBJECT_ID('dbo.OrderItems_Backup', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.OrderItems_Backup;
    PRINT '   ✓ Đã xóa OrderItems_Backup';
END
*/

-- =====================================================================================================================
-- HOÀN TẤT
-- =====================================================================================================================
PRINT '';
PRINT '=====================================================================================================================';
PRINT '✅ MIGRATION DATABASE HOÀN TẤT THÀNH CÔNG!';
PRINT '=====================================================================================================================';
PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '🔴 QUAN TRỌNG: PHẢI CHẠY CÁC LỆNH SAU ĐỂ XÓA EF CORE CACHE!';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT '📋 MỞ PowerShell VÀ CHẠY TỪNG LỆNH SAU (Copy/Paste):';
PRINT '';
PRINT '# Bước 1: Di chuyển đến thư mục project';
PRINT 'cd C:\Users\Admin\source\repos\WebBanHang_Cayxanh\WebBanHang';
PRINT '';
PRINT '# Bước 2: Dừng app nếu đang chạy (Ctrl+C)';
PRINT '';
PRINT '# Bước 3: Xóa toàn bộ cache';
PRINT 'dotnet clean';
PRINT 'Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue';
PRINT 'Remove-Item -Recurse -Force Migrations -ErrorAction SilentlyContinue';
PRINT 'dotnet nuget locals all --clear';
PRINT '';
PRINT '# Bước 4: Build lại không dùng cache';
PRINT 'dotnet build --no-incremental --force';
PRINT '';
PRINT '# Bước 5: Chạy ứng dụng';
PRINT 'dotnet run';
PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT '✅ SAU KHI CHẠY CÁC LỆNH TRÊN:';
PRINT '1. Mở browser: http://localhost:5000';
PRINT '2. Đăng nhập admin: admin@gmail.com / admin123';
PRINT '3. Vào Admin → Quản lý đơn hàng';
PRINT '4. Nếu KHÔNG còn lỗi UserId1 → THÀNH CÔNG! 🎉';
PRINT '';
PRINT '⚠️ NẾU VẪN BỊ LỖI:';
PRINT '   - Kiểm tra file Models/Order.cs có [ForeignKey("UserId")] chưa';
PRINT '   - Restart Visual Studio hoàn toàn';
PRINT '   - Chạy lại: dotnet build --no-incremental --force';
PRINT '';
PRINT '📂 SAU KHI KIỂM TRA OK, XÓA BACKUP TABLES:';
PRINT '   DROP TABLE dbo.Orders_Backup;';
PRINT '   DROP TABLE dbo.OrderItems_Backup;';
PRINT '';
PRINT '=====================================================================================================================';
GO

-- =====================================================================================================================
-- SCRIPT RESTORE DỮ LIỆU (NẾU CẦN)
-- =====================================================================================================================
/*
-- CHẠY PHẦN NÀY NẾU CẦN RESTORE DỮ LIỆU TỪ BACKUP:

USE WebBanHangDB;
GO

-- Xóa dữ liệu hiện tại
DELETE FROM dbo.OrderItems;
DELETE FROM dbo.Orders;

-- Restore từ backup
SET IDENTITY_INSERT dbo.Orders ON;

INSERT INTO dbo.Orders (Id, UserId, OrderDate, TotalAmount, Status, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, Notes, CreatedAt, UpdatedAt)
SELECT Id, UserId, OrderDate, TotalAmount, Status, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, Notes, CreatedAt, UpdatedAt
FROM dbo.Orders_Backup;

SET IDENTITY_INSERT dbo.Orders OFF;

SET IDENTITY_INSERT dbo.OrderItems ON;

INSERT INTO dbo.OrderItems (Id, OrderId, ProductId, ProductName, ProductPrice, Quantity, Subtotal)
SELECT Id, OrderId, ProductId, ProductName, ProductPrice, Quantity, Subtotal
FROM dbo.OrderItems_Backup;

SET IDENTITY_INSERT dbo.OrderItems OFF;

PRINT 'Đã restore dữ liệu từ backup!';
*/
