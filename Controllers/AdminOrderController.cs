using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using Microsoft.AspNetCore.Authorization;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminOrderController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminOrderController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _dbContext.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Paid;
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Xác nhận đã thu tiền thành công!" });
        }

        return Json(new { success = false, message = "Trạng thái đơn hàng không hợp lệ để xác nhận" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }
}
