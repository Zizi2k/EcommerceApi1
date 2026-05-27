namespace EcommerceApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        /// <summary>Giá tại thời điểm đặt (snapshot).</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>Giá vốn tại thời điểm đặt (snapshot).</summary>
        public decimal UnitCost { get; set; }
    }
}
