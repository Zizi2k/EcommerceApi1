🚀 Công nghệ sử dụng
Back-end: ASP.NET Core Web API (.NET 8)
Database: SQL Server
ORM: Entity Framework Core (Code First)
Security: JWT (JSON Web Token) Authentication
Front-end: HTML5, CSS3, JavaScript (Fetch API)
API Documentation: Swagger UI
🛠 Chức năng chính
Người dùng:
Xem danh sách sản phẩm (tự động cập nhật từ API).
Đăng ký / Đăng nhập hệ thống (nhận JWT Token).
Thêm sản phẩm vào giỏ hàng (yêu cầu đăng nhập).
Quản lý giỏ hàng (xem, cập nhật số lượng, xóa).

Admin:
Quản lý sản phẩm (Thêm, Sửa, Xóa).
Quản lý kho hàng và đơn hàng (nếu bạn đã làm thêm).
📦 Hướng dẫn cài đặt
1. Yêu cầu hệ thống
Visual Studio 2022 (hoặc VS Code).
.NET 8 SDK.
SQL Server Express hoặc LocalDB.
2. Cấu hình Database
Mở file appsettings.json và thay đổi chuỗi kết nối (Connection String) phù hợp với máy của bạn:
JSON
"ConnectionStrings": {
  "Nhom1": "Data Source=localhost;Database=FlowerShopDB_NEW;User ID=sa;Password=123456;Trust Server Certificate=True"
}
3. Khởi tạo dữ liệu (Migrations)
Mở Package Manager Console trong Visual Studio và chạy:
PowerShell
Add-Migration InitialCreate
Update-Database
4. Chạy dự án
Nhấn F5 trong Visual Studio.
Truy cập Swagger tại: https://localhost:PORT/swagger để kiểm tra API.
Truy cập giao diện tại: https://localhost:PORT/index.html.
📂 Cấu trúc thư mục
/Controllers: Chứa các bộ điều khiển xử lý logic API (Auth, Product, Cart).
/Models: Các thực thể dữ liệu (Product, User, CartItem).
/Data: Cấu hình DbContext và Migrations.
/wwwroot: Chứa toàn bộ mã nguồn Front-end (HTML, CSS, JS).
📝 Thông tin nhóm thực hiện
Sinh viên: Phan Lê Duy - Phan Hoàng Giang Sơn - Lê Đăng Quang
Lớp: Nhóm 1
Giảng viên hướng dẫn: Lê Quang Thái
