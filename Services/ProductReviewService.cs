using EcommerceApi.Data;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services
{
    public class ProductReviewService
    {
        private readonly ApplicationDbContext _db;

        public ProductReviewService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SyncReviewsForOrderAsync(Order order)
        {
            if (order.CustomerRating is null or < 1 or > 5)
                return;

            var items = await _db.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == order.Id)
                .ToListAsync();
            if (items.Count == 0) return;

            var reviewer = !string.IsNullOrWhiteSpace(order.CustomerName)
                ? order.CustomerName.Trim()
                : (order.AccountUsername ?? "Khách").Trim();

            var note = order.CustomerReviewNote;
            var rating = order.CustomerRating.Value;
            var created = order.CustomerReviewedAtUtc ?? DateTime.UtcNow;

            foreach (var item in items)
            {
                var existing = await _db.ProductReviews
                    .FirstOrDefaultAsync(r => r.OrderId == order.Id && r.ProductId == item.ProductId);

                if (existing == null)
                {
                    _db.ProductReviews.Add(new ProductReview
                    {
                        ProductId = item.ProductId,
                        OrderId = order.Id,
                        UserId = order.UserId,
                        ReviewerName = reviewer,
                        Rating = rating,
                        Note = note,
                        CreatedAtUtc = created
                    });
                }
                else
                {
                    existing.Rating = rating;
                    existing.Note = note;
                    existing.ReviewerName = reviewer;
                    existing.CreatedAtUtc = created;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Dictionary<int, (int Count, double Average)>> GetSummariesAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, (int, double)>();

            var rows = await _db.ProductReviews.AsNoTracking()
                .Where(r => ids.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count(),
                    Average = g.Average(r => (double)r.Rating)
                })
                .ToListAsync();

            return rows.ToDictionary(r => r.ProductId, r => (r.Count, Math.Round(r.Average, 1)));
        }

        /// <summary>Thống kê hiển thị: nếu chưa có đánh giá thật thì dùng số ảo ổn định theo productId.</summary>
        public (int Count, double Average, bool IsPlaceholder) GetDisplayStats(int productId, int realCount, double realAverage)
        {
            if (realCount > 0)
                return (realCount, realAverage, false);

            var seed = unchecked(productId * 7919 + 104729);
            var count = 18 + Math.Abs(seed % 73);
            var avg = 4.2 + (Math.Abs(seed / 97) % 8) * 0.1;
            return (count, Math.Round(avg, 1), true);
        }

        public List<DTOs.ProductReviewDto> GetPlaceholderReviews(int productId, double averageRating, int take = 3)
        {
            if (take < 1) take = 1;
            if (take > 5) take = 5;

            var names = new[] { "Nguyễn V***", "Trần H***", "Lê M***", "Phạm T***", "Hoàng K***" };
            var notes = new[]
            {
                "Sản phẩm đúng mô tả, giao hàng nhanh.",
                "Chất lượng tốt, đóng gói cẩn thận.",
                "Hài lòng, sẽ mua lại.",
                "Giá hợp lý so với thị trường.",
                "Trải nghiệm mua hàng tốt."
            };

            var baseRating = (int)Math.Round(averageRating, MidpointRounding.AwayFromZero);
            baseRating = Math.Clamp(baseRating, 4, 5);
            var list = new List<DTOs.ProductReviewDto>();
            var now = DateTime.UtcNow;

            for (var i = 0; i < take; i++)
            {
                var seed = unchecked(productId * 1009 + i * 9173);
                var rating = Math.Clamp(baseRating + (Math.Abs(seed % 3) - 1), 4, 5);
                var daysAgo = 3 + Math.Abs(seed % 45);
                list.Add(new DTOs.ProductReviewDto
                {
                    Id = -(productId * 10 + i + 1),
                    ProductId = productId,
                    OrderId = 0,
                    ReviewerName = names[Math.Abs(seed) % names.Length],
                    Rating = rating,
                    Note = notes[Math.Abs(seed / 7) % notes.Length],
                    CreatedAtUtc = now.AddDays(-daysAgo)
                });
            }

            return list;
        }

        public async Task<List<DTOs.ProductReviewDto>> GetReviewsForProductAsync(int productId, int limit = 20)
        {
            if (limit < 1) limit = 1;
            if (limit > 50) limit = 50;

            return await _db.ProductReviews.AsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(limit)
                .Select(r => new DTOs.ProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    OrderId = r.OrderId,
                    ReviewerName = r.ReviewerName,
                    Rating = r.Rating,
                    Note = r.Note,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();
        }
    }
}
