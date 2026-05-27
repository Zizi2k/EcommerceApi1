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
        private readonly PhoneVerificationService _phoneVerification;

        public AuthController(
            IDemoUserStore users,
            CustomerRankingService customerRanking,
            UserProfileResolver profiles,
            PhoneVerificationService phoneVerification)
        {
            _users = users;
            _customerRanking = customerRanking;
            _profiles = profiles;
            _phoneVerification = phoneVerification;
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

            var normalizedPhone = PhoneVerificationService.NormalizePhone(dto.Phone);
            if (!PhoneVerificationService.IsValidVietnamPhone(normalizedPhone))
                return BadRequest(new { message = "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)." });
            if (!_phoneVerification.TryConsumeToken(dto.OtpToken, 0, normalizedPhone, out _))
                return BadRequest(new { message = "Số điện thoại chưa được xác thực OTP hoặc mã đã hết hạn." });

            var user = dto.Username.Trim();
            if (!_users.TryRegister(user, dto.Password ?? "", dto.DisplayName, out var error))
                return BadRequest(new { message = error });

            return Ok(BuildAuthResponse(user));
        }

        [HttpPost("register/send-otp")]
        public IActionResult SendRegisterOtp([FromBody] SendPhoneOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Thiếu số điện thoại." });
            try
            {
                var normalized = PhoneVerificationService.NormalizePhone(dto.Phone);
                var code = _phoneVerification.SendOtp(normalized);
                return Ok(new
                {
                    message = "Đã gửi mã OTP. Mã có hiệu lực 5 phút.",
                    phone = normalized,
                    expiresInSeconds = 300,
                    demoCode = code
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register/verify-otp")]
        public IActionResult VerifyRegisterOtp([FromBody] VerifyPhoneOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Thiếu số điện thoại." });
            try
            {
                var normalized = PhoneVerificationService.NormalizePhone(dto.Phone);
                var token = _phoneVerification.VerifyOtp(normalized, dto.Code, 0);
                return Ok(new
                {
                    message = "Xác thực số điện thoại thành công.",
                    phone = normalized,
                    otpToken = token,
                    verifiedInSeconds = 1200
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
