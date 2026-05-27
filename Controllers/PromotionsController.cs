using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Slide khuyến mãi hiển thị trên trang chủ (chỉ bản ghi đang bật).</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetActivePromotions()
        {
            var list = await QueryWithProduct()
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.Id)
                .Select(p => MapToSlide(p))
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllPromotions()
        {
            var list = await QueryWithProduct()
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.Id)
                .Select(p => MapToSlide(p))
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreatePromotion(PromotionalProductCreateDto dto)
        {
            var err = await ValidateDto(dto.ProductId);
            if (err != null) return BadRequest(new { message = err });

            var entity = new PromotionalProduct
            {
                ProductId = dto.ProductId,
                Headline = TrimOrNull(dto.Headline),
                Subtitle = TrimOrNull(dto.Subtitle),
                BadgeText = TrimOrNull(dto.BadgeText) ?? "KHUYẾN MÃI",
                PromoPrice = dto.PromoPrice,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.PromotionalProducts.Add(entity);
            await _context.SaveChangesAsync();

            var created = await QueryWithProduct()
                .Where(p => p.Id == entity.Id)
                .Select(p => MapToSlide(p))
                .FirstAsync();

            return CreatedAtAction(nameof(GetActivePromotions), new { id = entity.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePromotion(int id, PromotionalProductUpdateDto dto)
        {
            var entity = await _context.PromotionalProducts.FindAsync(id);
            if (entity == null) return NotFound();

            var err = await ValidateDto(dto.ProductId, id);
            if (err != null) return BadRequest(new { message = err });

            entity.ProductId = dto.ProductId;
            entity.Headline = TrimOrNull(dto.Headline);
            entity.Subtitle = TrimOrNull(dto.Subtitle);
            entity.BadgeText = TrimOrNull(dto.BadgeText) ?? "KHUYẾN MÃI";
            entity.PromoPrice = dto.PromoPrice;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var entity = await _context.PromotionalProducts.FindAsync(id);
            if (entity == null) return NotFound();

            _context.PromotionalProducts.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private IQueryable<PromotionalProduct> QueryWithProduct() =>
            _context.PromotionalProducts
                .AsNoTracking()
                .Include(p => p.Product!)
                .ThenInclude(pr => pr.Category);

        private static object MapToSlide(PromotionalProduct p)
        {
            var product = p.Product;
            var displayPrice = p.PromoPrice ?? product?.Price ?? 0;
            var originalPrice = product?.Price ?? 0;

            return new
            {
                p.Id,
                p.ProductId,
                p.Headline,
                p.Subtitle,
                p.BadgeText,
                p.PromoPrice,
                p.SortOrder,
                p.IsActive,
                productName = product?.Name ?? "",
                description = product?.Description ?? "",
                imageUrl = product?.ImageUrl ?? "",
                price = originalPrice,
                displayPrice,
                categoryId = product?.CategoryId,
                categoryName = product?.Category != null ? product.Category.Name : ""
            };
        }

        private async Task<string?> ValidateDto(int productId, int? excludePromoId = null)
        {
            if (productId < 1)
                return "Chọn sản phẩm hợp lệ.";

            if (!await _context.Products.AnyAsync(p => p.Id == productId))
                return "Sản phẩm không tồn tại.";

            var duplicate = await _context.PromotionalProducts
                .AnyAsync(p => p.ProductId == productId && (!excludePromoId.HasValue || p.Id != excludePromoId.Value));

            if (duplicate)
                return "Sản phẩm này đã có trong danh sách khuyến mãi.";

            return null;
        }

        private static string? TrimOrNull(string? value)
        {
            var t = value?.Trim();
            return string.IsNullOrEmpty(t) ? null : t;
        }
    }
}
