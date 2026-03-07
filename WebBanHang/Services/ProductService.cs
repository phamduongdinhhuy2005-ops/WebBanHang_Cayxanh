using WebBanHang.Models;
using WebBanHang.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace WebBanHang.Services
{
    public interface IProductService
    {
        List<Product> GetAllProducts();
        Product? GetProductById(int id);
        List<Product> GetProductsByCategory(string category);
        List<Product> SearchProducts(string searchTerm);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;

        public ProductService(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Product> GetAllProducts()
        {
            return _db.Products.AsNoTracking().ToList();
        }

        public Product? GetProductById(int id)
        {
            return _db.Products.Find(id);
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category) || category.ToLower() == "all")
                return _db.Products.AsNoTracking().ToList();

            // Special handling for 'khuyenmai' (promotions): return products that have a real price reduction
            if (category.Equals("khuyenmai", StringComparison.OrdinalIgnoreCase))
            {
                // Load to memory and parse price strings (Price/OriginalPrice stored as NVARCHAR like '29,990,000 VND')
                var all = _db.Products.AsNoTracking().ToList();
                return all.Where(p =>
                {
                    var orig = ParsePrice(p.OriginalPrice);
                    var price = ParsePrice(p.Price);
                    return orig > price && p.Discount.HasValue && p.Discount > 0;
                }).ToList();
            }

            // Default: exact category match (case-insensitive)
            var products = _db.Products.AsNoTracking().ToList();
            return products.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Parse price strings like '29,990,000 VND' to numeric value (28990000)
        private static long ParsePrice(string? priceString)
        {
            if (string.IsNullOrEmpty(priceString))
                return 0;

            // Keep digits only
            var digits = new string(priceString.Where(char.IsDigit).ToArray());
            if (long.TryParse(digits, out var value))
                return value;
            return 0;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return _db.Products.AsNoTracking().ToList();

            searchTerm = searchTerm.ToLower();
            return _db.Products.AsNoTracking().Where(p => p.Name.ToLower().Contains(searchTerm) || p.Description.ToLower().Contains(searchTerm)).ToList();
        }
    }
}


