using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCategoryController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminCategoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Tên loại sản phẩm không được rỗng.";
            return RedirectToAction(nameof(Index));
        }

        var exists = await _dbContext.Categories.AnyAsync(c => c.Name == name);
        if (exists)
        {
            TempData["ErrorMessage"] = "Loại sản phẩm này đã tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        _dbContext.Categories.Add(new Category { Name = name });
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Tạo loại sản phẩm thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return NotFound();

        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("Name", "Tên loại sản phẩm không được rỗng.");
            return View(category);
        }

        var exists = await _dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == name);
        if (exists)
        {
            ModelState.AddModelError("Name", "Loại sản phẩm này đã tồn tại.");
            return View(category);
        }

        category.Name = name;
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật loại sản phẩm thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return Json(new { success = false, message = "Không tìm thấy loại sản phẩm." });
        }

        var hasProducts = await _dbContext.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return Json(new { success = false, message = "Không thể xóa vì đang có sản phẩm thuộc loại này." });
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return Json(new { success = true, message = "Đã xóa loại sản phẩm." });
    }
}

