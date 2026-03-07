using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebBanHang
{
    /// <summary>
    /// Class chính để khởi động ứng dụng ASP.NET Core MVC
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ========== ĐĂNG KÝ SERVICES ==========

            // Đăng ký Controllers và Views (MVC pattern)
            builder.Services.AddControllersWithViews();

            // Đăng ký Session để lưu trạng thái đăng nhập tạm thời
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // đọc connection string từ configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
                ?? "Data Source=.;Initial Catalog=WebBanHangDB;Integrated Security=True;TrustServerCertificate=True;";

            // Đăng ký EF DbContext
            // Register DbContext with SQL Server and enable simple retry on failure to improve
            // connection stability in environments with transient connectivity issues.
            builder.Services.AddDbContext<WebBanHang.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)));

            // Đăng ký ProductService sẽ sử dụng database
            builder.Services.AddScoped<WebBanHang.Services.IProductService, WebBanHang.Services.ProductService>();

            var app = builder.Build();
            // Ensure database exists and create schema if needed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<WebBanHang.Data.ApplicationDbContext>();
                    // Create DB and schema if missing. Try applying EF migrations first; if there
                    // are no migrations present, EnsureCreated will still create schema based on
                    // the current model. This makes local setups and CI environments more robust.
                    try
                    {
                        context.Database.Migrate();
                    }
                    catch
                    {
                        // If migrations are not available or migration fails, fall back to EnsureCreated
                        context.Database.EnsureCreated();
                    }

                    // NOTE: seeding of sample products was removed to avoid accidental
                    // insertion when the developer expects the data to be managed via
                    // SQL Server Management Studio. If you need to seed test data, use
                    // a dedicated seeding script or EF migrations.
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating/creating the database.");
                }
            }

            // Development-time: try to fix file encodings for views that may have been saved
            // with non-UTF8 encodings (helps avoid mojibake in development).
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    void EnsureUtf8(string relativePath)
                    {
                        var full = Path.Combine(Directory.GetCurrentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(full)) return;
                        var bytes = File.ReadAllBytes(full);
                        // Quick BOM check for UTF8
                        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                        {
                            return; // already utf8 with BOM
                        }

                        // Try to decode as UTF8; if roundtrip fails, re-encode from default code page to UTF8
                        try
                        {
                            var s = System.Text.Encoding.UTF8.GetString(bytes);
                            var encoded = System.Text.Encoding.UTF8.GetBytes(s);
                            // if decoding then re-encoding yields same bytes, assume file is already utf8
                            if (!bytes.SequenceEqual(encoded))
                            {
                                // Re-save using UTF8 (no BOM)
                                var text = System.Text.Encoding.Default.GetString(bytes);
                                File.WriteAllText(full, text, new System.Text.UTF8Encoding(false));
                            }
                        }
                        catch
                        {
                            var text = System.Text.Encoding.Default.GetString(bytes);
                            File.WriteAllText(full, text, new System.Text.UTF8Encoding(false));
                        }
                    }

                    // Thử chuẩn hóa tất cả file .cshtml trong thư mục Views để tránh lỗi mã hóa
                    var viewsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Views");
                    if (Directory.Exists(viewsRoot))
                    {
                        var allCshtml = Directory.GetFiles(viewsRoot, "*.cshtml", SearchOption.AllDirectories);
                        foreach (var f in allCshtml)
                        {
                            EnsureUtf8(f);
                        }
                    }
                }
                catch
                {
                    // swallow errors in this best-effort conversion
                }
            }
            // ========== CẤU HÌNH HTTP REQUEST PIPELINE ==========

            // Xử lý lỗi khác nhau giữa môi trường Development và Production
            if (!app.Environment.IsDevelopment())
            {
                // Production: Hiển thị trang lỗi thân thiện
                app.UseExceptionHandler("/Home/Error");

                // Bật HSTS (HTTP Strict Transport Security) để bảo mật
                // Mặc định: 30 ngày
                app.UseHsts();
            }

            // Tự động chuyển hướng HTTP sang HTTPS
            app.UseHttpsRedirection();
            
            // Cho phép sử dụng static files (CSS, JS, images) từ wwwroot
            app.UseStaticFiles();

            // Ensure responses declare UTF-8 charset to avoid mojibake when files
            // were saved with a non-UTF8 encoding. Prefer running the conversion
            // script in Tools/convert-encoding to permanently fix file encoding.
            app.Use(async (context, next) =>
            {
                // Only set for HTML responses; other static assets should retain their content-type
                context.Response.OnStarting(() =>
                {
                    try
                    {
                        var ct = context.Response.ContentType ?? string.Empty;
                        if (string.IsNullOrEmpty(ct) || ct.StartsWith("text/html") || ct.Contains("charset=") == false)
                        {
                            context.Response.ContentType = "text/html; charset=utf-8";
                        }
                    }
                    catch { }
                    return System.Threading.Tasks.Task.CompletedTask;
                });
                await next();
            });

            // Bật routing để map URL tới Controller/Action
            app.UseRouting();

            // Bật Session (phải đặt trước UseAuthorization)
            app.UseSession();

            // Bật Authorization (hiện tại chưa cấu hình)
            app.UseAuthorization();

            // Cấu hình route mặc định: {controller=Home}/{action=Index}/{id?}
            // VD: / -> HomeController.Index()
            // VD: /Product -> ProductController.Index()
            // VD: /Product/Details/5 -> ProductController.Details(5)
            // Map attribute-routed API controllers (e.g., [Route("api/[controller]")])
            app.MapControllers();

            // Default MVC route for controllers with views
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Chạy ứng dụng
            app.Run();
        }
    }
}



