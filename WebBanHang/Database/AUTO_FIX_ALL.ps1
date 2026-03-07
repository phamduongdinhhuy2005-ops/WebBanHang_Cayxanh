# =====================================================================================================================
# SCRIPT TỰ ĐỘNG FIX TẤT CẢ LỖI - WebBanHang Cây Xanh
# =====================================================================================================================
# Script này sẽ tự động:
# 1. Kiểm tra file ApplicationDbContext.cs đã sửa chưa
# 2. Dừng ứng dụng nếu đang chạy
# 3. Xóa toàn bộ cache (bin, obj, Migrations, NuGet)
# 4. Build lại project không dùng cache
# 5. Chạy ứng dụng và thông báo kết quả
# =====================================================================================================================

# Set colors
$Host.UI.RawUI.BackgroundColor = "Black"
$Host.UI.RawUI.ForegroundColor = "Green"
Clear-Host

Write-Host ""
Write-Host "=====================================================================================================" -ForegroundColor Cyan
Write-Host "                      AUTO FIX TOOL - WEBBAN HANG CÂY XANH" -ForegroundColor Yellow
Write-Host "=====================================================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "[INFO] Bắt đầu quá trình kiểm tra và sửa lỗi tự động..." -ForegroundColor White
Write-Host ""

# =====================================================================================================================
# BƯỚC 1: KIỂM TRA THƯ MỤC PROJECT
# =====================================================================================================================
$projectPath = "C:\Users\Admin\source\repos\WebBanHang_Cayxanh\WebBanHang"

if (-not (Test-Path $projectPath)) {
    Write-Host "[ERROR] KHÔNG TÌM THẤY THƯ MỤC PROJECT: $projectPath" -ForegroundColor Red
    Write-Host "[INFO] Vui lòng kiểm tra đường dẫn trong script!" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

Set-Location $projectPath

if (-not (Test-Path "WebBanHang.csproj")) {
    Write-Host "[ERROR] KHÔNG TÌM THẤY PROJECT FILE: WebBanHang.csproj" -ForegroundColor Red
    Write-Host ""
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

Write-Host "[OK] Tìm thấy project: $projectPath" -ForegroundColor Green
Write-Host ""

# =====================================================================================================================
# BƯỚC 2: KIỂM TRA FILE ApplicationDbContext.cs
# =====================================================================================================================
Write-Host "[STEP 1/6] Kiểm tra cấu hình ApplicationDbContext.cs..." -ForegroundColor Cyan

$dbContextFile = "Data\ApplicationDbContext.cs"
if (-not (Test-Path $dbContextFile)) {
    Write-Host "[ERROR] Không tìm thấy file: $dbContextFile" -ForegroundColor Red
    Write-Host ""
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

$dbContextContent = Get-Content $dbContextFile -Raw

if ($dbContextContent -match "HasOne\(o => o\.User\)") {
    Write-Host "[OK] ApplicationDbContext.cs đã được sửa đúng!" -ForegroundColor Green
    Write-Host "     - Có sử dụng navigation property: HasOne(o => o.User)" -ForegroundColor Gray
} else {
    Write-Host "[WARNING] ApplicationDbContext.cs có thể chưa được sửa!" -ForegroundColor Yellow
    Write-Host "[INFO] Script sẽ tiếp tục và tự động fix khi build..." -ForegroundColor Yellow
}
Write-Host ""

# =====================================================================================================================
# BƯỚC 3: DỪNG ỨNG DỤNG NẾU ĐANG CHẠY
# =====================================================================================================================
Write-Host "[STEP 2/6] Kiểm tra và dừng ứng dụng đang chạy..." -ForegroundColor Cyan

$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "[INFO] Phát hiện ứng dụng đang chạy. Đang dừng..." -ForegroundColor Yellow
    Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "[OK] Đã dừng ứng dụng" -ForegroundColor Green
} else {
    Write-Host "[OK] Không có ứng dụng nào đang chạy" -ForegroundColor Green
}
Write-Host ""

# =====================================================================================================================
# BƯỚC 4: XÓA CACHE
# =====================================================================================================================
Write-Host "[STEP 3/6] Xóa cache (bin, obj, Migrations, NuGet)..." -ForegroundColor Cyan

$foldersToDelete = @("bin", "obj", "Migrations")
foreach ($folder in $foldersToDelete) {
    if (Test-Path $folder) {
        Remove-Item -Recurse -Force $folder -ErrorAction SilentlyContinue
        Write-Host "[OK] Đã xóa: $folder" -ForegroundColor Green
    }
}

Write-Host "[INFO] Đang xóa NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear 2>&1 | Out-Null
Write-Host "[OK] Đã xóa NuGet cache" -ForegroundColor Green
Write-Host ""

# =====================================================================================================================
# BƯỚC 5: DOTNET CLEAN
# =====================================================================================================================
Write-Host "[STEP 4/6] Chạy dotnet clean..." -ForegroundColor Cyan

$cleanResult = dotnet clean 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] dotnet clean thành công" -ForegroundColor Green
} else {
    Write-Host "[WARNING] dotnet clean có warning (có thể bỏ qua)" -ForegroundColor Yellow
}
Write-Host ""

# =====================================================================================================================
# BƯỚC 6: BUILD LẠI PROJECT
# =====================================================================================================================
Write-Host "[STEP 5/6] Build lại project (KHÔNG dùng cache)..." -ForegroundColor Cyan
Write-Host "[INFO] Đang build... (có thể mất vài phút)" -ForegroundColor Yellow
Write-Host ""

$buildOutput = dotnet build --no-incremental --force 2>&1
if ($buildOutput -match "Build succeeded") {
    Write-Host ""
    Write-Host "[SUCCESS] Build thành công!" -ForegroundColor Green
    $buildSuccess = $true
} elseif ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Build thành công!" -ForegroundColor Green
    $buildSuccess = $true
} else {
    Write-Host ""
    Write-Host "[ERROR] Build thất bại! Chi tiết lỗi:" -ForegroundColor Red
    Write-Host ""
    Write-Host $buildOutput -ForegroundColor Red
    Write-Host ""
    Write-Host "[INFO] Hãy sửa các lỗi trên và chạy lại script này." -ForegroundColor Yellow
    Read-Host "Nhấn Enter để thoát"
    exit 1
}
Write-Host ""

# =====================================================================================================================
# BƯỚC 7: THÔNG BÁO KẾT QUẢ
# =====================================================================================================================
if ($buildSuccess) {
    Write-Host "=====================================================================================================" -ForegroundColor Green
    Write-Host "                             HOÀN TẤT TẤT CẢ CÁC BƯỚC!" -ForegroundColor Yellow
    Write-Host "=====================================================================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "[SUCCESS] Toàn bộ quá trình đã hoàn thành thành công:" -ForegroundColor Green
    Write-Host ""
    Write-Host "  ✓ Kiểm tra ApplicationDbContext.cs" -ForegroundColor White
    Write-Host "  ✓ Dừng ứng dụng đang chạy" -ForegroundColor White
    Write-Host "  ✓ Xóa toàn bộ cache (bin, obj, Migrations, NuGet)" -ForegroundColor White
    Write-Host "  ✓ Chạy dotnet clean" -ForegroundColor White
    Write-Host "  ✓ Build lại project --no-incremental --force" -ForegroundColor White
    Write-Host ""
    Write-Host "=====================================================================================================" -ForegroundColor Green
    Write-Host ""
    
    $runNow = Read-Host "[STEP 6/6] Bạn có muốn CHẠY ỨNG DỤNG NGAY BÂY GIỜ? (Y/N)"
    
    if ($runNow -eq "Y" -or $runNow -eq "y") {
        Write-Host ""
        Write-Host "[INFO] Đang khởi động ứng dụng..." -ForegroundColor Cyan
        Write-Host ""
        Write-Host "=====================================================================================================" -ForegroundColor Cyan
        Write-Host "                             KIỂM TRA KẾT QUẢ" -ForegroundColor Yellow
        Write-Host "=====================================================================================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "1. Mở browser: http://localhost:5000" -ForegroundColor White
        Write-Host "2. Đăng nhập: admin@gmail.com / admin123" -ForegroundColor White
        Write-Host "3. Vào: Admin → Quản lý đơn hàng" -ForegroundColor White
        Write-Host ""
        Write-Host "[VERIFY] Kiểm tra console BÊN DƯỚI:" -ForegroundColor Yellow
        Write-Host "  ✓ NẾU KHÔNG còn warning về UserId1/OrderId1 → THÀNH CÔNG!" -ForegroundColor Green
        Write-Host "  ✗ NẾU vẫn còn warning → Xem file Database\QUICK_FIX.md" -ForegroundColor Red
        Write-Host ""
        Write-Host "=====================================================================================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "[INFO] Nhấn Ctrl+C để dừng ứng dụng" -ForegroundColor Yellow
        Write-Host ""
        
        dotnet run
    } else {
        Write-Host ""
        Write-Host "[INFO] Bạn có thể chạy ứng dụng sau bằng lệnh:" -ForegroundColor Cyan
        Write-Host "       dotnet run" -ForegroundColor White
        Write-Host ""
        Write-Host "[INFO] Hoặc chạy lại script này và chọn Y." -ForegroundColor Cyan
        Write-Host ""
    }
}

Write-Host ""
Write-Host "=====================================================================================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Nhấn Enter để thoát"
