using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Builder] - [Nhóm: Creational]
// Mục đích: Xây dựng Order từng bước linh hoạt.
public class OrderBuilder
{
    private readonly Order _order = new();
    private decimal _total;

    public OrderBuilder SetCustomerName(string customerName)
    {
        _order.CustomerName = customerName;
        return this;
    }

    public OrderBuilder SetShippingAddress(string shippingAddress)
    {
        _order.ShippingAddress = shippingAddress;
        return this;
    }

    public OrderBuilder SetPaymentMethod(PaymentMethod paymentMethod)
    {
        _order.PaymentMethod = paymentMethod;
        return this;
    }

    public OrderBuilder AddItem(Product product, int quantity)
    {
        var item = new OrderItem
        {
            ProductId = product.Id,
            Product = product,
            Quantity = quantity,
            UnitPrice = product.Price,
        };
        _order.Items.Add(item);
        _total += item.TotalPrice;
        return this;
    }

    public Order Build()
    {
        _order.TotalAmount = _total;
        return _order;
    }
}
