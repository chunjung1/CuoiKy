using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using Microsoft.AspNetCore.Authorization;

namespace CuoiKy.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCouponController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminCouponController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var coupons = await _dbContext.Coupons
            .OrderByDescending(c => c.ExpiryDate)
            .ToListAsync();
        return View(coupons);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Coupon coupon)
    {
        if (ModelState.IsValid)
        {
            // Tránh trùng mã giảm giá
            var exists = await _dbContext.Coupons.AnyAsync(c => c.Code.ToLower() == coupon.Code.ToLower());
            if (exists)
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại trong hệ thống.");
                return View(coupon);
            }

            coupon.Code = coupon.Code.ToUpper().Trim();
            _dbContext.Coupons.Add(coupon);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tạo mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(coupon);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _dbContext.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        return View(coupon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Coupon coupon)
    {
        if (id != coupon.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var exists = await _dbContext.Coupons.AnyAsync(c => c.Code.ToLower() == coupon.Code.ToLower() && c.Id != id);
            if (exists)
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại.");
                return View(coupon);
            }

            var existing = await _dbContext.Coupons.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Code = coupon.Code.ToUpper().Trim();
            existing.DiscountPercent = coupon.DiscountPercent;
            existing.ExpiryDate = coupon.ExpiryDate;
            existing.UsageLimit = coupon.UsageLimit;
            existing.UsedCount = coupon.UsedCount;

            _dbContext.Coupons.Update(existing);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(coupon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _dbContext.Coupons.FindAsync(id);
        if (coupon == null) return Json(new { success = false, message = "Không tìm thấy mã giảm giá." });

        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync();
        return Json(new { success = true, message = "Xóa mã giảm giá thành công!" });
    }
}
