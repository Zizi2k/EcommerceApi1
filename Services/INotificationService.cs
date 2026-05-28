namespace EcommerceApi.Services
{
    public interface INotificationService
    {
        Task NotifyUserAsync(int userId, string title, string message, string type, int? orderId = null, string? linkUrl = null);
        Task NotifyAllAdminsAsync(string title, string message, string type, int? orderId = null, string? linkUrl = null, int? excludeUserId = null);
    }
}
