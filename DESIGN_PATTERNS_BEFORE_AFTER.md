# 10 Design Patterns (Trước/Sau khi áp dụng)

Tài liệu này mô tả 10 Design Patterns trong project, theo cấu trúc:

- Trước khi áp dụng "... Pattern"
- Đoạn code trước khi áp dụng
- Giải thích (biện luận)
- Sau khi áp dụng "... Pattern"
- Đoạn code sau khi áp dụng
- Giải thích (biện luận)

> Lưu ý: Phần "trước khi áp dụng" là đoạn code **minh hoạ** mô phỏng cách làm thông thường khi chưa áp dụng pattern (vì dự án không lưu lịch sử code theo thời gian).

---

## 1) Singleton Pattern

### Trước khi áp dụng "Singleton Pattern"
```csharp
public static class AppConfig
{
    public static string AppName = "TechStore";
    public static string Company = "TechStore Co.";
}

public class HomeController
{
    public string Title() => AppConfig.AppName;
}
```

**Giải thích (biện luận)**
- Dùng `static` đơn giản nhưng khó mở rộng (khó inject/test), không kiểm soát khởi tạo/lazy-load.
- Khi cấu hình phức tạp hơn, dễ phát sinh phụ thuộc “cứng” vào biến tĩnh.

### Sau khi áp dụng "Singleton Pattern"
```csharp
public sealed class AppConfigManager
{
    private static readonly Lazy<AppConfigManager> lazy = new(() => new AppConfigManager());
    public static AppConfigManager Instance => lazy.Value;

    private AppConfigManager() { }

    public string AppName { get; set; } = "TechStore";
    public string Company { get; set; } = "TechStore Co.";
}
```
Nguồn: [AppConfigManager.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Singleton/AppConfigManager.cs)

**Giải thích (biện luận)**
- Đảm bảo chỉ có **1 instance** cấu hình toàn cục.
- `Lazy<T>` giúp khởi tạo **lười** (chỉ tạo khi cần) và thread-safe.

---

## 2) Factory Method Pattern

### Trước khi áp dụng "Factory Method Pattern"
```csharp
var product = new Product
{
    CategoryId = 1,
    Name = name,
    Price = price,
    StockQuantity = stock,
    Description = desc
};
_dbContext.Products.Add(product);
```

**Giải thích (biện luận)**
- Mỗi nơi tạo `Product` đều phải tự set thuộc tính → dễ sai/thiếu.
- Khi cách khởi tạo thay đổi, phải sửa ở nhiều nơi.

### Sau khi áp dụng "Factory Method Pattern"
```csharp
public interface IProductFactory
{
    Product CreateProduct(int categoryId, string name, decimal price, int stockQuantity, string description);
}

public class ProductFactory : IProductFactory
{
    public Product CreateProduct(int categoryId, string name, decimal price, int stockQuantity, string description)
    {
        return new Product
        {
            CategoryId = categoryId,
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            Description = description
        };
    }
}
```
Nguồn: [IProductFactory.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Factory/IProductFactory.cs)

**Giải thích (biện luận)**
- Gom logic tạo `Product` về 1 chỗ (tập trung, dễ bảo trì).
- Mở rộng dễ: nếu thêm logic (ví dụ default values, validate), chỉ sửa trong factory.

---

## 3) Builder Pattern

### Trước khi áp dụng "Builder Pattern"
```csharp
var order = new Order
{
    CustomerName = customerName,
    ShippingAddress = shippingAddress,
    PaymentMethod = paymentMethod,
};

foreach (var item in items)
{
    order.Items.Add(new OrderItem
    {
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice
    });
}

order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Quantity);
```

**Giải thích (biện luận)**
- Khởi tạo `Order` nhiều bước, dễ bị quên bước tính `TotalAmount`.
- Khi có thêm bước mới (tính phí, khuyến mãi...), code càng dài và khó đọc.

### Sau khi áp dụng "Builder Pattern"
```csharp
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
```
Nguồn: [OrderBuilder.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Builder/OrderBuilder.cs)

**Giải thích (biện luận)**
- Chuỗi lệnh builder giúp code rõ ràng, đảm bảo các bước (đặc biệt là tính tổng).
- Dễ mở rộng thêm bước xây dựng mà không làm rối controller.

---

## 4) Facade Pattern

### Trước khi áp dụng "Facade Pattern"
```csharp
foreach (var item in order.Items)
{
    var product = _dbContext.Products.Find(item.ProductId);
    if (product == null) throw new Exception("Không tồn tại");
    if (product.StockQuantity < item.Quantity) throw new Exception("Không đủ kho");
}

order.Status = OrderStatus.Pending;
order.CreatedAt = DateTime.UtcNow;
_dbContext.Orders.Add(order);
_dbContext.SaveChanges();

foreach (var item in order.Items)
{
    var product = _dbContext.Products.Find(item.ProductId);
    product.StockQuantity -= item.Quantity;
}
_dbContext.SaveChanges();
```

**Giải thích (biện luận)**
- Logic checkout bị “tràn” vào controller (kiểm kho, lưu order, trừ kho...).
- Dễ sai thứ tự xử lý và khó tái sử dụng.

### Sau khi áp dụng "Facade Pattern"
```csharp
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
        foreach (var item in order.Items)
        {
            var product = _dbContext.Products.Find(item.ProductId);
            if (product == null) throw new InvalidOperationException($"Product {item.ProductId} không tồn tại.");
            if (product.StockQuantity <= 0) throw new InvalidOperationException($"Sản phẩm {product.Name} đã hết hàng.");
            if (product.StockQuantity < item.Quantity) throw new InvalidOperationException($"Sản phẩm {product.Name} không đủ tồn kho.");
        }

        order.Status = OrderStatus.Pending;
        order.CreatedAt = DateTime.UtcNow;

        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        _inventoryObserver.OnOrderCreated(order);
    }
}
```
Nguồn: [CheckoutFacade.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Facade/CheckoutFacade.cs)

**Giải thích (biện luận)**
- Controller chỉ cần gọi 1 hàm `PlaceOrder` → controller mỏng.
- Facade gom các bước phức tạp thành API đơn giản và nhất quán.

---

## 5) Strategy Pattern

### Trước khi áp dụng "Strategy Pattern"
```csharp
string result;
if (paymentMethod == PaymentMethod.COD)
{
    result = $"Thanh toán tiền mặt: {amount:C}";
}
else
{
    result = $"Thanh toán chuyển khoản: {amount:C}";
}
```

**Giải thích (biện luận)**
- `if/else` sẽ phình to nếu thêm nhiều phương thức thanh toán.
- Khó mở rộng mà không sửa logic cũ.

### Sau khi áp dụng "Strategy Pattern"
```csharp
public interface IPaymentStrategy
{
    string Pay(decimal amount);
}

public class CashPaymentStrategy : IPaymentStrategy
{
    public string Pay(decimal amount) => $"Thanh toán tiền mặt: {amount:C}";
}

public class BankTransferStrategy : IPaymentStrategy
{
    public string Pay(decimal amount) => $"Thanh toán chuyển khoản: {amount:C}";
}

public class PaymentContext
{
    private readonly IPaymentStrategy _strategy;
    public PaymentContext(IPaymentStrategy strategy) { _strategy = strategy; }
    public string Checkout(decimal amount) => _strategy.Pay(amount);
}
```
Nguồn: [PaymentStrategy.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Strategy/PaymentStrategy.cs)

**Giải thích (biện luận)**
- Mỗi phương thức thanh toán là 1 strategy riêng.
- Thêm phương thức mới chỉ cần thêm class mới implement `IPaymentStrategy`.

---

## 6) Observer Pattern

### Trước khi áp dụng "Observer Pattern"
```csharp
_dbContext.Orders.Add(order);
_dbContext.SaveChanges();

foreach (var item in order.Items)
{
    var product = _dbContext.Products.Find(item.ProductId);
    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
}
_dbContext.SaveChanges();
```

**Giải thích (biện luận)**
- Logic “trừ kho” gắn cứng vào nơi tạo order.
- Sau này thêm xử lý khác khi tạo order (log, gửi email...) sẽ làm checkout rối.

### Sau khi áp dụng "Observer Pattern"
```csharp
public interface IOrderObserver
{
    void OnOrderCreated(Order order);
}

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
}
```
Nguồn: [IOrderObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/IOrderObserver.cs), [InventoryObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/InventoryObserver.cs)

**Giải thích (biện luận)**
- Tách hành vi sau khi tạo đơn (trừ kho) thành observer.
- Có thể thêm nhiều observer khác mà không làm phình checkout.

---

## 7) Iterator Pattern

### Trước khi áp dụng "Iterator Pattern"
```csharp
for (var i = 0; i < products.Count; i++)
{
    var p = products[i];
    Console.WriteLine(p.Name);
}
```

**Giải thích (biện luận)**
- Vẫn duyệt được, nhưng việc “cách duyệt” và “cấu trúc lưu trữ” gắn chặt nhau.

### Sau khi áp dụng "Iterator Pattern"
```csharp
public interface IIterator<T>
{
    bool HasNext();
    T Next();
}

public class ProductIterator : IIterator<Product>
{
    private readonly IList<Product> _products;
    private int _current = 0;

    public ProductIterator(IList<Product> products)
    {
        _products = products;
    }

    public bool HasNext() => _current < _products.Count;

    public Product Next()
    {
        return _products[_current++];
    }
}
```
Nguồn: [IIterator.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Iterator/IIterator.cs)

**Giải thích (biện luận)**
- Tách logic duyệt ra khỏi collection.
- Sau này đổi cách duyệt (lọc, phân trang...) có thể triển khai iterator khác.

---

## 8) State Pattern

### Trước khi áp dụng "State Pattern"
```csharp
if (order.Status == OrderStatus.Pending)
{
    label = "Chờ thanh toán";
}
else if (order.Status == OrderStatus.Shipping)
{
    label = "Đang giao hàng";
}
else if (order.Status == OrderStatus.Completed)
{
    label = "Hoàn thành";
}
else
{
    label = "Đã hủy";
}
```

**Giải thích (biện luận)**
- Điều kiện tăng dần theo số trạng thái.
- Logic chuyển trạng thái (next) dễ bị rải rác.

### Sau khi áp dụng "State Pattern"
```csharp
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

    public void MoveNext()
    {
        _state = _state.NextState();
    }
}
```
Nguồn: [IOrderState.cs](file:///c:/Users/DELL/CuoiKy/Patterns/State/IOrderState.cs)

**Giải thích (biện luận)**
- Mỗi trạng thái là 1 class riêng, gom logic in/next.
- Dễ thêm trạng thái mới và điều chỉnh luồng chuyển trạng thái.

---

## 9) Decorator Pattern

### Trước khi áp dụng "Decorator Pattern"
```csharp
decimal total = order.TotalAmount;

if (giftWrap)
{
    total += 15m;
}

if (expressShipping)
{
    total += 25m;
}
```

**Giải thích (biện luận)**
- Nếu thêm nhiều loại phí (gói quà, bảo hiểm, ưu tiên...) sẽ dài và khó kiểm soát.

### Sau khi áp dụng "Decorator Pattern"
```csharp
public abstract class OrderComponent
{
    public abstract decimal GetTotal();
}

public class BasicOrderComponent : OrderComponent
{
    private readonly Order _order;
    public BasicOrderComponent(Order order) { _order = order; }
    public override decimal GetTotal() => _order.TotalAmount;
}

public abstract class OrderDecorator : OrderComponent
{
    protected readonly OrderComponent _component;
    public OrderDecorator(OrderComponent component) { _component = component; }
}

public class GiftWrapDecorator : OrderDecorator
{
    public GiftWrapDecorator(OrderComponent component) : base(component) { }
    public override decimal GetTotal() => _component.GetTotal() + 15m;
}

public class ExpressShippingDecorator : OrderDecorator
{
    public ExpressShippingDecorator(OrderComponent component) : base(component) { }
    public override decimal GetTotal() => _component.GetTotal() + 25m;
}
```
Nguồn: [OrderComponent.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Decorator/OrderComponent.cs)

**Giải thích (biện luận)**
- Ghép “phí” theo nhu cầu bằng cách bọc decorator.
- Có thể kết hợp nhiều decorator linh hoạt mà không sửa class gốc.

---

## 10) Adapter Pattern

### Trước khi áp dụng "Adapter Pattern"
```csharp
<img src="https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=TechStoreOrder_@Model.Id" />
```

**Giải thích (biện luận)**
- View phụ thuộc trực tiếp vào format URL của nhà cung cấp QR.
- Nếu đổi nhà cung cấp, phải sửa ở nhiều nơi (view/controller).

### Sau khi áp dụng "Adapter Pattern"

**Adapter + interface chuẩn hoá**
```csharp
public interface IQrCodeGenerator
{
    string BuildImageUrl(string data, int size);
}

public class QrServerClient
{
    public string Create(string data, int size)
    {
        var encoded = Uri.EscapeDataString(data);
        return $"https://api.qrserver.com/v1/create-qr-code/?size={size}x{size}&data={encoded}";
    }
}

public class QrServerQrCodeGenerator : IQrCodeGenerator
{
    private readonly QrServerClient _client;

    public QrServerQrCodeGenerator(QrServerClient client)
    {
        _client = client;
    }

    public string BuildImageUrl(string data, int size)
    {
        return _client.Create(data, size);
    }
}
```
Nguồn: [QrCodeAdapter.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Adapter/QrCodeAdapter.cs)

**Sử dụng trong controller và view**
```csharp
if (order.PaymentMethod == PaymentMethod.BankTransfer)
{
    ViewBag.QrUrl = _qrCodeGenerator.BuildImageUrl($"TechStoreOrder_{order.Id}", 250);
}
```
Nguồn: [CartController.cs](file:///c:/Users/DELL/CuoiKy/Controllers/CartController.cs)

```csharp
<img src="@ViewBag.QrUrl" alt="QR Code Thanh Toán" />
```
Nguồn: [OrderSuccess.cshtml](file:///c:/Users/DELL/CuoiKy/Views/Cart/OrderSuccess.cshtml)

**Giải thích (biện luận)**
- View chỉ biết `QrUrl`, không phụ thuộc vào chi tiết nhà cung cấp.
- Nếu đổi provider QR, chỉ cần thay implementation của `IQrCodeGenerator`.

---
