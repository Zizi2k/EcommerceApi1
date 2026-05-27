namespace EcommerceApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        /// <summary>Giá vốn trung bình của sản phẩm (dùng cho báo cáo lãi).</summary>
        public decimal CostPrice { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Category? Category { get; set; }
    }
}

