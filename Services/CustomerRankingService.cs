using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services
{
    public class CustomerRankingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDemoUserStore _users;

        public CustomerRankingService(ApplicationDbContext context, IDemoUserStore users)
        {
            _context = context;
            _users = users;
        }

        public async Task<List<CustomerRankDto>> GetRankedCustomersAsync()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ToListAsync();

            var deliveredOrders = orders
                .Where(o => OrderStatuses.Normalize(o.Status) == OrderStatuses.Delivered)
                .ToList();

            var grouped = deliveredOrders
                .GroupBy(o => o.UserId)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(o => o.CreatedAtUtc).First();
                    var accountUsername = g.Select(o => o.AccountUsername).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
                        ?? _users.UsernameForUserId(g.Key)
                        ?? ("user-" + g.Key);

                    UserProfileData profile;
                    try
                    {
                        profile = _users.GetProfile(accountUsername);
                    }
                    catch
                    {
                        profile = new UserProfileData { Username = accountUsername, DisplayName = accountUsername };
                    }

                    var profileName = profile.DisplayName?.Trim();
                    var latestCodName = latest.CustomerName?.Trim();
                    // Ưu tiên tên trong Cài đặt (vd. Trang), sau đó tên người nhận COD
                    var displayName = !string.IsNullOrWhiteSpace(profileName)
                            && !string.Equals(profileName, accountUsername, StringComparison.OrdinalIgnoreCase)
                        ? profileName
                        : !string.IsNullOrWhiteSpace(latestCodName)
                            ? latestCodName
                            : accountUsername;

                    var phone = g.Select(o => o.CustomerPhone).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    var address = g.Select(o => o.ShippingAddress).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                    var orderCount = g.Count();
                    var tier = ResolveTier(orderCount);

                    return new CustomerRankDto
                    {
                        UserId = g.Key,
                        AccountUsername = accountUsername,
                        DisplayName = displayName,
                        Phone = phone,
                        ShippingAddress = address,
                        OrderCount = orderCount,
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        LastOrderAtUtc = g.Max(o => o.CreatedAtUtc),
                        Tier = tier.Code,
                        TierLabel = tier.Label,
                        RecentOrders = g
                            .OrderByDescending(o => o.CreatedAtUtc)
                            .Take(5)
                            .Select(o => new CustomerOrderSummaryDto
                            {
                                OrderId = o.Id,
                                TotalAmount = o.TotalAmount,
                                PaymentMethod = o.PaymentMethod,
                                CreatedAtUtc = o.CreatedAtUtc,
                                ItemCount = o.Items.Count
                            })
                            .ToList()
                    };
                })
                .OrderByDescending(c => c.OrderCount)
                .ThenByDescending(c => c.TotalSpent)
                .ThenByDescending(c => c.LastOrderAtUtc)
                .ToList();

            for (var i = 0; i < grouped.Count; i++)
                grouped[i].Rank = i + 1;

            return grouped;
        }

        private static (string Code, string Label) ResolveTier(int orderCount) => orderCount switch
        {
            >= 10 => ("diamond", "VIP Kim cương"),
            >= 7 => ("gold", "Vàng"),
            >= 4 => ("silver", "Bạc"),
            >= 2 => ("bronze", "Đồng"),
            1 => ("new", "Mới"),
            _ => ("none", "—")
        };
    }
}
