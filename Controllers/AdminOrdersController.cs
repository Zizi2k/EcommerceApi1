using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Route("api/Admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public AdminOrdersController(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminOrderDto>>> GetOrders(
            [FromQuery] string? status,
            [FromQuery] string? q)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAtUtc);

            var orders = await query.ToListAsync();
            var normalizedFilter = string.IsNullOrWhiteSpace(status)
                ? null
                : OrderStatuses.Normalize(status);
            var search = q?.Trim().ToLowerInvariant();

            if (normalizedFilter != null)
            {
                orders = orders
                    .Where(o => OrderStatuses.Normalize(o.Status) == normalizedFilter)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                        o.Id.ToString().Contains(search) ||
                        (o.CustomerName ?? "").ToLowerInvariant().Contains(search) ||
                        (o.CustomerPhone ?? "").ToLowerInvariant().Contains(search) ||
                        (o.AccountUsername ?? "").ToLowerInvariant().Contains(search))
                    .ToList();
            }

            var productIds = orders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();
            var productNames = productIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Products.AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Name);

            var result = orders.Select(o => MapOrder(o, productNames)).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminOrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productNames = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name);

            return Ok(MapOrder(order, productNames));
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new { message = "Thiếu trạng thái." });

            var next = OrderStatuses.Normalize(dto.Status);
            if (!OrderStatuses.IsValid(next))
                return BadRequest(new { message = "Trạng thái không hợp lệ.", allowed = OrderStatuses.All });
            if (next == OrderStatuses.Cancelled)
                return BadRequest(new { message = "Dùng nút Chấp nhận hủy để hủy đơn có yêu cầu từ khách." });

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (OrderStatuses.Normalize(order.Status) == OrderStatuses.Cancelled)
                return BadRequest(new { message = "Đơn đã hủy, không thể đổi trạng thái." });

            var prevLabel = OrderStatuses.GetLabel(order.Status);
            order.Status = next;
            await _context.SaveChangesAsync();

            await _notifications.NotifyUserAsync(
                order.UserId,
                "Cập nhật trạng thái đơn",
                $"Đơn #{order.Id}: {prevLabel} → {OrderStatuses.GetLabel(next)}",
                NotificationTypes.OrderStatusUpdated,
                order.Id,
                "account.html#my-orders-title");

            return Ok(new
            {
                message = "Đã cập nhật trạng thái đơn hàng.",
                orderId = id,
                status = next,
                statusLabel = OrderStatuses.GetLabel(next)
            });
        }

        [HttpPut("{id:int}/review")]
        public async Task<IActionResult> SubmitReview(int id, [FromBody] SubmitOrderReviewDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Thiếu dữ liệu đánh giá." });

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Điểm đánh giá phải từ 1 đến 5 sao." });

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (OrderStatuses.Normalize(order.Status) != OrderStatuses.Delivered)
                return BadRequest(new { message = "Chỉ đánh giá được khi đơn ở trạng thái Đã giao." });

            order.AdminRating = dto.Rating;
            order.AdminReviewNote = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            order.AdminReviewedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã lưu đánh giá đơn hàng.",
                orderId = id,
                adminRating = order.AdminRating,
                adminReviewNote = order.AdminReviewNote,
                adminReviewedAtUtc = order.AdminReviewedAtUtc
            });
        }

        [HttpPost("{id:int}/cancel-request/accept")]
        public async Task<IActionResult> AcceptCancelRequest(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var err = ValidateCancelResponse(order);
            if (err != null) return BadRequest(new { message = err });

            order.Status = OrderStatuses.Cancelled;
            await _context.SaveChangesAsync();

            await _notifications.NotifyUserAsync(
                order.UserId,
                "Yêu cầu hủy được chấp nhận",
                $"Đơn #{order.Id} đã được hủy. Lý do: {order.CancelReason}",
                NotificationTypes.OrderCancelAccepted,
                order.Id,
                "account.html#my-orders-title");

            return Ok(new
            {
                message = "Đã chấp nhận yêu cầu hủy đơn.",
                orderId = id,
                status = order.Status,
                statusLabel = OrderStatuses.GetLabel(order.Status)
            });
        }

        [HttpPost("{id:int}/cancel-request/reject")]
        public async Task<IActionResult> RejectCancelRequest(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var err = ValidateCancelResponse(order);
            if (err != null) return BadRequest(new { message = err });

            var rejectedReason = order.CancelReason;
            order.CancelReason = null;
            order.CancelNote = null;
            order.CancelRequestedAtUtc = null;
            await _context.SaveChangesAsync();

            await _notifications.NotifyUserAsync(
                order.UserId,
                "Yêu cầu hủy bị từ chối",
                $"Đơn #{order.Id} tiếp tục xử lý ({OrderStatuses.GetLabel(order.Status)}). Lý do đã gửi: {rejectedReason}",
                NotificationTypes.OrderCancelRejected,
                order.Id,
                "account.html#my-orders-title");

            return Ok(new
            {
                message = "Đã từ chối yêu cầu hủy. Đơn tiếp tục giao hàng.",
                orderId = id,
                status = order.Status,
                statusLabel = OrderStatuses.GetLabel(order.Status)
            });
        }

        private static string? ValidateCancelResponse(Order order)
        {
            if (string.IsNullOrWhiteSpace(order.CancelReason))
                return "Đơn này không có yêu cầu hủy.";

            var status = OrderStatuses.Normalize(order.Status);
            if (status == OrderStatuses.Delivered)
                return "Đơn đã giao, không thể xử lý yêu cầu hủy.";
            if (status == OrderStatuses.Cancelled)
                return "Đơn đã hủy.";

            return null;
        }

        private static AdminOrderDto MapOrder(Order o, Dictionary<int, string> productNames)
        {
            var status = OrderStatuses.Normalize(o.Status);
            return new AdminOrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                AccountUsername = o.AccountUsername,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                ShippingAddress = o.ShippingAddress,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                Status = status,
                StatusLabel = OrderStatuses.GetLabel(status),
                CreatedAtUtc = o.CreatedAtUtc,
                AdminRating = o.AdminRating,
                AdminReviewNote = o.AdminReviewNote,
                AdminReviewedAtUtc = o.AdminReviewedAtUtc,
                CustomerRating = o.CustomerRating,
                CustomerReviewNote = o.CustomerReviewNote,
                CustomerReviewedAtUtc = o.CustomerReviewedAtUtc,
                CancelReason = o.CancelReason,
                CancelNote = o.CancelNote,
                CancelRequestedAtUtc = o.CancelRequestedAtUtc,
                HasCancelRequest = !string.IsNullOrWhiteSpace(o.CancelReason) &&
                                   status != OrderStatuses.Cancelled,
                CanRespondToCancelRequest = !string.IsNullOrWhiteSpace(o.CancelReason) &&
                                            status != OrderStatuses.Cancelled &&
                                            status != OrderStatuses.Delivered,
                CanReview = status == OrderStatuses.Delivered,
                Items = o.Items.Select(i => new AdminOrderItemDto
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
