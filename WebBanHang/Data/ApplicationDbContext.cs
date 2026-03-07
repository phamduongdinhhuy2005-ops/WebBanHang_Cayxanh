using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<WebBanHang.Models.UserCart> UserCarts { get; set; } = null!;
        public DbSet<UserAccount> Users { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Users
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DisplayName).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Cấu hình bảng Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");

                // Quan hệ với Users
                entity.HasOne<UserAccount>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng OrderItems
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");

                // Quan hệ với Orders
                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}



