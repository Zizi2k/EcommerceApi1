namespace EcommerceApi.Models
{
    public static class NotificationTypes
    {
        public const string OrderPlaced = "OrderPlaced";
        public const string OrderCancelRequested = "OrderCancelRequested";
        public const string OrderCancelAccepted = "OrderCancelAccepted";
        public const string OrderCancelRejected = "OrderCancelRejected";
        public const string OrderStatusUpdated = "OrderStatusUpdated";
        public const string OrderReviewFromCustomer = "OrderReviewFromCustomer";
    }
}
