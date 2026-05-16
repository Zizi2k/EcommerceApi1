using EcommerceApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> DemoUsers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = "123456",
            ["khach"] = "123456",
            ["user"] = "123456",
        };

        /// <summary>Cùng tài khoản → cùng avatar (Dicebear theo seed = username).</summary>
        private static string AvatarUrlFor(string username)
        {
            var seed = string.IsNullOrWhiteSpace(username) ? "guest" : username.Trim();
            return $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}";
        }

        /// <summary>ID số trong JWT để giỏ hàng (Cart) khớp user — admin luôn là 1.</summary>
        private static string NameIdentifierFor(string username)
        {
            if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)) return "1";
            if (string.Equals(username, "khach", StringComparison.OrdinalIgnoreCase)) return "2";
            if (string.Equals(username, "user", StringComparison.OrdinalIgnoreCase)) return "3";
            var h = username.GetHashCode();
            return (System.Math.Abs(h % 999_000) + 1000).ToString();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Username))
                return BadRequest("Thiếu tên đăng nhập.");

            var user = login.Username.Trim();
            if (!DemoUsers.TryGetValue(user, out var expected) || expected != login.Password)
                return Unauthorized("Sai tài khoản hoặc mật khẩu!");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user),
                new Claim(ClaimTypes.NameIdentifier, NameIdentifierFor(user)),
                new Claim(ClaimTypes.Role, string.Equals(user, "admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = user,
                avatarUrl = AvatarUrlFor(user)
            });
        }
    }
}
