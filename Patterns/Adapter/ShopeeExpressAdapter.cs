using System;
using System.Threading.Tasks;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

public class ShopeeExpressAdapter : IShippingServiceAdapter
{
    public async Task<string> CreateShippingOrderAsync(Order order)
    {
        // Giả lập thời gian trễ kết nối API (500ms)
        await Task.Delay(500);

        // Mô phỏng sinh mã vận đơn ngẫu nhiên
        var random = new Random();
        var trackingNumber = $"SPX-VN-{random.Next(100000000, 999999999)}";

        Console.WriteLine($"[ShopeeExpress] Đã đẩy đơn hàng #{order.Id} sang đối tác. Mã vận đơn: {trackingNumber}");
        return trackingNumber;
    }
}
