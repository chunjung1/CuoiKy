using System.Collections.Generic;
using CuoiKy.Data;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Facade] - [Nhóm: Structural]
// Mục đích: Gom các bước checkout (kiểm tra kho, lưu đơn, thông báo inventory) thành 1 API đơn giản.
public class CheckoutFacade
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEnumerable<IOrderObserver> _observers;

    public CheckoutFacade(ApplicationDbContext dbContext, IEnumerable<IOrderObserver> observers)
    {
        _dbContext = dbContext;
        _observers = observers;
    }

    public void PlaceOrder(Order order)
    {
        // Kiểm tra tồn kho
        foreach (var item in order.Items)
        {
            var product = _dbContext.Products.Find(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product {item.ProductId} không tồn tại.");
            }

            if (product.StockQuantity <= 0)
            {
                throw new InvalidOperationException($"Sản phẩm {product.Name} đã hết hàng.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Sản phẩm {product.Name} không đủ tồn kho.");
            }
        }

        // Update trạng thái order
        order.Status = OrderStatus.Pending;
        order.PaymentStatus = PaymentStatus.Pending;
        var utcNow = DateTime.UtcNow;
        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        order.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

        // Lưu order vào database
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        // Notify all observers
        foreach (var observer in _observers)
        {
            observer.OnOrderCreated(order);
        }
    }
}
