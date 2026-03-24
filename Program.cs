using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using CuoiKy.Patterns;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Design Patterns
builder.Services.AddScoped<IOrderObserver, InventoryObserver>();
builder.Services.AddScoped<CheckoutFacade>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // db.Database.EnsureCreated();

    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "Phone" },
            new Category { Name = "Laptop" },
            new Category { Name = "Keyboard" },
            new Category { Name = "Mouse" },
            new Category { Name = "Headphone" }
        );
        db.SaveChanges();
    }

    if (!db.Products.Any())
    {
        var factory = new ProductFactory();
        var categoryMap = db.Categories.ToDictionary(c => c.Name, c => c.Id);
        db.Products.AddRange(
            factory.CreateProduct(categoryMap["Phone"], "Điện thoại A", 1200, 10, "Smartphone 4G"),
            factory.CreateProduct(categoryMap["Laptop"], "Laptop B", 2200, 6, "Laptop mỏng nhẹ"),
            factory.CreateProduct(categoryMap["Keyboard"], "Bàn phím C", 150, 20, "Bàn phím cơ"),
            factory.CreateProduct(categoryMap["Mouse"], "Chuột D", 80, 30, "Chuột không dây"),
            factory.CreateProduct(categoryMap["Headphone"], "Tai nghe E", 200, 15, "Tai nghe chống ồn")
        );
        db.SaveChanges();
    }
}

app.Run();
