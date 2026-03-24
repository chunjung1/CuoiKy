
namespace CuoiKy.Patterns;

public interface IPaymentStrategy
{
    string Pay(decimal amount);
}

public class CashPaymentStrategy : IPaymentStrategy
{
    public string Pay(decimal amount)
    {
        return $"Thanh toán tiền mặt: {amount:C}";
    }
}

public class BankTransferStrategy : IPaymentStrategy
{
    public string Pay(decimal amount)
    {
        return $"Thanh toán chuyển khoản: {amount:C}";
    }
}

public class PaymentContext
{
    private readonly IPaymentStrategy _strategy;

    public PaymentContext(IPaymentStrategy strategy)
    {
        _strategy = strategy;
    }

    public string Checkout(decimal amount)
    {
        return _strategy.Pay(amount);
    }
}
