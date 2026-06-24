using CuoiKy.Data;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Observer] - [Nhóm: Behavioral]
// Mục đích: Khi Order tạo xong, Inventory được thông báo tự động trừ.
public class InventoryObserver : IOrderObserver
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryObserver(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void OnOrderCreated(Order order)
    {
        foreach (var item in order.Items)
        {
            var product = _dbContext.Products.Find(item.ProductId);
            if (product != null)
            {
                product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
            }
        }
        _dbContext.SaveChanges();
    }

    public void OnOrderPaid(Order order)
    {
        // Không làm gì, tồn kho đã được trừ lúc tạo đơn hàng.
    }
}
