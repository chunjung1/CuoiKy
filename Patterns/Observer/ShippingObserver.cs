using System;
using CuoiKy.Data;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

public class ShippingObserver : IOrderObserver
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ShopeeExpressAdapter _shopeeExpress;
    private readonly GiaoHangNhanhAdapter _ghn;

    public ShippingObserver(ApplicationDbContext dbContext, ShopeeExpressAdapter shopeeExpress, GiaoHangNhanhAdapter ghn)
    {
        _dbContext = dbContext;
        _shopeeExpress = shopeeExpress;
        _ghn = ghn;
    }

    public void OnOrderCreated(Order order)
    {
        // Nếu là COD (Thanh toán khi nhận hàng), đẩy đơn sang bên vận chuyển ngay lập tức
        if (order.PaymentMethod == PaymentMethod.Cash)
        {
            PushToShipping(order);
        }
    }

    public void OnOrderPaid(Order order)
    {
        // Khi thanh toán thành công (đối với PayOS hoặc BankTransfer), đẩy đơn giao nhận
        PushToShipping(order);
    }

    private void PushToShipping(Order order)
    {
        try
        {
            string trackingNumber;
            if (order.ShippingPartner == "GHN")
            {
                trackingNumber = _ghn.CreateShippingOrderAsync(order).GetAwaiter().GetResult();
            }
            else
            {
                trackingNumber = _shopeeExpress.CreateShippingOrderAsync(order).GetAwaiter().GetResult();
            }

            order.TrackingNumber = trackingNumber;
            order.Status = OrderStatus.Shipping; // Tự động cập nhật sang trạng thái Đang giao hàng

            _dbContext.SaveChanges();
            Console.WriteLine($"[ShippingObserver] Đơn hàng #{order.Id} đã chuyển sang đơn vị giao nhận. Tracking: {trackingNumber}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShippingObserver Error] Lỗi đẩy đơn hàng #{order.Id}: {ex.Message}");
        }
    }
}
