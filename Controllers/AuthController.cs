using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDemoUserStore _users;
        private readonly CustomerRankingService _customerRanking;

        public AuthController(IDemoUserStore users, CustomerRankingService customerRanking)
        {
            _users = users;
            _customerRanking = customerRanking;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Username))
                return BadRequest(new { message = "Thiếu tên đăng nhập." });

            var user = login.Username.Trim();
            if (!_users.ValidatePassword(user, login.Password ?? ""))
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

            return Ok(BuildAuthResponse(user));
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest(new { message = "Thiếu tên đăng nhập." });

            if (!string.IsNullOrEmpty(dto.ConfirmPassword) && dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });

            var user = dto.Username.Trim();
            if (!_users.TryRegister(user, dto.Password ?? "", dto.DisplayName, out var error))
                return BadRequest(new { message = error });

            return Ok(BuildAuthResponse(user));
        }

        private object BuildAuthResponse(string user)
        {
            var profile = _users.GetProfile(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user),
                new Claim(ClaimTypes.NameIdentifier, _users.NameIdentifierFor(user)),
                new Claim(ClaimTypes.Role, string.Equals(user, "admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = profile.Username,
                displayName = profile.DisplayName,
                avatarUrl = profile.AvatarUrl,
                backgroundUrl = profile.BackgroundUrl
            };
        }

        [HttpGet("profile/me")]
        [Authorize]
        public IActionResult GetProfileMe()
        {
            var username = CurrentUsername();
            if (username == null) return Unauthorized();
            return Ok(ToProfileResponse(_users.GetProfile(username)));
        }

        [HttpPut("profile")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var username = CurrentUsername();
            if (username == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            var profile = _users.UpdateProfile(username, dto.DisplayName, dto.AvatarUrl, dto.BackgroundUrl);
            return Ok(ToProfileResponse(profile));
        }

        [HttpGet("admin/customers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerRankDto>>> GetAdminCustomers()
        {
            return Ok(await _customerRanking.GetRankedCustomersAsync());
        }

        [HttpPut("profile/password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var username = CurrentUsername();
            if (username == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            if (!_users.TryChangePassword(username, dto.CurrentPassword ?? "", dto.NewPassword ?? "", out var error))
                return BadRequest(new { message = error });

            return Ok(new { message = "Đã đổi mật khẩu." });
        }

        private string? CurrentUsername()
        {
            var name = User.FindFirstValue(ClaimTypes.Name);
            return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        private static object ToProfileResponse(UserProfileData profile) => new
        {
            username = profile.Username,
            displayName = profile.DisplayName,
            avatarUrl = profile.AvatarUrl,
            backgroundUrl = profile.BackgroundUrl
        };
    }
}
