using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Decorator] - [Nhóm: Structural]
// Mục đích: Thêm phí lựa chọn (gói quà, giao hỏa tốc) cho Order.
public abstract class OrderComponent
{
    public abstract decimal GetTotal();
}

public class BasicOrderComponent : OrderComponent
{
    private readonly Order _order;
    public BasicOrderComponent(Order order)
    {
        _order = order;
    }

    public override decimal GetTotal() => _order.TotalAmount;
}

public abstract class OrderDecorator : OrderComponent
{
    protected readonly OrderComponent _component;
    public OrderDecorator(OrderComponent component)
    {
        _component = component;
    }
}

public class GiftWrapDecorator : OrderDecorator
{
    public GiftWrapDecorator(OrderComponent component) : base(component) { }
    public override decimal GetTotal() => _component.GetTotal() + 15000m;
}

public class ExpressShippingDecorator : OrderDecorator
{
    public ExpressShippingDecorator(OrderComponent component) : base(component) { }
    public override decimal GetTotal() => _component.GetTotal() + 25000m;
}
