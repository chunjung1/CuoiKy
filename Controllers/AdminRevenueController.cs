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

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? filterType, int? year)
    {
        string type = filterType ?? "day";
        int selectedYear = year ?? DateTime.Today.Year;

        DateTime start;
        DateTime end;

        if (type == "month" || type == "quarter")
        {
            start = new DateTime(selectedYear, 1, 1);
            end = new DateTime(selectedYear, 12, 31, 23, 59, 59, 999);
        }
        else // "day"
        {
            end = (endDate?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);
            start = startDate?.Date ?? DateTime.Today.AddDays(-29);
            if (start > end)
            {
                (start, end) = (end.Date, start.Date.AddDays(1).AddTicks(-1));
            }
        }

        var ordersQuery = _dbContext.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .Where(o => o.PaymentStatus == PaymentStatus.Paid);

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);
        var totalOrders = await ordersQuery.CountAsync();
        var paidOrders = totalOrders;

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

        var groupedItems = new List<RevenueGroupedItem>();

        if (type == "month")
        {
            var query = await ordersQuery
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    Orders = g.Count()
                })
                .ToListAsync();

            for (int m = 1; m <= 12; m++)
            {
                var match = query.FirstOrDefault(x => x.Month == m);
                groupedItems.Add(new RevenueGroupedItem
                {
                    Label = $"Tháng {m:00}",
                    Revenue = match?.Revenue ?? 0m,
                    Orders = match?.Orders ?? 0
                });
            }
        }
        else if (type == "quarter")
        {
            var query = await ordersQuery
                .GroupBy(o => (o.CreatedAt.Month - 1) / 3 + 1)
                .Select(g => new
                {
                    Quarter = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    Orders = g.Count()
                })
                .ToListAsync();

            for (int q = 1; q <= 4; q++)
            {
                var match = query.FirstOrDefault(x => x.Quarter == q);
                groupedItems.Add(new RevenueGroupedItem
                {
                    Label = $"Quý {q}",
                    Revenue = match?.Revenue ?? 0m,
                    Orders = match?.Orders ?? 0
                });
            }
        }
        else // "day"
        {
            foreach (var r in revenueByDays)
            {
                groupedItems.Add(new RevenueGroupedItem
                {
                    Label = r.Date.ToString("dd/MM"),
                    Revenue = r.Revenue,
                    Orders = r.Orders
                });
            }
        }

        var topProductsRaw = await (
                from oi in _dbContext.OrderItems
                join o in _dbContext.Orders on oi.OrderId equals o.Id
                where o.CreatedAt >= start && o.CreatedAt <= end
                where o.PaymentStatus == PaymentStatus.Paid
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
            FilterType = type,
            SelectedYear = selectedYear,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            PaidOrders = paidOrders,
            RevenueByDays = revenueByDays,
            RevenueGroupedItems = groupedItems,
            TopProducts = topProducts
        };

        return View(vm);
    }
}
