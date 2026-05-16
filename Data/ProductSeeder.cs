using EcommerceApi.Models;
using System.Collections.Generic;

namespace EcommerceApi.Data
{
    public static class ProductSeeder
    {
        public static List<Product> GetProducts()
        {
            return new List<Product>
            {
                // LAPTOP & MÁY TÍNH (1-10)
                new Product { Id = 1001, Name = "Laptop Gaming Raptor X", Description = "Laptop gaming cao cấp với GTX 4080, RAM 32GB, SSD 1TB NVMe. Hiệu năng tuyệt vời để chơi game và làm việc sáng tạo.", Price = 42990000, Stock = 20, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1002, Name = "Laptop Ultrabook Pro", Description = "Thiết kế mỏng nhẹ, weight 1.2kg, pin 15 giờ. Ideal cho lập trình viên và người làm việc mobile.", Price = 28990000, Stock = 15, ImageUrl = "https://images.unsplash.com/photo-1588872657840-790ff3a4bbe0?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1003, Name = "Laptop Creator Studio", Description = "Màn hình OLED 16 inch, RTX 4070, bàn phím backlit. Dành cho video editor, designer, photographer.", Price = 35990000, Stock = 12, ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1004, Name = "Laptop Business Office", Description = "Core i7, RAM 16GB, SSD 512GB. Phù hợp cho công sở, học tập, làm việc online.", Price = 19990000, Stock = 25, ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1005, Name = "Laptop Gaming ROG Beast", Description = "Tần số cao 240Hz, RGB lighting, thiết kế aggressive. Gaming experience tuyệt đỉnh.", Price = 45990000, Stock = 8, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1006, Name = "Laptop Mỏng nhẹ Elite", Description = "Weight 1kg, thickness 14mm, full HD, pin 16h. Dành cho người hay di chuyển.", Price = 21990000, Stock = 18, ImageUrl = "https://images.unsplash.com/photo-1588872657840-790ff3a4bbe0?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1007, Name = "Laptop Workstation Pro", Description = "Xeon processor, RTX 6000, RAM 64GB. Cho công việc CAD, 3D rendering, AI.", Price = 65990000, Stock = 5, ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1008, Name = "Laptop Casual Everyday", Description = "Chip ARM efficiency, 15 giờ pin, price tốt. Học tập, công sở, giải trí.", Price = 11990000, Stock = 40, ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1009, Name = "Laptop 2-in-1 Hybrid", Description = "Màn hình cảm ứng 13 inch, gập được như tablet, pen stylus support.", Price = 18990000, Stock = 14, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&h=600&fit=crop", CategoryId = 1 },
                new Product { Id = 1010, Name = "Desktop Gaming Tower", Description = "RTX 4090, Core i9, RAM 64GB, 4TB SSD. Máy tính để bàn gaming ultimate.", Price = 72990000, Stock = 6, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 1 },

                // SMARTPHONE (11-20)
                new Product { Id = 1011, Name = "Smartphone Flagship Neo", Description = "Màn hình 6.7 inch, camera 108MP, pin 5000mAh, 5G, Snapdragon 8 Gen 3.", Price = 24990000, Stock = 30, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1012, Name = "Smartphone Compact Pro", Description = "Màn hình 5.8 inch, giá tốt, performance mid-range, pin 4500mAh.", Price = 9990000, Stock = 45, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1013, Name = "Smartphone Camera King", Description = "Camera 200MP, Zoom optical 10x, AI night mode, 8K video recording.", Price = 28990000, Stock = 20, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1014, Name = "Smartphone Gaming Beast", Description = "Màn hình 144Hz, GPU dedicated, cooling system, RAM 24GB.", Price = 22990000, Stock = 16, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1015, Name = "Smartphone Budget Hero", Description = "Giá siêu rẻ dưới 5 triệu, pin 6000mAh, camera kép, 5G.", Price = 4990000, Stock = 60, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1016, Name = "Smartphone Foldable Max", Description = "Gập được, màn hình 8 inch, camera đầu, design futuristic.", Price = 35990000, Stock = 10, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1017, Name = "Smartphone Pro Max Plus", Description = "Flagship tại 32MP x4 cameras, periscope zoom, titanium frame.", Price = 32990000, Stock = 12, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1018, Name = "Smartphone Mid-Range Star", Description = "Chiến thủ mid-range với Snapdragon 7 Gen 3, camera tốt, giá hợp lý.", Price = 13990000, Stock = 35, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1019, Name = "Smartphone Portrait Pro", Description = "Camera 48MP với aperture lớn, portrait mode siêu đẹp, sensor ổn định.", Price = 14990000, Stock = 28, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },
                new Product { Id = 1020, Name = "Smartphone Outdoor Tough", Description = "IP69 waterproof, shockproof, thermal camera, pin 7500mAh.", Price = 18990000, Stock = 22, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop", CategoryId = 2 },

                // TAI NGHE (21-30)
                new Product { Id = 1021, Name = "Tai nghe Bluetooth Alpha", Description = "Chống ồn chủ động (ANC), pin 40 giờ, codec LDAC, spatial audio.", Price = 5290000, Stock = 50, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1022, Name = "Tai nghe Gaming Thunder", Description = "Surround 7.1, latency 20ms, mic chuyên game, RGB lighting.", Price = 3290000, Stock = 35, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1023, Name = "Tai nghe Sport Runner", Description = "IP67 waterproof, secure fit, bone conduction option, pin 8 giờ.", Price = 1890000, Stock = 45, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1024, Name = "Tai nghe DJ Pro", Description = "Detachable cable, 50mm drivers, tuned cho mixing, foldable design.", Price = 4590000, Stock = 20, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1025, Name = "Tai nghe Studio Monitor", Description = "Flat frequency response, accurate sound, Balanced cable included.", Price = 6990000, Stock = 15, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1026, Name = "Tai nghe Earbud True Wireless", Description = "50h battery total, instant pairing, gesture control, compact case.", Price = 2490000, Stock = 55, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1027, Name = "Tai nghe Commute Comfort", Description = "Comfort wear 10 giờ, active noise cancel, quick-touch controls.", Price = 1990000, Stock = 40, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1028, Name = "Tai nghe Premium Luxury", Description = "Carbon fiber, custom tuned, lifetime warranty, handcrafted.", Price = 9990000, Stock = 8, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1029, Name = "Tai nghe Kids Safe", Description = "Volume limiter 85dB, colorful design, durable, kid-friendly controls.", Price = 590000, Stock = 70, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },
                new Product { Id = 1030, Name = "Tai nghe Conference Pro", Description = "Dual mic, noise cancellation, USB-C, 30h work battery.", Price = 3890000, Stock = 25, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 3 },

                // CAMERA & PHOTO (31-38)
                new Product { Id = 1031, Name = "Camera hành trình Ultra HD", Description = "4K@60fps, góc rộng 170°, ghi đêm rõ nét, dual channel.", Price = 3490000, Stock = 25, ImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1032, Name = "Camera Mirrorless Full Frame", Description = "24MP, quay 8K@30p, body chống nước, hybrid AF cực nhanh.", Price = 27990000, Stock = 12, ImageUrl = "https://images.unsplash.com/photo-1519183071298-a2962be90b44?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1033, Name = "Webcam 1080p Pro", Description = "Full HD, góc 90°, micro khử ồn, auto-focus, light correction.", Price = 1290000, Stock = 48, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1034, Name = "Máy ảnh Action 4K", Description = "Waterproof IP68, stabilization 6-axis, touchscreen 2 inch, voice control.", Price = 4990000, Stock = 20, ImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1035, Name = "Ống kính Telephoto 70-200mm", Description = "f/2.8 constant, image stabilization, weather sealed, AF siêu nhanh.", Price = 12990000, Stock = 10, ImageUrl = "https://images.unsplash.com/photo-1519183071298-a2962be90b44?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1036, Name = "Webcam 4K Ultra", Description = "4K@30fps, wide angle, auto-framing, voice command compatible.", Price = 2290000, Stock = 32, ImageUrl = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1037, Name = "Drone Photography Pro", Description = "48MP camera, 7km range, 46 min flight time, 8K video, AI tracking.", Price = 18990000, Stock = 8, ImageUrl = "https://images.unsplash.com/photo-1526406915894-7bcd65f60a54?q=80&w=800&h=600&fit=crop", CategoryId = 4 },
                new Product { Id = 1038, Name = "Lens Gimbal Stabilizer", Description = "3-axis stabilization, wireless control, smooth 4K video, compatible most cameras.", Price = 8990000, Stock = 15, ImageUrl = "https://images.unsplash.com/photo-1526406915894-7bcd65f60a54?q=80&w=800&h=600&fit=crop", CategoryId = 4 },

                // PHÍM & CHUỘT (39-45)
                new Product { Id = 1039, Name = "Bàn phím cơ Retro RGB", Description = "Switch blue mechanical, layout fullsize, LED RGB customizable.", Price = 1890000, Stock = 40, ImageUrl = "https://images.unsplash.com/photo-1587829191301-4ba8a0664415?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1040, Name = "Chuột gaming Titan", Description = "16000 DPI, thiết kế ergonomic, RGB lighting, 8 programmable buttons.", Price = 890000, Stock = 45, ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1041, Name = "Bàn phím Wireless Silent", Description = "Chiclet keys, quiet typing, 3 months battery, slim profile.", Price = 590000, Stock = 55, ImageUrl = "https://images.unsplash.com/photo-1587829191301-4ba8a0664415?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1042, Name = "Bàn phím Mechanical Gaming RGB", Description = "Hot-swap switches, stabilizers modded, per-key RGB, USB-C.", Price = 2290000, Stock = 30, ImageUrl = "https://images.unsplash.com/photo-1587829191301-4ba8a0664415?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1043, Name = "Chuột Wireless Office", Description = "Silent click, 18 tháng pin AA, precision tracking, design minimal.", Price = 290000, Stock = 70, ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1044, Name = "Bàn phím & Chuột Combo", Description = "Pairing wireless 2.4GHz, dual USB receiver, pin lâu dài.", Price = 690000, Stock = 50, ImageUrl = "https://images.unsplash.com/photo-1587829191301-4ba8a0664415?q=80&w=800&h=600&fit=crop", CategoryId = 5 },
                new Product { Id = 1045, Name = "Chuột Gaming Wireless", Description = "Latency 1ms, side buttons, RGB zones, light weight 80g.", Price = 1290000, Stock = 38, ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?q=80&w=800&h=600&fit=crop", CategoryId = 5 },

                // MÀNG HÌNH & DISPLAY (46-50)
                new Product { Id = 1046, Name = "Màn hình 27inch QHD Gaming", Description = "2560x1440, 165Hz, IPS panel, FreeSync/G-Sync, 1ms response.", Price = 8590000, Stock = 15, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 6 },
                new Product { Id = 1047, Name = "Màn hình 4K Ultrawide", Description = "5120x2160, 60Hz, USB-C power delivery, height adjustable stand.", Price = 18990000, Stock = 8, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 6 },
                new Product { Id = 1048, Name = "Màn hình 24inch Full HD Office", Description = "1920x1080, 60Hz, FlickerFree, brightness adjust, VESA mount.", Price = 2990000, Stock = 35, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 6 },
                new Product { Id = 1049, Name = "Màn hình Gaming 360Hz Esports", Description = "1080p, 360Hz refresh, 0.5ms, color-accurate, RGB backlight.", Price = 12990000, Stock = 10, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 6 },
                new Product { Id = 1050, Name = "Portable Monitor 15.6 USB-C", Description = "1920x1080 IPS, USB-C power + video, speakers built-in, lightweight.", Price = 3490000, Stock = 28, ImageUrl = "https://images.unsplash.com/photo-1515879218367-8466d910aaa4?q=80&w=800&h=600&fit=crop", CategoryId = 6 }
            };
        }

        /// <summary>20 sản phẩm bổ sung (Id để DB tự sinh). Chèn theo tên trong Program nếu chưa có.</summary>
        public static List<Product> GetTwentyExtraProducts()
        {
            const string phone = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&h=600&fit=crop";
            const string laptop = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?q=80&w=800&h=600&fit=crop";
            const string audio = "https://images.unsplash.com/photo-1511367461989-f85a21fda167?q=80&w=800&h=600&fit=crop";
            const string camera = "https://images.unsplash.com/photo-1518770660439-4636190af475?q=80&w=800&h=600&fit=crop";
            const string keyboard = "https://images.unsplash.com/photo-1587829191301-4ba8a0664415?q=80&w=800&h=600&fit=crop";
            const string network = "https://images.unsplash.com/photo-1544197150-b99a580bb7a8?q=80&w=800&h=600&fit=crop";
            const string power = "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?q=80&w=800&h=600&fit=crop";

            return new List<Product>
            {
                new Product { Name = "Samsung Galaxy S25 Ultra", Description = "Snapdragon 8 Elite, S Pen, camera 200MP, màn 6.9 inch Dynamic AMOLED 2X.", Price = 32990000, Stock = 12, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "Google Pixel 9 Pro", Description = "Tensor G4, Magic Eraser, Night Sight, Android gốc, 7 năm cập nhật.", Price = 22490000, Stock = 10, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "Xiaomi 15", Description = "Snapdragon 8 Gen 4, Leica camera, pin 5400mAh, sạc nhanh 90W.", Price = 14990000, Stock = 22, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "OPPO Find X8", Description = "Hasselblad tuning, zoom tiềm vọng, sạc SuperVOOC 80W, thiết kế mỏng.", Price = 18990000, Stock = 15, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "Nothing Phone (3)", Description = "Glyph Interface, Snapdragon 8s Gen 3, màn 6.7 inch 120Hz.", Price = 12990000, Stock = 18, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "iPhone 14", Description = "Chip A15 Bionic, camera kép 12MP, Face ID, 5G, pin cả ngày.", Price = 17990000, Stock = 25, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "ASUS ROG Phone 9", Description = "Gaming phone, tản nhiệt tích hợp, màn 165Hz, pin 5800mAh.", Price = 21990000, Stock = 8, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "realme GT 6", Description = "Chip flagship, sạc 120W, màn AMOLED 6000 nits, giá hiệu năng.", Price = 9990000, Stock = 30, ImageUrl = phone, CategoryId = 2 },
                new Product { Name = "MacBook Air 15 inch M3", Description = "Apple M3, RAM 16GB, SSD 512GB, pin 18 giờ, không quạt.", Price = 28990000, Stock = 9, ImageUrl = laptop, CategoryId = 1 },
                new Product { Name = "Dell XPS 15 OLED", Description = "Intel Core Ultra 7, RTX 4050, màn OLED 3.5K, vỏ nhôm cao cấp.", Price = 42990000, Stock = 6, ImageUrl = laptop, CategoryId = 1 },
                new Product { Name = "LG gram 17 2025", Description = "Siêu nhẹ dưới 1.4kg, pin 80Wh, màn 17 inch WQXGA, MIL-STD.", Price = 35990000, Stock = 7, ImageUrl = laptop, CategoryId = 1 },
                new Product { Name = "Sony WH-1000XM6", Description = "ANC đỉnh, LDAC, 30 giờ pin, multipoint, cảm biến đeo tự dừng nhạc.", Price = 7990000, Stock = 20, ImageUrl = audio, CategoryId = 3 },
                new Product { Name = "Apple AirPods Pro 3", Description = "Adaptive Audio, USB-C, chống ồn chủ động, Spatial Audio cá nhân hóa.", Price = 4990000, Stock = 35, ImageUrl = audio, CategoryId = 3 },
                new Product { Name = "Bose QuietComfort Ultra Earbuds", Description = "ANC mạnh, Immersive Audio, pin 6h + hộp 18h, fit ổn định.", Price = 9490000, Stock = 14, ImageUrl = audio, CategoryId = 3 },
                new Product { Name = "Canon EOS R50 Kit RF-S18-45", Description = "APS-C 24.2MP, quay 4K không crop, Dual Pixel AF II, nhẹ cho Vlog.", Price = 16990000, Stock = 11, ImageUrl = camera, CategoryId = 4 },
                new Product { Name = "GoPro HERO13 Black", Description = "5.3K60, HyperSmooth 6.0, chống nước 10m, GPS & metadata.", Price = 10990000, Stock = 16, ImageUrl = camera, CategoryId = 4 },
                new Product { Name = "Logitech MX Keys S", Description = "Bàn phím không dây low-profile, đèn nền thông minh, Easy-Switch 3 thiết bị.", Price = 2490000, Stock = 28, ImageUrl = keyboard, CategoryId = 5 },
                new Product { Name = "Keychron Q1 Pro QMK", Description = "Khung nhôm, hot-swap, Bluetooth/USB-C, layout 75%, tùy chỉnh phím.", Price = 3990000, Stock = 19, ImageUrl = keyboard, CategoryId = 5 },
                new Product { Name = "ASUS RT-AX86U Pro", Description = "Wi-Fi 6, 2.5G WAN/LAN, AiProtection Pro, tối ưu game Mobile Game Mode.", Price = 5490000, Stock = 13, ImageUrl = network, CategoryId = 7 },
                new Product { Name = "Anker Prime Power Bank 27650mAh", Description = "Sạc nhanh 140W, 3 cổng, màn hiển thị, phù hợp laptop & điện thoại.", Price = 2190000, Stock = 24, ImageUrl = power, CategoryId = 8 }
            };
        }
    }
}
