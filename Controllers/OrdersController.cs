using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;
        private readonly ProductReviewService _productReviews;

        public OrdersController(ApplicationDbContext context, INotificationService notifications, ProductReviewService productReviews)
        {
            _context = context;
            _notifications = notifications;
            _productReviews = productReviews;
        }

        [HttpGet("cancel-reasons")]
        public ActionResult<IEnumerable<string>> GetCancelReasons() => Ok(OrderCancelReasons.All);

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<UserOrderDto>>> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync();

            var productIds = orders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();
            var productNames = productIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Products.AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Name);

            return Ok(orders.Select(o => Map(o, productNames)).ToList());
        }

        [HttpPut("{id:int}/review")]
        public async Task<IActionResult> SubmitReview(int id, [FromBody] SubmitUserOrderReviewDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Thiếu dữ liệu đánh giá." });
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Điểm đánh giá phải từ 1 đến 5 sao." });

            var userId = GetUserId();
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (OrderStatuses.Normalize(order.Status) != OrderStatuses.Delivered)
                return BadRequest(new { message = "Chỉ đánh giá được khi đơn ở trạng thái Đã giao." });

            if (order.CustomerRating.HasValue)
                return BadRequest(new { message = "Bạn đã đánh giá đơn này rồi." });

            order.CustomerRating = dto.Rating;
            order.CustomerReviewNote = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            order.CustomerReviewedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _productReviews.SyncReviewsForOrderAsync(order);

            await _notifications.NotifyAllAdminsAsync(
                "Khách đánh giá đơn hàng",
                $"Đơn #{order.Id}: {dto.Rating}/5 sao",
                NotificationTypes.OrderReviewFromCustomer,
                order.Id,
                "admin.html#orders-section-title");

            return Ok(new
            {
                message = "Cảm ơn bạn đã đánh giá đơn hàng.",
                orderId = order.Id,
                customerRating = order.CustomerRating,
                customerReviewNote = order.CustomerReviewNote,
                customerReviewedAtUtc = order.CustomerReviewedAtUtc
            });
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> SubmitCancelRequest(int id, [FromBody] SubmitOrderCancelDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Vui lòng chọn lý do hủy đơn." });

            var resolvedReason = OrderCancelReasons.Resolve(dto.Reason);
            if (resolvedReason == null)
                return BadRequest(new { message = "Lý do hủy không hợp lệ.", allowedReasons = OrderCancelReasons.All });

            var userId = GetUserId();
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var status = OrderStatuses.Normalize(order.Status);
            if (status == OrderStatuses.Delivered)
                return BadRequest(new { message = "Đơn đã giao, không thể hủy." });

            if (!string.IsNullOrWhiteSpace(order.CancelReason))
                return BadRequest(new { message = "Đơn này đã gửi yêu cầu hủy trước đó." });

            order.CancelReason = resolvedReason;
            order.CancelNote = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            order.CancelRequestedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notifications.NotifyAllAdminsAsync(
                "Yêu cầu hủy đơn",
                $"Khách yêu cầu hủy đơn #{order.Id}: {resolvedReason}",
                NotificationTypes.OrderCancelRequested,
                order.Id,
                "admin.html#orders-section-title");

            return Ok(new
            {
                message = "Đã gửi yêu cầu hủy đơn. Admin sẽ xử lý sớm.",
                orderId = order.Id,
                cancelReason = order.CancelReason,
                cancelNote = order.CancelNote,
                cancelRequestedAtUtc = order.CancelRequestedAtUtc
            });
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static UserOrderDto Map(Order o, Dictionary<int, string> productNames)
        {
            var status = OrderStatuses.Normalize(o.Status);
            return new UserOrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                Status = status,
                StatusLabel = OrderStatuses.GetLabel(status),
                CreatedAtUtc = o.CreatedAtUtc,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                ShippingAddress = o.ShippingAddress,
                CustomerRating = o.CustomerRating,
                CustomerReviewNote = o.CustomerReviewNote,
                CustomerReviewedAtUtc = o.CustomerReviewedAtUtc,
                CancelReason = o.CancelReason,
                CancelNote = o.CancelNote,
                CancelRequestedAtUtc = o.CancelRequestedAtUtc,
                CanCancel = status != OrderStatuses.Delivered &&
                            status != OrderStatuses.Cancelled &&
                            string.IsNullOrWhiteSpace(o.CancelReason),
                CanReview = status == OrderStatuses.Delivered && !o.CustomerRating.HasValue,
                Items = o.Items.Select(i => new UserOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = productNames.TryGetValue(i.ProductId, out var name) ? name : ("SP #" + i.ProductId),
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }
    }
}
