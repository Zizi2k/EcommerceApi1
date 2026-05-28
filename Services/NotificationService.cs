using EcommerceApi.Configuration;
using EcommerceApi.Data;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly AdminSettings _adminSettings;

        public NotificationService(ApplicationDbContext db, IOptions<AdminSettings> adminSettings)
        {
            _db = db;
            _adminSettings = adminSettings.Value;
        }

        public async Task NotifyUserAsync(int userId, string title, string message, string type, int? orderId = null, string? linkUrl = null)
        {
            if (userId <= 0) return;
            await AddAsync(userId, title, message, type, orderId, linkUrl);
        }

        public async Task NotifyAllAdminsAsync(string title, string message, string type, int? orderId = null, string? linkUrl = null, int? excludeUserId = null)
        {
            var adminIds = await GetAdminRecipientIdsAsync();
            foreach (var id in adminIds)
            {
                if (excludeUserId.HasValue && id == excludeUserId.Value) continue;
                await AddAsync(id, title, message, type, orderId, linkUrl);
            }
        }

        private async Task AddAsync(int userId, string title, string message, string type, int? orderId, string? linkUrl)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title.Trim(),
                Message = message.Trim(),
                Type = type,
                RelatedOrderId = orderId,
                LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? null : linkUrl.Trim(),
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        private async Task<List<int>> GetAdminRecipientIdsAsync()
        {
            var ids = new HashSet<int> { 1 };

            var roleAdmins = await _db.Users.AsNoTracking()
                .Where(u => u.Role == "Admin")
                .Select(u => u.Id)
                .ToListAsync();
            foreach (var id in roleAdmins) ids.Add(id);

            var emails = _adminSettings.Emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (emails.Count > 0)
            {
                var emailAdmins = await _db.Users.AsNoTracking()
                    .Where(u => emails.Contains(u.Email.ToLower()))
                    .Select(u => u.Id)
                    .ToListAsync();
                foreach (var id in emailAdmins) ids.Add(id);
            }

            return ids.ToList();
        }
    }
}
