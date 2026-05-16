using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private static string AvatarUrlFor(string username)
        {
            var seed = string.IsNullOrWhiteSpace(username) ? "guest" : username.Trim();
            return $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}";
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Thiếu tên đăng nhập hoặc mật khẩu.");

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên đăng nhập đã tồn tại.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Username + "@demo.com", // Tạm thời gen email tự động
                PasswordHash = HashPassword(request.Password),
                Role = request.Username.ToLower() == "admin" ? "Admin" : "Customer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Username))
                return BadRequest("Thiếu tên đăng nhập.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username.Trim());

            if (user == null || user.PasswordHash != HashPassword(login.Password))
                return Unauthorized("Sai tài khoản hoặc mật khẩu!");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtSecret = _config["Jwt:SecretKey"] ?? "Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = user.Username,
                avatarUrl = AvatarUrlFor(user.Username)
            });
        }

        /// Hàm băm mật khẩu đơn giản (SHA256)
        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}