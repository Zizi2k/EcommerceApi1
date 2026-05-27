using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications([FromQuery] int limit = 40)
        {
            var userId = GetUserId();
            if (limit < 1) limit = 1;
            if (limit > 100) limit = 100;

            var list = await _context.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .Take(limit)
                .Select(n => Map(n))
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<NotificationUnreadDto>> GetUnreadCount()
        {
            var userId = GetUserId();
            var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return Ok(new NotificationUnreadDto { Count = count });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetUserId();
            var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return NotFound(new { message = "Không tìm thấy thông báo." });

            if (!n.IsRead)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã đánh dấu đã đọc.", id });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            if (unread.Count > 0)
                await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu tất cả đã đọc.", count = unread.Count });
        }

        private static NotificationDto Map(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            LinkUrl = n.LinkUrl,
            RelatedOrderId = n.RelatedOrderId,
            IsRead = n.IsRead,
            CreatedAtUtc = n.CreatedAtUtc
        };

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
