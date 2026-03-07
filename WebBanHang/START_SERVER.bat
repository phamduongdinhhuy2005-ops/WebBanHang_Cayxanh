@echo off
chcp 65001 >nul
REM =========================================
REM BATCH FILE TU DONG CHAY SERVER ASP.NET
REM Chi can double-click file nay de chay
REM =========================================

echo.
echo ====================================
echo   KHOI DONG SERVER WEBBANHANG
echo ====================================
echo.

REM Chuyen den thu muc du an
cd /d "%~dp0"

REM Kiem tra xem da cai .NET SDK chua
echo [1/3] Kiem tra .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo.
    echo [LOI] Khong tim thay .NET SDK!
    echo Vui long cai dat .NET 8 SDK tu: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)
echo [OK] Da cai dat .NET SDK

REM Restore cac dependencies
echo.
echo [2/3] Restore dependencies...
dotnet restore
if errorlevel 1 (
    echo.
    echo [LOI] Khong the restore dependencies!
    echo.
    pause
    exit /b 1
)
echo [OK] Restore thanh cong

REM Build du an
echo.
echo [3/3] Build du an...
dotnet build
if errorlevel 1 (
    echo.
    echo [LOI] Build that bai!
    echo.
    pause
    exit /b 1
)
echo [OK] Build thanh cong

REM Chay du an
echo.
echo ====================================
echo   SERVER DANG CHAY...
echo ====================================
echo.
echo Trang chu: http://localhost:5132
echo Nhan Ctrl+C de dung server.
echo.

dotnet run --launch-profile http

REM Neu co loi
if errorlevel 1 (
    echo.
    echo [LOI] Khong the chay server!
    echo.
    pause
)
