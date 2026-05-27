using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserProfileResolver _profiles;

        public ProfileController(UserProfileResolver profiles)
        {
            _profiles = profiles;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var login = CurrentLogin();
            if (login == null) return Unauthorized();

            var profile = await _profiles.GetProfileAsync(login);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ tài khoản." });

            return Ok(ToResponse(profile));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var login = CurrentLogin();
            if (login == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            var profile = await _profiles.UpdateProfileAsync(login, dto.DisplayName, dto.AvatarUrl, dto.BackgroundUrl);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ tài khoản." });

            return Ok(ToResponse(profile));
        }

        [HttpPut("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var login = CurrentLogin();
            if (login == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            if (!_profiles.TryChangePassword(login, dto.CurrentPassword ?? "", dto.NewPassword ?? "", out var error))
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

        private static object ToResponse(UserProfileData profile) => new
        {
            username = profile.Username,
            displayName = profile.DisplayName,
            avatarUrl = profile.AvatarUrl,
            backgroundUrl = profile.BackgroundUrl
        };
    }
}
