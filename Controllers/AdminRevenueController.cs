using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using CuoiKy.ViewModels;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminRevenueController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminRevenueController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var end = (endDate?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);
        var start = startDate?.Date ?? DateTime.Today.AddDays(-29);

        if (start > end)
        {
            (start, end) = (end.Date, start.Date.AddDays(1).AddTicks(-1));
        }

        var revenueStatuses = new[] { OrderStatus.Paid, OrderStatus.Shipping, OrderStatus.Completed };

        var ordersQuery = _dbContext.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .Where(o => revenueStatuses.Contains(o.Status));

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);
        var totalOrders = await ordersQuery.CountAsync();
        var paidStatuses = new[] { OrderStatus.Paid, OrderStatus.Shipping, OrderStatus.Completed };
        var paidOrders = await ordersQuery.CountAsync(o => paidStatuses.Contains(o.Status));

        var revenueByDays = await ordersQuery
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueByDay
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var topProductsRaw = await (
                from oi in _dbContext.OrderItems
                join o in _dbContext.Orders on oi.OrderId equals o.Id
                where o.CreatedAt >= start && o.CreatedAt <= end
                where revenueStatuses.Contains(o.Status)
                group oi by oi.ProductId into g
                select new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                }
            )
            .OrderByDescending(x => x.Quantity)
            .ThenByDescending(x => x.Revenue)
            .Take(6)
            .ToListAsync();

        var productIds = topProductsRaw.Select(x => x.ProductId).ToList();
        var productNameMap = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var topProducts = topProductsRaw
            .Select(x => new TopProductRevenue
            {
                ProductId = x.ProductId,
                Name = productNameMap.TryGetValue(x.ProductId, out var name) ? name : $"#{x.ProductId}",
                Quantity = x.Quantity,
                Revenue = x.Revenue
            })
            .ToList();

        var vm = new AdminRevenueViewModel
        {
            StartDate = start,
            EndDate = end,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            PaidOrders = paidOrders,
            RevenueByDays = revenueByDays,
            TopProducts = topProducts
        };

        return View(vm);
    }
}
