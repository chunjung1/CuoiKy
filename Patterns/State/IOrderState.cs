using CuoiKy.Models;

namespace CuoiKy.Patterns;

public interface IOrderState
{
    void PrintState();
    IOrderState NextState();
}

public class PendingState : IOrderState
{
    public void PrintState() => Console.WriteLine("Order đang chờ xử lý");
    public IOrderState NextState() => new ShippingState();
}

public class ShippingState : IOrderState
{
    public void PrintState() => Console.WriteLine("Order đang giao hàng");
    public IOrderState NextState() => new CompletedState();
}

public class CompletedState : IOrderState
{
    public void PrintState() => Console.WriteLine("Order đã hoàn thành");
    public IOrderState NextState() => this;
}

public class CanceledState : IOrderState
{
    public void PrintState() => Console.WriteLine("Order đã hủy");
    public IOrderState NextState() => this;
}

public class OrderStateContext
{
    private IOrderState _state;

    public OrderStateContext(OrderStatus status)
    {
        _state = status switch
        {
            OrderStatus.Pending => new PendingState(),
            OrderStatus.Shipping => new ShippingState(),
            OrderStatus.Completed => new CompletedState(),
            OrderStatus.Canceled => new CanceledState(),
            _ => new PendingState()
        };
    }

    public string ShowState()
    {
        _state.PrintState();
        return _state.GetType().Name;
    }

    public void MoveNext()
    {
        _state = _state.NextState();
    }
}
