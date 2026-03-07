using Microsoft.AspNetCore.Mvc;
using WebBanHang.Services;
using WebBanHang.Models;
using System.Linq;
using System.Collections.Generic;

namespace WebBanHang.Controllers.API
{
    /// <summary>
    /// RESTful API Controller cho quản lý sản phẩm
    /// Endpoint: /api/products
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        /// <summary>
        /// Constructor với Dependency Injection
        /// </summary>
        public ProductsController(IProductService productService, ILogger<ProductsController> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _productService = productService;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm
        /// GET: api/Products
        /// GET: api/Products?category=laptop (lọc theo danh mục)
        /// GET: api/Products?search=asus (tìm kiếm)
        /// </summary>
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? category = null, [FromQuery] string? search = null,
            [FromQuery] int page = 0, [FromQuery] int pageSize = 0, [FromQuery] string? sort = null)
        {
            try
            {
                // Lấy toàn bộ danh sách (service trả list cơ bản)
                var items = _productService.GetAllProducts().AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchTerm = search.ToLower();
                    items = items.Where(p => (p.Name ?? string.Empty).ToLower().Contains(searchTerm)
                        || (p.Description ?? string.Empty).ToLower().Contains(searchTerm));
                }

                // Lọc theo danh mục
                if (!string.IsNullOrWhiteSpace(category) && category.ToLower() != "all")
                {
                    var cat = category.Trim().ToLower();

                    // Special case: khuyenmai (promotions)
                    if (cat == "khuyenmai")
                    {
                        items = items.Where(p =>
                            // Either category explicitly contains 'khuyenmai' (comma/semicolon separated)
                            ((p.Category ?? string.Empty).Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                                .Any(c => c.Trim().Equals("khuyenmai", System.StringComparison.OrdinalIgnoreCase)))
                            // Or product has a real price reduction (OriginalPrice > Price) and Discount > 0
                            || (ParsePrice(p.OriginalPrice) > ParsePrice(p.Price) && (p.Discount ?? 0) > 0)
                        );
                    }
                    else
                    {
                        // Match any of the comma/semicolon separated categories
                        items = items.Where(p => (p.Category ?? string.Empty)
                            .Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                            .Any(c => c.Trim().Equals(cat, System.StringComparison.OrdinalIgnoreCase)));
                    }
                }

                // Sắp xếp đơn giản
                if (!string.IsNullOrWhiteSpace(sort))
                {
                    switch (sort.ToLower())
                    {
                        case "price_asc":
                            items = items.OrderBy(p => ParsePrice(p.Price));
                            break;
                        case "price_desc":
                            items = items.OrderByDescending(p => ParsePrice(p.Price));
                            break;
                        case "name_asc":
                            items = items.OrderBy(p => p.Name);
                            break;
                        case "name_desc":
                            items = items.OrderByDescending(p => p.Name);
                            break;
                        case "newest":
                            items = items.OrderByDescending(p => p.CreatedAt);
                            break;
                        default:
                            // Default: sort by CreatedAt descending
                            items = items.OrderByDescending(p => p.CreatedAt);
                            break;
                    }
                }
                else
                {
                    // Default sort when no sort parameter
                    items = items.OrderByDescending(p => p.CreatedAt);
                }

                var total = items.Count();

                // Nếu pageSize > 0 thì áp dụng phân trang
                List<Product> resultList;
                if (pageSize > 0 && page > 0)
                {
                    var skip = (page - 1) * pageSize;
                    resultList = items.Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    resultList = items.ToList();
                }

                // Map to a lightweight shape expected by frontend JS. The JS expects
                // fields like id, name, desc, image, price, originalPrice, discount, category.
                var payload = resultList.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    desc = p.Description,
                    image = p.ImageUrl,
                    price = p.Price,
                    originalPrice = p.OriginalPrice,
                    discount = p.Discount,
                    stock = p.StockQuantity,
                    category = p.Category,
                    specifications = p.Specifications,
                    longDescription = p.LongDescription
                }).ToList();

                return Ok(new { items = payload, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                return StatusCode(500, "Lỗi server nội bộ");
            }
        }

        private static int ParsePrice(string? price)
        {
            if (string.IsNullOrEmpty(price)) return 0;
            var digits = new string(price.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var v)) return v;
            return 0;
        }

        /// <summary>
        /// <summary>
        /// Lấy chi tiết một sản phẩm theo ID
        /// GET: api/Products/5
        /// </summary>
        /// <param name="id">ID sản phẩm</param>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Không tìm thấy sản phẩm với ID {id}" });
                }

                // Map to the same lightweight shape used by the list endpoint
                var dto = new
                {
                    id = product.Id,
                    name = product.Name,
                    desc = product.Description,
                    image = product.ImageUrl,
                    price = product.Price,
                    originalPrice = product.OriginalPrice,
                    discount = product.Discount,
                    stock = product.StockQuantity,
                    category = product.Category,
                    specifications = product.Specifications,
                    longDescription = product.LongDescription
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm {ProductId}", id);
                return StatusCode(500, "Lỗi server nội bộ");
            }
        }
    }
}


