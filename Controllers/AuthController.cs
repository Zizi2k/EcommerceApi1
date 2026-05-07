using EcommerceApi.DTOs;
using EcommerceApi.Models; // Thay bằng namespace của bạn
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // Để cho đơn giản ở mức đồ án, mình sẽ ví dụ kiểm tra cứng hoặc từ DB
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto login)
    {
        // 1. Kiểm tra User trong Database (Ở đây mình ví dụ check giả định)
        if (login.Username == "admin" && login.Password == "123456")
        {
            // 2. Tạo danh sách các quyền (Claims)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, login.Username),
                new Claim(ClaimTypes.NameIdentifier, "1"), // Giả sử ID của User này là 1
                new Claim(ClaimTypes.Role, "Admin")
            };

            // 3. Tạo Key bí mật (Phải trùng với key ở Program.cs Chặng 2)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Khởi tạo Token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token hết hạn sau 1 ngày
                signingCredentials: creds
            );

            // 5. Trả Token về cho Client
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        return Unauthorized("Sai tài khoản hoặc mật khẩu!");
    }
}