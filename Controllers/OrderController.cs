using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;

namespace CuoiKy.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public OrderController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> MyOrders(OrderStatus? status)
    {
        var username = User.Identity?.Name;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var query = _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        ViewBag.FilterStatus = status;
        var orders = await query.ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var username = User.Identity?.Name;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id, string cancelOption, string? otherReason)
    {
        var username = User.Identity?.Name;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        if (order == null)
        {
            return NotFound();
        }

        // Only allow cancel if Pending or Paid (before shipping)
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
        {
            TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
            return RedirectToAction(nameof(MyOrders));
        }

        // Build cancel reason
        string reason = cancelOption;
        if (cancelOption == "Lý do khác" && !string.IsNullOrWhiteSpace(otherReason))
        {
            reason = $"Lý do khác: {otherReason.Trim()}";
        }

        order.Status = OrderStatus.Canceled;
        order.CancelReason = reason;

        // Restore stock
        foreach (var item in order.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Hủy đơn hàng thành công!";
        return RedirectToAction(nameof(MyOrders));
    }
}

