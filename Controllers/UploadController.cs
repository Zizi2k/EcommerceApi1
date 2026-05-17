using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    /// <summary>Upload file tĩnh (ảnh sản phẩm) — route riêng tránh xung đột / 405 với api/Products.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("product-image")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<object>> UploadProductImage(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Thiếu file ảnh." });

            if (!AllowedImageContentTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Chỉ chấp nhận JPEG, PNG, WebP hoặc GIF." });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            {
                ext = file.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".png"
                };
            }

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "", "uploads", "products");
            Directory.CreateDirectory(uploadsDir);

            var safeExt = AllowedImageExtensions.Contains(ext) ? ext : ".png";
            var storedName = $"{Guid.NewGuid():N}{safeExt}";
            var physicalPath = Path.Combine(uploadsDir, storedName);

            await using (var stream = System.IO.File.Create(physicalPath))
                await file.CopyToAsync(stream, cancellationToken);

            var publicUrl = $"/uploads/products/{storedName}";
            return Ok(new { url = publicUrl });
        }

        [HttpPost("profile-image")]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<object>> UploadProfileImage(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Thiếu file ảnh." });

            if (!AllowedImageContentTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Chỉ chấp nhận JPEG, PNG, WebP hoặc GIF." });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            {
                ext = file.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".png"
                };
            }

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "", "uploads", "profiles");
            Directory.CreateDirectory(uploadsDir);

            var safeExt = AllowedImageExtensions.Contains(ext) ? ext : ".png";
            var storedName = $"{Guid.NewGuid():N}{safeExt}";
            var physicalPath = Path.Combine(uploadsDir, storedName);

            await using (var stream = System.IO.File.Create(physicalPath))
                await file.CopyToAsync(stream, cancellationToken);

            var publicUrl = $"/uploads/profiles/{storedName}";
            return Ok(new { url = publicUrl });
        }
    }
}
