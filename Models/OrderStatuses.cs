namespace EcommerceApi.Models
{
    /// <summary>Trạng thái vận chuyển đơn hàng.</summary>
    public static class OrderStatuses
    {
        public const string Preparing = "Preparing";
        public const string Delivering = "Delivering";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = [Preparing, Delivering, Delivered, Cancelled];

        public static string Normalize(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Preparing;

            var s = status.Trim();
            if (s.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return Delivered;
            if (s.Equals(Preparing, StringComparison.OrdinalIgnoreCase) ||
                s.Equals("DangChuanBi", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Đang chuẩn bị", StringComparison.OrdinalIgnoreCase))
                return Preparing;
            if (s.Equals(Delivering, StringComparison.OrdinalIgnoreCase) ||
                s.Equals("DangGiao", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Đang giao", StringComparison.OrdinalIgnoreCase))
                return Delivering;
            if (s.Equals(Delivered, StringComparison.OrdinalIgnoreCase) ||
                s.Equals("DaGiao", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Đã giao", StringComparison.OrdinalIgnoreCase))
                return Delivered;
            if (s.Equals(Cancelled, StringComparison.OrdinalIgnoreCase) ||
                s.Equals("DaHuy", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Đã hủy", StringComparison.OrdinalIgnoreCase))
                return Cancelled;

            return Preparing;
        }

        public static bool IsValid(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            All.Contains(Normalize(status), StringComparer.Ordinal);

        public static string GetLabel(string? status) => Normalize(status) switch
        {
            Preparing => "Đang chuẩn bị",
            Delivering => "Đang giao",
            Delivered => "Đã giao",
            Cancelled => "Đã hủy",
            _ => "Đang chuẩn bị"
        };
    }
}
