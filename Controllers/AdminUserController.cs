using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using Microsoft.AspNetCore.Authorization;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUserController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminUserController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _dbContext.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Username)
            .ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }

        // Kiểm tra nếu là Admin thì không cho xóa
        if (user.Role == UserRole.Admin)
        {
            return Json(new { success = false, message = "Không thể xóa người dùng có quyền Admin." });
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Json(new { success = true, message = "Đã xóa người dùng thành công." });
    }
}
