namespace EcommerceApi.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public int? RelatedOrderId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class NotificationUnreadDto
    {
        public int Count { get; set; }
    }
}
