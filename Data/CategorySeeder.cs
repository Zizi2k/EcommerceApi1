using EcommerceApi.Models;

namespace EcommerceApi.Data
{
    public static class CategorySeeder
    {
        /// <summary>Danh mục cố định id 1–11 (khớp bộ lọc trên trang chủ).</summary>
        public static List<Category> GetCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Laptop & Máy Tính" },
                new Category { Id = 2, Name = "Smartphone" },
                new Category { Id = 3, Name = "Tai Nghe" },
                new Category { Id = 4, Name = "Camera & Photo" },
                new Category { Id = 5, Name = "Phím & Chuột" },
                new Category { Id = 6, Name = "Màn Hình & Display" },
                new Category { Id = 7, Name = "Router & Network" },
                new Category { Id = 8, Name = "Pin & Sạc" },
                new Category { Id = 9, Name = "Máy Chiếu" },
                new Category { Id = 10, Name = "Wearable" },
                new Category { Id = 11, Name = "Phụ Kiện" },
            };
        }
    }
}
