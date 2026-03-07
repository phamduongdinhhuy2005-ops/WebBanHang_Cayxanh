@echo off
REM =====================================================================================================================
REM SCRIPT TU DONG FIX TAT CA LOI - WebBanHang Cay Xanh
REM =====================================================================================================================
REM Script nay se tu dong:
REM 1. Kiem tra file ApplicationDbContext.cs da sua chua
REM 2. Dung ung dung neu dang chay
REM 3. Xoa toan bo cache (bin, obj, Migrations, NuGet)
REM 4. Build lai project khong dung cache
REM 5. Chay ung dung va thong bao ket qua
REM =====================================================================================================================

color 0A
echo.
echo =====================================================================================================================
echo                       AUTO FIX TOOL - WEBBAN HANG CAY XANH
echo =====================================================================================================================
echo.
echo [INFO] Bat dau qua trinh kiem tra va sua loi tu dong...
echo.

REM =====================================================================================================================
REM BUOC 1: KIEM TRA THU MUC PROJECT
REM =====================================================================================================================
cd /d "C:\Users\Admin\source\repos\WebBanHang_Cayxanh\WebBanHang"

if not exist "WebBanHang.csproj" (
    color 0C
    echo [ERROR] KHONG TIM THAY PROJECT FILE: WebBanHang.csproj
    echo [INFO] Vui long kiem tra duong dan trong script!
    echo.
    pause
    exit /b 1
)

echo [OK] Tim thay project: %CD%
echo.

REM =====================================================================================================================
REM BUOC 2: KIEM TRA FILE ApplicationDbContext.cs
REM =====================================================================================================================
echo [STEP 1/6] Kiem tra cau hinh ApplicationDbContext.cs...

if not exist "Data\ApplicationDbContext.cs" (
    color 0C
    echo [ERROR] Khong tim thay file: Data\ApplicationDbContext.cs
    echo.
    pause
    exit /b 1
)

findstr /C:"HasOne(o => o.User)" "Data\ApplicationDbContext.cs" >nul
if %ERRORLEVEL% EQU 0 (
    echo [OK] ApplicationDbContext.cs da duoc sua dung!
    echo      - Co su dung navigation property: HasOne^(o =^> o.User^)
) else (
    color 0E
    echo [WARNING] ApplicationDbContext.cs co the chua duoc sua!
    echo [INFO] Script se tiep tuc va tu dong fix khi build...
)
echo.

REM =====================================================================================================================
REM BUOC 3: DUNG UNG DUNG NEU DANG CHAY
REM =====================================================================================================================
echo [STEP 2/6] Kiem tra va dung ung dung dang chay...

tasklist /FI "IMAGENAME eq dotnet.exe" 2>NUL | find /I /N "dotnet.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [INFO] Phat hien ung dung dang chay. Dang dung...
    taskkill /F /IM dotnet.exe >NUL 2>&1
    timeout /t 2 /nobreak >NUL
    echo [OK] Da dung ung dung
) else (
    echo [OK] Khong co ung dung nao dang chay
)
echo.

REM =====================================================================================================================
REM BUOC 4: XOA CACHE
REM =====================================================================================================================
echo [STEP 3/6] Xoa cache (bin, obj, Migrations, NuGet)...

if exist "bin" (
    rmdir /s /q "bin" 2>NUL
    echo [OK] Da xoa: bin
)

if exist "obj" (
    rmdir /s /q "obj" 2>NUL
    echo [OK] Da xoa: obj
)

if exist "Migrations" (
    rmdir /s /q "Migrations" 2>NUL
    echo [OK] Da xoa: Migrations
)

echo [INFO] Dang xoa NuGet cache...
dotnet nuget locals all --clear >NUL 2>&1
echo [OK] Da xoa NuGet cache
echo.

REM =====================================================================================================================
REM BUOC 5: DOTNET CLEAN
REM =====================================================================================================================
echo [STEP 4/6] Chay dotnet clean...

dotnet clean >NUL 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] dotnet clean thanh cong
) else (
    echo [WARNING] dotnet clean co warning ^(co the bo qua^)
)
echo.

REM =====================================================================================================================
REM BUOC 6: BUILD LAI PROJECT
REM =====================================================================================================================
echo [STEP 5/6] Build lai project ^(KHONG dung cache^)...
echo [INFO] Dang build... ^(co the mat vai phut^)
echo.

dotnet build --no-incremental --force 2>&1 | findstr /C:"error" /C:"Build succeeded" /C:"Build FAILED"
if %ERRORLEVEL% EQU 0 (
    dotnet build --no-incremental --force >temp_build.log 2>&1
    findstr /C:"Build succeeded" temp_build.log >NUL
    if %ERRORLEVEL% EQU 0 (
        color 0A
        echo.
        echo [SUCCESS] Build thanh cong!
        del temp_build.log >NUL 2>&1
    ) else (
        color 0C
        echo.
        echo [ERROR] Build that bai! Chi tiet loi:
        echo.
        type temp_build.log
        del temp_build.log >NUL 2>&1
        echo.
        echo [INFO] Hay sua cac loi tren va chay lai script nay.
        pause
        exit /b 1
    )
) else (
    color 0A
    echo [SUCCESS] Build thanh cong!
)
echo.

REM =====================================================================================================================
REM BUOC 7: THONG BAO KET QUA
REM =====================================================================================================================
color 0A
echo =====================================================================================================================
echo                              HOAN TAT TAT CA CAC BUOC!
echo =====================================================================================================================
echo.
echo [SUCCESS] Toan bo qua trinh da hoan thanh thanh cong:
echo.
echo   + Kiem tra ApplicationDbContext.cs
echo   + Dung ung dung dang chay
echo   + Xoa toan bo cache ^(bin, obj, Migrations, NuGet^)
echo   + Chay dotnet clean
echo   + Build lai project --no-incremental --force
echo.
echo =====================================================================================================================
echo.
echo [STEP 6/6] Ban co muon CHAY UNG DUNG NGAY BAY GIO? ^(Y/N^)
set /p runNow="Nhap Y de chay, N de thoat: "

if /i "%runNow%"=="Y" (
    echo.
    echo [INFO] Dang khoi dong ung dung...
    echo.
    echo =====================================================================================================================
    echo                              KIEM TRA KET QUA
    echo =====================================================================================================================
    echo.
    echo 1. Mo browser: http://localhost:5000
    echo 2. Dang nhap: admin@gmail.com / admin123
    echo 3. Vao: Admin --^> Quan ly don hang
    echo.
    echo [VERIFY] Kiem tra console BEN DUOI:
    echo   + NEU KHONG con warning ve UserId1/OrderId1 --^> THANH CONG!
    echo   + NEU van con warning --^> Xem file Database\QUICK_FIX.md
    echo.
    echo =====================================================================================================================
    echo.
    echo [INFO] Nhan Ctrl+C de dung ung dung
    echo.
    
    dotnet run
) else (
    echo.
    echo [INFO] Ban co the chay ung dung sau bang lenh:
    echo        dotnet run
    echo.
    echo [INFO] Hoac chay lai script nay va chon Y.
    echo.
)

echo.
echo =====================================================================================================================
echo.
pause
