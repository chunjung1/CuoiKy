using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CuoiKy.Data;
using CuoiKy.Models;
using System.Security.Claims;
using CuoiKy.ViewModels;

namespace CuoiKy.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AccountController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        // Đăng nhập bằng Email
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        
        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_dbContext.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
            return View(model);
        }

        if (_dbContext.Users.Any(u => u.Username == model.Username))
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập này đã được sử dụng.");
            return View(model);
        }

        var newUser = new User
        {
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,
            Role = UserRole.Customer
        };

        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            // Tạo mã token giả lập (6 chữ số)
            string token = new Random().Next(100000, 999999).ToString();
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(10);
            _dbContext.SaveChanges();

            // Lưu ý: Trong thực tế bạn sẽ gửi mã này qua Email (SmtpClient, SendGrid,...)
            // Ở đây tôi giả lập bằng cách thông báo cho người dùng
            TempData["SuccessMessage"] = $"Một mã khôi phục đã được gửi đến {email}. (Mã giả lập: {token})";
            ViewData["Email"] = email;
            return View("ResetPassword");
        }

        ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string email, string token, string newPassword)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.ResetToken == token);
        
        if (user != null && user.ResetTokenExpiry > DateTime.Now)
        {
            user.Password = newPassword;
            user.ResetToken = null; // Xóa token sau khi dùng
            user.ResetTokenExpiry = null;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        TempData["ErrorMessage"] = "Mã xác nhận không đúng hoặc đã hết hạn.";
        ViewData["Email"] = email;
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
