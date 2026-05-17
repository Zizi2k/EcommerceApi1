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
        private readonly IDemoUserStore _users;

        public ProfileController(IDemoUserStore users)
        {
            _users = users;
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var username = CurrentUsername();
            if (username == null) return Unauthorized();
            var profile = _users.GetProfile(username);
            return Ok(ToResponse(profile));
        }

        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var username = CurrentUsername();
            if (username == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu." });

            var profile = _users.UpdateProfile(username, dto.DisplayName, dto.AvatarUrl, dto.BackgroundUrl);
            return Ok(ToResponse(profile));
        }

        [HttpPut("password")]
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

        private static object ToResponse(UserProfileData profile) => new
        {
            username = profile.Username,
            displayName = profile.DisplayName,
            avatarUrl = profile.AvatarUrl,
            backgroundUrl = profile.BackgroundUrl
        };
    }
}
