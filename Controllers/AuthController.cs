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
        private readonly UserProfileResolver _profiles;

        public AuthController(IDemoUserStore users, CustomerRankingService customerRanking, UserProfileResolver profiles)
        {
            _users = users;
            _customerRanking = customerRanking;
            _profiles = profiles;
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
        public async Task<IActionResult> GetProfileMe()
        {
            var login = CurrentLogin();
            if (login == null) return Unauthorized();
            var profile = await _profiles.GetProfileAsync(login);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ tài khoản." });
            return Ok(ToProfileResponse(profile));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var login = CurrentLogin();
            if (login == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            var profile = await _profiles.UpdateProfileAsync(login, dto.DisplayName, dto.AvatarUrl, dto.BackgroundUrl);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ tài khoản." });
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

            if (!_profiles.TryChangePassword(username, dto.CurrentPassword ?? "", dto.NewPassword ?? "", out var error))
                return BadRequest(new { message = error });

            return Ok(new { message = "Đã đổi mật khẩu." });
        }

        private string? CurrentLogin()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email)) return email.Trim();
            var name = User.FindFirstValue(ClaimTypes.Name);
            return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        private string? CurrentUsername() => CurrentLogin();

        private static object ToProfileResponse(UserProfileData profile) => new
        {
            username = profile.Username,
            displayName = profile.DisplayName,
            avatarUrl = profile.AvatarUrl,
            backgroundUrl = profile.BackgroundUrl
        };
    }
}
