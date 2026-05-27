using EcommerceApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl }) ?? "/Account/GoogleCallback";
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded || result.Principal == null)
                return Redirect("/oauth-callback.html?error=google_auth_failed");

            var email = result.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "";

            // Các claim thường gặp của Google
            var googleId =
                result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? result.Principal.FindFirstValue("sub")
                ?? "";
            var avatar =
                result.Principal.FindFirstValue("picture")
                ?? result.Principal.FindFirstValue("urn:google:picture")
                ?? "";

            if (string.IsNullOrWhiteSpace(email))
                return Redirect("/oauth-callback.html?error=missing_email");

            var displayName = string.IsNullOrWhiteSpace(name) ? email : name.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                user = new Models.User
                {
                    Email = email,
                    Name = displayName,
                    FullName = displayName,
                    PasswordHash = "GOOGLE_OAUTH",
                    AuthProvider = "Google",
                    CreatedAt = DateTime.UtcNow,
                    AvatarUrl = string.IsNullOrWhiteSpace(avatar) ? null : avatar,
                    GoogleId = string.IsNullOrWhiteSpace(googleId) ? null : googleId,
                    GoogleSub = string.IsNullOrWhiteSpace(googleId) ? null : googleId,
                    Role = "Customer"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                var changed = false;
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    if (user.Name != displayName) { user.Name = displayName; changed = true; }
                    if (user.FullName != displayName) { user.FullName = displayName; changed = true; }
                }
                if (!string.IsNullOrWhiteSpace(avatar) && user.AvatarUrl != avatar) { user.AvatarUrl = avatar; changed = true; }
                if (!string.IsNullOrWhiteSpace(googleId))
                {
                    if (user.GoogleId != googleId) { user.GoogleId = googleId; changed = true; }
                    if (user.GoogleSub != googleId) { user.GoogleSub = googleId; changed = true; }
                }
                if (string.IsNullOrWhiteSpace(user.AuthProvider) || user.AuthProvider == "Local")
                {
                    user.AuthProvider = "Google";
                    changed = true;
                }
                if (changed) await _context.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync("External");

            var token = BuildJwt(user);
            var url = "/oauth-callback.html"
                + "?token=" + Uri.EscapeDataString(token)
                + "&name=" + Uri.EscapeDataString(user.FullName ?? user.Name ?? "")
                + "&avatar=" + Uri.EscapeDataString(user.AvatarUrl ?? "")
                + "&email=" + Uri.EscapeDataString(user.Email ?? "");

            return Redirect(url);
        }

        private static string BuildJwt(Models.User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer"),
                new("auth_provider", "Google"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
