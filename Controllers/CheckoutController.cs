using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApi.Controllers
{
    /// <summary>Thanh toán đa hình thức: tạo đơn từ giỏ hàng và xóa giỏ.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        public static readonly string[] AllowedPaymentMethods =
            ["COD", "BankTransfer", "MoMo", "VNPay", "Card"];

        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PaymentMethod))
                return BadRequest(new { message = "Thiếu hình thức thanh toán." });

            var method = dto.PaymentMethod.Trim();
            if (!AllowedPaymentMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Hình thức thanh toán không hợp lệ.", allowed = AllowedPaymentMethods });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
                return BadRequest(new { message = "Giỏ hàng trống, không thể thanh toán." });

            foreach (var line in cartItems)
            {
                if (line.Product == null)
                    return BadRequest(new { message = "Có sản phẩm không tồn tại trong giỏ hàng." });
            }

            decimal total = cartItems.Sum(c => c.Product!.Price * c.Quantity);

            var status = string.Equals(method, "COD", StringComparison.OrdinalIgnoreCase)
                ? "PendingPayment"
                : "Completed";

            var order = new Order
            {
                UserId = userId,
                TotalAmount = total,
                PaymentMethod = method,
                Status = status,
                CreatedAtUtc = DateTime.UtcNow,
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product!.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thanh toán thành công!",
                orderId = order.Id,
                totalAmount = total,
                paymentMethod = method,
                status = order.Status
            });
        }
    }
}
