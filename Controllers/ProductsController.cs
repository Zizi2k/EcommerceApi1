using EcommerceApi.Data;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products — phân trang, lọc giá/danh mục, tìm theo tên/mô tả/tên danh mục
        [HttpGet]
        public async Task<ActionResult<object>> GetProducts(
            int page = 1,
            int pageSize = 9,
            string? search = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? categoryId = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                var matchingCategoryIds = await _context.Categories
                    .Where(c => c.Name.ToLower().Contains(s))
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Description.ToLower().Contains(s) ||
                    matchingCategoryIds.Contains(p.CategoryId));
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.ImageUrl,
                    p.CategoryId,
                    categoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();

            return Ok(new
            {
                products,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var dto = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.ImageUrl,
                    p.CategoryId,
                    categoryName = p.Category != null ? p.Category.Name : ""
                })
                .FirstOrDefaultAsync();

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            product.Category = null;
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            product.Category = null;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.Id == id);
    }
}
