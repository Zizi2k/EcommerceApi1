using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập mới dùng được các API này
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách giỏ hàng của người dùng hiện tại
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Lấy ID người dùng từ Token

            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product) // "Join" với bảng Product để lấy tên, giá, ảnh
                .ToListAsync();
        }

        // 2. Thêm sản phẩm vào giỏ hàng
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(CartItemDto cartItemDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == cartItemDto.ProductId);

            if (existingItem != null)
            {
                // Nếu có rồi thì tăng số lượng
                existingItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                // Nếu chưa có thì thêm mới vào bảng CartItems
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm vào giỏ hàng thành công!" });
        }

        // 3. Cập nhật số lượng (Ví dụ nhấn nút + hoặc - trong giỏ hàng)
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int newQuantity)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return NotFound();

            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem); // Nếu số lượng = 0 thì xóa luôn khỏi giỏ
            }
            else
            {
                cartItem.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. Xóa sản phẩm khỏi giỏ
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}