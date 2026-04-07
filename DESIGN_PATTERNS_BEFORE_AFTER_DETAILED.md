# 10 Design Patterns (Giải thích rất chi tiết theo từng dòng)

Tài liệu này mở rộng từ `DESIGN_PATTERNS_BEFORE_AFTER.md` theo yêu cầu: giải thích **kỹ và chi tiết theo từng dòng code**, viết theo kiểu đoạn văn.

> Lưu ý quan trọng: phần "Trước khi áp dụng" là **code minh hoạ** mô phỏng cách làm phổ biến trước khi dùng pattern (vì dự án không có lịch sử version). Phần "Sau khi áp dụng" bám theo **code thật trong project**.

***

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

**Giải thích rất chi tiết**

Ở dòng `public static class AppConfig`, ta tạo một class tĩnh để chứa cấu hình. Cách này dễ làm vì không cần khởi tạo object, nhưng cũng đồng nghĩa là mọi nơi trong hệ thống đều có thể truy cập trực tiếp và sửa dữ liệu cấu hình, dẫn tới khó kiểm soát vòng đời và khó test.

Khối `{ ... }` sau đó chỉ là phạm vi của class. Dòng `public static string AppName = "TechStore";` khai báo biến cấu hình `AppName` toàn cục, truy cập trực tiếp bằng `AppConfig.AppName`. `public static string Company ...` tương tự.

Phần `public class HomeController` là ví dụ nơi sử dụng. Trong `Title() => AppConfig.AppName;`, toán tử `=>` nghĩa là phương thức trả về luôn giá trị ở vế phải. Điểm yếu ở đây: controller phụ thuộc “cứng” vào `AppConfig`, khiến bạn không thể thay cấu hình bằng cách inject mock khi test.

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

**Giải thích rất chi tiết**

`public sealed class AppConfigManager` dùng `sealed` để chặn kế thừa, tránh việc subclass phá vỡ tính “một instance duy nhất”.

`private static readonly Lazy<AppConfigManager> lazy = new(() => new AppConfigManager());` là điểm quan trọng nhất. Ở đây:

- `static` nghĩa là thuộc về class, không thuộc về object cụ thể.
- `readonly` nghĩa là sau khi khởi tạo thì không gán lại biến `lazy` nữa.
- `Lazy<T>` là wrapper giúp tạo object theo kiểu **lazy initialization**: chỉ khi nào ai đó cần mới tạo instance.
- `new(() => new AppConfigManager())` truyền vào một lambda (hàm ẩn danh) để nói “khi cần thì tạo AppConfigManager bằng constructor private”.

`public static AppConfigManager Instance => lazy.Value;` cung cấp điểm truy cập duy nhất. Khi ai gọi `AppConfigManager.Instance`, nó sẽ lấy `lazy.Value`, lúc này `Lazy` sẽ tạo instance nếu chưa có.

`private AppConfigManager() { }` khóa constructor, đảm bảo bên ngoài không thể `new AppConfigManager()`.

Hai dòng property `AppName` và `Company` là cấu hình, viết theo property để dễ thêm logic (validation, notify, load from env) trong tương lai.

***

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

**Giải thích rất chi tiết**

`var product = new Product { ... }` tạo object `Product` theo kiểu object initializer. Dòng `CategoryId = 1` hard-code category ID, rất dễ sai nếu DB thay đổi. Các dòng `Name = name`, `Price = price`, `StockQuantity = stock`, `Description = desc` phụ thuộc vào biến cục bộ bên ngoài.

Sau đó `_dbContext.Products.Add(product);` thêm entity vào tracking của EF Core. Vấn đề: logic “tạo Product chuẩn” bị rải rác ở nhiều nơi. Nếu sau này bạn muốn auto set một số thuộc tính (ví dụ `CreatedAt`) hoặc validate chuẩn hơn, bạn phải sửa ở nhiều nơi.

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

**Giải thích rất chi tiết**

`public interface IProductFactory` định nghĩa “hợp đồng” cho việc tạo Product. Khi code gọi factory, nó không quan tâm bên dưới tạo như thế nào.

`Product CreateProduct(...)` là “factory method” (phương thức tạo). Các tham số được định nghĩa đầy đủ để bắt buộc bên gọi cung cấp dữ liệu cần thiết.

`public class ProductFactory : IProductFactory` là implement cụ thể. Trong `CreateProduct`, phần `return new Product { ... }` gom toàn bộ logic khởi tạo vào một chỗ. Điều này tạo lợi thế:

- Nếu cần set default hoặc validate, bạn thêm ở đây.
- Nếu sau này có nhiều loại Product khác nhau (ví dụ DigitalProduct, PhysicalProduct), bạn có thể mở rộng bằng nhiều factory khác nhau.

***

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

**Giải thích rất chi tiết**

Đoạn này tạo `Order` trước, rồi `foreach` để thêm `OrderItem`, cuối cùng tính tổng `TotalAmount`. Vấn đề thường gặp là bạn có thể quên dòng tính tổng, hoặc tính sai nếu sau này thêm phí/giảm giá. Ngoài ra, nếu muốn thêm bước mới (ví dụ set `CreatedAt`, set status, validate), code sẽ dài và trộn nhiều trách nhiệm.

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

**Giải thích rất chi tiết**

`private readonly Order _order = new();` tạo ra order nội bộ, chỉ builder quản lý. `private decimal _total;` dùng để tích luỹ tổng tiền một cách nhất quán.

Các method `SetCustomerName`, `SetShippingAddress`, `SetPaymentMethod` đều trả về `this`. Đây là kỹ thuật fluent API, giúp bạn gọi nối chuỗi: `new OrderBuilder().SetCustomerName(...).SetShippingAddress(...).Build()`.

`AddItem(Product product, int quantity)` tạo `OrderItem` từ product thật. Việc set `ProductId`, `UnitPrice` dựa trên `product.Price` giúp tránh việc bên ngoài tự truyền unit price sai. Sau đó `_total += item.TotalPrice;` đảm bảo tổng tiền được cập nhật ngay khi thêm item.

`Build()` là bước “kết thúc”. Ở đây builder set `TotalAmount` từ `_total` rồi trả order hoàn chỉnh.

***

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

**Giải thích rất chi tiết**

Đây là một luồng checkout “đầy đủ bước” nhưng đặt trực tiếp trong controller/service, khiến nơi gọi phải biết quá nhiều: kiểm kho, lưu order, trừ kho. Nếu muốn tái sử dụng luồng checkout ở chỗ khác, bạn lại phải copy logic. Nếu muốn thay đổi quy trình (ví dụ thêm log, gửi email), code sẽ bị phình ra.

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

**Giải thích rất chi tiết**

`CheckoutFacade` là lớp đóng vai trò “mặt tiền” cho quy trình checkout. Người gọi chỉ cần gọi `PlaceOrder(order)`.

Hai field `_dbContext` và `_inventoryObserver` được inject qua constructor. Điều này làm lớp dễ test hơn và tách bạch trách nhiệm.

Trong `PlaceOrder`, vòng `foreach` đầu tiên là validation tồn kho. Ta dùng `InvalidOperationException` để báo lỗi nghiệp vụ.

Sau khi validate, facade set `order.Status` và `CreatedAt`, rồi lưu order vào DB. Cuối cùng gọi `_inventoryObserver.OnOrderCreated(order)` để trừ kho theo kiểu Observer (tách hành vi hậu xử lý).

***

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

**Giải thích rất chi tiết**

Logic thanh toán nằm trong `if/else`. Khi thêm phương thức mới (ví dụ MoMo, VNPay, PayPal), khối điều kiện sẽ dài và dễ sai.

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

**Giải thích rất chi tiết**

`IPaymentStrategy` là “hợp đồng” cho mọi chiến lược thanh toán. Mỗi class cụ thể (`CashPaymentStrategy`, `BankTransferStrategy`) chỉ tập trung vào cách tính/biểu diễn thanh toán.

`PaymentContext` nhận một `IPaymentStrategy` qua constructor. Như vậy, tại runtime bạn chọn strategy theo lựa chọn người dùng, rồi gọi `Checkout(amount)` để thực thi.

***

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

**Giải thích rất chi tiết**

Ở đây việc trừ kho được viết chung với quy trình tạo order. Nếu thêm hành vi khác sau tạo order (log, email, analytics) thì checkout sẽ ngày càng phình.

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

**Giải thích rất chi tiết**

`IOrderObserver` định nghĩa một điểm móc (hook) `OnOrderCreated`. `InventoryObserver` triển khai hook này để trừ tồn kho.

Trong `OnOrderCreated`, mỗi item được xử lý độc lập. Dùng `Math.Max(0, ...)` để tránh tồn kho âm. Cuối cùng `SaveChanges()` commit thay đổi.

***

## 7) Iterator Pattern

### Trước khi áp dụng "Iterator Pattern"

```csharp
for (var i = 0; i < products.Count; i++)
{
    var p = products[i];
    Console.WriteLine(p.Name);
}
```

**Giải thích rất chi tiết**

Vòng for phụ thuộc vào việc collection là loại có `Count` và truy cập theo index. Nếu collection thay đổi kiểu (ví dụ stream/pagination) thì code duyệt thay đổi theo.

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

**Giải thích rất chi tiết**

Interface `IIterator<T>` quy chuẩn hoá thao tác duyệt: hỏi còn phần tử không (`HasNext`) và lấy phần tử tiếp theo (`Next`). `ProductIterator` giữ `_current` để nhớ vị trí hiện tại.

`HasNext()` so sánh `_current` với `_products.Count`. `Next()` trả phần tử tại index `_current` rồi tăng `_current` lên 1.

***

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

**Giải thích rất chi tiết**

Nếu chỉ để hiển thị label thì if/else ổn. Nhưng khi thêm logic chuyển trạng thái, xử lý theo state, bạn sẽ phải rải điều kiện ở nhiều nơi.

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

**Giải thích rất chi tiết**

Mỗi state (Pending/Shipping/Completed/Cancelled) trở thành một class riêng và tự biết state tiếp theo. Context giữ một `_state` hiện tại; khi gọi `MoveNext()` nó chuyển `_state` sang `_state.NextState()`.

***

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

**Giải thích rất chi tiết**

Đây là cách cộng phí “thẳng” bằng biến `total`. Nếu thêm nhiều tuỳ chọn, bạn sẽ có rất nhiều `if` và khó kết hợp linh hoạt.

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

**Giải thích rất chi tiết**

`OrderComponent` là interface/abstract base cho việc tính tổng. `BasicOrderComponent` bọc `Order` gốc và trả `TotalAmount`.

`OrderDecorator` giữ tham chiếu `_component` để có thể bọc chồng. `GiftWrapDecorator` và `ExpressShippingDecorator` chỉ override `GetTotal()` bằng cách gọi `_component.GetTotal()` rồi cộng thêm phí. Nhờ đó bạn có thể kết hợp: `new ExpressShippingDecorator(new GiftWrapDecorator(new BasicOrderComponent(order)))`.

***

## 10) Adapter Pattern

### Trước khi áp dụng "Adapter Pattern"

```csharp
<img src="https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=TechStoreOrder_@Model.Id" />
```

**Giải thích rất chi tiết**

View đang phụ thuộc trực tiếp vào URL format của `qrserver.com`. Nếu đổi sang nhà cung cấp khác hoặc thay querystring, bạn phải sửa trực tiếp trong view.

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

**Giải thích rất chi tiết**

`IQrCodeGenerator` là giao diện nội bộ của hệ thống, giúp phần còn lại của app chỉ biết “tôi cần một URL ảnh QR từ data và size”.

`QrServerClient` mô phỏng một thư viện/SDK bên thứ ba. Nó có method `Create` trả về URL đúng format của provider.

`QrServerQrCodeGenerator` là adapter: nó “dịch” từ giao diện nội bộ (`BuildImageUrl`) sang lời gọi cụ thể của provider (`_client.Create`). Khi bạn đổi provider, bạn chỉ cần viết adapter mới implement `IQrCodeGenerator`.

***

## Ghi chú

- Nếu bạn muốn tôi giải thích **siêu chi tiết đúng từng dòng** theo format “Dòng 1…, Dòng 2…” (kèm đánh số dòng), nói rõ pattern nào trước (ví dụ chỉ Singleton + Factory trước), tôi sẽ viết thêm một bản khác theo đúng format đó.

