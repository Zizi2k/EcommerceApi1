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
            var flashType = NormalizeFlashSaleType(dto.FlashSaleType);
            var dailyRange = ParseDailyRange(dto.DailyStartTime, dto.DailyEndTime);
            if (dto.IsFlashSale && flashType == "DailySlot" && dailyRange == null)
                return BadRequest(new { message = "Khung giờ hằng ngày không hợp lệ (định dạng HH:mm)." });
            if (dto.IsFlashSale && flashType == "Event" && dto.EventStartUtc.HasValue && dto.EventEndUtc.HasValue &&
                dto.EventEndUtc <= dto.EventStartUtc)
                return BadRequest(new { message = "Kết thúc sự kiện phải sau thời gian bắt đầu." });

            var entity = new PromotionalProduct
            {
                ProductId = dto.ProductId,
                Headline = TrimOrNull(dto.Headline),
                Subtitle = TrimOrNull(dto.Subtitle),
                BadgeText = TrimOrNull(dto.BadgeText) ?? "KHUYẾN MÃI",
                PromoPrice = dto.PromoPrice,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                IsFlashSale = dto.IsFlashSale,
                FlashSaleType = dto.IsFlashSale ? flashType : "None",
                DailySlotKey = dto.IsFlashSale && flashType == "DailySlot" ? "CUSTOM" : null,
                DailyStartMinute = dto.IsFlashSale && flashType == "DailySlot" ? dailyRange?.Start : null,
                DailyEndMinute = dto.IsFlashSale && flashType == "DailySlot" ? dailyRange?.End : null,
                EventStartUtc = dto.IsFlashSale && flashType == "Event" ? dto.EventStartUtc : null,
                EventEndUtc = dto.IsFlashSale && flashType == "Event" ? dto.EventEndUtc : null,
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
            var flashType = NormalizeFlashSaleType(dto.FlashSaleType);
            var dailyRange = ParseDailyRange(dto.DailyStartTime, dto.DailyEndTime);
            if (dto.IsFlashSale && flashType == "DailySlot" && dailyRange == null)
                return BadRequest(new { message = "Khung giờ hằng ngày không hợp lệ (định dạng HH:mm)." });
            if (dto.IsFlashSale && flashType == "Event" && dto.EventStartUtc.HasValue && dto.EventEndUtc.HasValue &&
                dto.EventEndUtc <= dto.EventStartUtc)
                return BadRequest(new { message = "Kết thúc sự kiện phải sau thời gian bắt đầu." });

            entity.ProductId = dto.ProductId;
            entity.Headline = TrimOrNull(dto.Headline);
            entity.Subtitle = TrimOrNull(dto.Subtitle);
            entity.BadgeText = TrimOrNull(dto.BadgeText) ?? "KHUYẾN MÃI";
            entity.PromoPrice = dto.PromoPrice;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.IsFlashSale = dto.IsFlashSale;
            entity.FlashSaleType = dto.IsFlashSale ? flashType : "None";
            entity.DailySlotKey = dto.IsFlashSale && flashType == "DailySlot" ? "CUSTOM" : null;
            entity.DailyStartMinute = dto.IsFlashSale && flashType == "DailySlot" ? dailyRange?.Start : null;
            entity.DailyEndMinute = dto.IsFlashSale && flashType == "DailySlot" ? dailyRange?.End : null;
            entity.EventStartUtc = dto.IsFlashSale && flashType == "Event" ? dto.EventStartUtc : null;
            entity.EventEndUtc = dto.IsFlashSale && flashType == "Event" ? dto.EventEndUtc : null;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("flash-sale")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFlashSale()
        {
            var now = DateTime.UtcNow;
            var nowLocal = ToVietnamTime(now);
            var currentMinute = nowLocal.Hour * 60 + nowLocal.Minute;

            var list = await QueryWithProduct()
                .Where(p => p.IsActive && p.IsFlashSale)
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            var eventPromos = list
                .Where(p => string.Equals(p.FlashSaleType, "Event", StringComparison.OrdinalIgnoreCase))
                .Where(p => (!p.EventStartUtc.HasValue || p.EventStartUtc <= now) &&
                            (!p.EventEndUtc.HasValue || p.EventEndUtc >= now))
                .ToList();

            var dailyPromos = list
                .Where(p => string.Equals(p.FlashSaleType, "DailySlot", StringComparison.OrdinalIgnoreCase))
                .Where(p => p.DailyStartMinute.HasValue && p.DailyEndMinute.HasValue &&
                            IsMinuteInRange(currentMinute, p.DailyStartMinute.Value, p.DailyEndMinute.Value))
                .ToList();

            return Ok(new
            {
                nowUtc = now,
                nowLocal = nowLocal,
                dailySlot = dailyPromos.Count > 0
                    ? new { label = FormatMinuteRange(dailyPromos[0].DailyStartMinute!.Value, dailyPromos[0].DailyEndMinute!.Value) }
                    : null,
                dailyProducts = dailyPromos.Select(MapToSlide).ToList(),
                eventProducts = eventPromos.Select(MapToSlide).ToList()
            });
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
                p.IsFlashSale,
                p.FlashSaleType,
                p.DailySlotKey,
                p.DailyStartMinute,
                p.DailyEndMinute,
                p.EventStartUtc,
                p.EventEndUtc,
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

        private static string NormalizeFlashSaleType(string? value)
        {
            var t = value?.Trim().ToLowerInvariant();
            return t switch
            {
                "dailyslot" or "daily" => "DailySlot",
                "event" => "Event",
                _ => "None"
            };
        }

        private static (int Start, int End)? ParseDailyRange(string? startText, string? endText)
        {
            if (!TryParseTimeToMinute(startText, out var start) || !TryParseTimeToMinute(endText, out var end))
                return null;
            if (start == end) return null;
            return (start, end);
        }

        private static bool TryParseTimeToMinute(string? value, out int minute)
        {
            minute = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            var s = value.Trim();
            var parts = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var h) || !int.TryParse(parts[1], out var m)) return false;
            if (h < 0 || h > 23 || m < 0 || m > 59) return false;
            minute = h * 60 + m;
            return true;
        }

        private static bool IsMinuteInRange(int nowMinute, int start, int end)
        {
            if (start < end) return nowMinute >= start && nowMinute < end;
            return nowMinute >= start || nowMinute < end;
        }

        private static string FormatMinuteRange(int start, int end)
        {
            string f(int m)
            {
                var hh = (m / 60) % 24;
                var mm = m % 60;
                return hh.ToString("00") + ":" + mm.ToString("00");
            }
            return f(start) + " - " + f(end);
        }

        private static DateTime ToVietnamTime(DateTime utc)
        {
            return utc.AddHours(7);
        }
    }
}
