using CuoiKy.Models;

namespace CuoiKy.Patterns;

public interface IOrderObserver
{
    void OnOrderCreated(Order order);
    void OnOrderPaid(Order order);
}
