using CuoiKy.Data;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Facade] - [Nhóm: Structural]
// Mục đích: Gom các bước checkout (kiểm tra kho, lưu đơn, thông báo inventory) thành 1 API đơn giản.
public class CheckoutFacade
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderObserver _inventoryObserver;

    public CheckoutFacade(ApplicationDbContext dbContext, IOrderObserver inventoryObserver)
    {
        _dbContext = dbContext;
        _inventoryObserver = inventoryObserver;
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
        var utcNow = DateTime.UtcNow;
        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        order.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

        // Lưu order vào database
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        // Notify inventory observer (trừ tồn kho)
        _inventoryObserver.OnOrderCreated(order);
    }
}
