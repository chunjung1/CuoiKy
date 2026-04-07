# Script quay video review 10 Design Patterns (bản dài, nói chi tiết)

Tài liệu này là **kịch bản nói (voice-over)** + **hướng dẫn thao tác mở file** để bạn quay video review dự án TechStore đã áp dụng các Design Patterns.

- Mục tiêu: nói **chi tiết** nhưng vẫn mạch lạc, có dẫn chứng file code.
- Gợi ý độ dài: ~8–12 phút.
- Cách quay: vừa nói vừa mở file trong IDE theo đúng thứ tự.

---

## 0) Mở đầu (30–45s)

**Bạn nói**

“Xin chào thầy/cô, em là … Hôm nay em xin trình bày đồ án TechStore (ASP.NET Core MVC) và tập trung review phần **áp dụng 10 Design Patterns** trong dự án.

Trong video này, em sẽ đi lần lượt từng pattern, trả lời 3 câu hỏi:

1) Pattern dùng để giải quyết vấn đề gì?
2) Trong dự án, em đặt pattern ở đâu (file nào)?
3) Nó giúp code tốt hơn như thế nào (dễ bảo trì/mở rộng/tách trách nhiệm)?

Sau mỗi phần em sẽ chỉ rõ file code tương ứng để thầy/cô kiểm tra.”

**Bạn thao tác**
- Mở thư mục `Patterns/` trong IDE để cho thấy project đã gom theo từng pattern.

Thư mục patterns hiện tại:
- `Patterns/Singleton/`
- `Patterns/Factory/`
- `Patterns/Builder/`
- `Patterns/Facade/`
- `Patterns/Observer/`
- `Patterns/Strategy/`
- `Patterns/State/`
- `Patterns/Iterator/`
- `Patterns/Decorator/`
- `Patterns/Adapter/`

---

## 1) Singleton Pattern (45–60s)

**Bạn nói**

“Đầu tiên là **Singleton Pattern**. Ý tưởng chính của Singleton là đảm bảo chỉ tồn tại **một instance** trong toàn hệ thống cho một đối tượng có tính ‘toàn cục’, thường dùng cho config/manager.

Trong dự án, em dùng Singleton cho `AppConfigManager` để mô phỏng một đối tượng quản lý cấu hình dùng chung.”

**Bạn mở file**
- [AppConfigManager.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Singleton/AppConfigManager.cs)

**Bạn nói chi tiết**

“Ở đây em dùng `Lazy<T>` để khởi tạo theo kiểu lazy initialization. Nghĩa là lúc nào cần thì mới tạo instance, và `Lazy<T>` cũng thread-safe trong ngữ cảnh phổ biến.

Điểm quan trọng của Singleton là constructor được đặt `private`, để không thể `new` từ bên ngoài. Bên ngoài chỉ truy cập qua `Instance`. Nhờ vậy hệ thống không bị tạo nhiều config manager khác nhau gây lệch dữ liệu.”

---

## 2) Factory Pattern (60–75s)

**Bạn nói**

“Tiếp theo là **Factory Pattern**. Factory giải quyết vấn đề: nếu việc tạo object nằm rải rác ở nhiều nơi, khi thay đổi cách khởi tạo sẽ phải sửa rất nhiều chỗ.

Factory gom logic tạo object vào một nơi, giúp code thống nhất, dễ mở rộng.”

**Bạn mở file**
- [IProductFactory.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Factory/IProductFactory.cs)

**Bạn nói chi tiết**

“Ở đây em có interface `IProductFactory` và class `ProductFactory`. Khi cần tạo `Product`, thay vì viết `new Product { ... }` ở nhiều nơi, em gọi factory.

Lợi ích: nếu sau này `Product` cần thêm default value hoặc kiểm tra dữ liệu, em chỉ sửa trong factory, không phải sửa nhiều controller.”

---

## 3) Builder Pattern (75–100s)

**Bạn nói**

“Thứ ba là **Builder Pattern**. Builder phù hợp khi tạo object phức tạp nhiều bước.

Ví dụ `Order` có nhiều `OrderItem`, tổng tiền phải tính đúng, và có nhiều thông tin như người nhận, địa chỉ, phương thức thanh toán… Nếu tạo trực tiếp trong controller dễ rối và dễ quên bước.”

**Bạn mở file**
- [OrderBuilder.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Builder/OrderBuilder.cs)

**Bạn nói chi tiết**

“Builder tách từng bước tạo order thành các method `Set...` và `AddItem`. Mỗi method trả về `this` để gọi theo kiểu fluent.

Điểm mạnh nằm ở `Build()` — đây là bước chốt, đảm bảo order tạo ra là ‘đúng và đủ’, ví dụ `TotalAmount` được cập nhật theo item.

Về mặt thiết kế, Builder giúp controller sạch hơn: controller chỉ thu thập input, còn logic dựng Order nằm trong builder.”

---

## 4) Facade Pattern (100–140s)

**Bạn nói**

“Thứ tư là **Facade Pattern**. Facade giống như ‘mặt tiền’ cho một quy trình phức tạp. Thay vì controller phải biết từng bước: kiểm kho, tạo order, lưu DB, trừ kho…, controller chỉ cần gọi một hàm duy nhất.

Trong dự án, em áp dụng Facade cho quy trình checkout/đặt hàng.”

**Bạn mở file**
- [CheckoutFacade.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Facade/CheckoutFacade.cs)

**Bạn nói chi tiết**

“Trong `PlaceOrder`, facade làm các bước:

- Kiểm tra sản phẩm tồn tại.
- Kiểm tra tồn kho, trong đó có xử lý trường hợp hết hàng.
- Set trạng thái ban đầu của order.
- Lưu order vào database.
- Sau đó kích hoạt observer để trừ kho.

Nhờ facade, controller không còn chứa logic nghiệp vụ phức tạp. Khi cần sửa luồng checkout, em chỉ sửa trong facade.”

---

## 5) Observer Pattern (140–180s)

**Bạn nói**

“Thứ năm là **Observer Pattern**. Observer dùng khi có một sự kiện xảy ra và nhiều hành vi ‘đi kèm’ sau đó.

Trong dự án, sự kiện là: **tạo order thành công**. Hành vi đi kèm là: **trừ tồn kho**.

Thay vì đặt trừ kho trực tiếp trong checkout (làm checkout phình to), em tách ra observer.”

**Bạn mở file**
- [IOrderObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/IOrderObserver.cs)
- [InventoryObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/InventoryObserver.cs)

**Bạn nói chi tiết**

“`IOrderObserver` định nghĩa hook `OnOrderCreated(Order order)`.

`InventoryObserver` implement hook này và thực hiện:

- Duyệt từng item trong order.
- Lấy product từ DB.
- Giảm `StockQuantity`.
- Lưu thay đổi.

Ưu điểm: nếu sau này muốn thêm hành vi khác sau khi tạo đơn như gửi email, log analytics…, em chỉ cần viết observer mới mà không sửa nhiều ở checkout.”

---

## 6) Strategy Pattern (180–220s)

**Bạn nói**

“Thứ sáu là **Strategy Pattern**. Strategy dùng để thay đổi ‘thuật toán’ theo ngữ cảnh.

Một ví dụ điển hình là các phương thức thanh toán: cash, bank transfer, hoặc sau này có thể thêm VNPay, MoMo… Nếu dùng if/else sẽ phình code.

Strategy tách mỗi kiểu xử lý thành một class riêng implement cùng interface.”

**Bạn mở file**
- [PaymentStrategy.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Strategy/PaymentStrategy.cs)

**Bạn nói chi tiết**

“Ở đây em có `IPaymentStrategy` và các implementation như `CashPaymentStrategy`, `BankTransferStrategy`.

`PaymentContext` nhận strategy tương ứng và gọi `Pay(amount)`. Khi thêm phương thức mới, chỉ cần thêm class mới implement interface, không đụng code cũ nhiều.”

---

## 7) State Pattern (220–260s)

**Bạn nói**

“Thứ bảy là **State Pattern**. State dùng khi hành vi của một đối tượng phụ thuộc vào trạng thái hiện tại.

Trong bài toán đơn hàng, trạng thái có thể là: Pending, Paid, Shipping, Completed, Canceled.

Nếu xử lý bằng if/else, logic sẽ rải rác và khó bảo trì khi thêm trạng thái mới.”

**Bạn mở file**
- [IOrderState.cs](file:///c:/Users/DELL/CuoiKy/Patterns/State/IOrderState.cs)

**Bạn nói chi tiết**

“Ở đây mỗi trạng thái là một class implement `IOrderState`.

Điểm chính của State pattern là mỗi state tự biết:

- Trạng thái hiện tại thể hiện gì (`PrintState`).
- State kế tiếp là gì (`NextState`).

`OrderStateContext` giữ state hiện tại và chuyển state thông qua `MoveNext()`. Nhờ vậy việc mở rộng luồng trạng thái dễ hơn, ít phải sửa if/else.”

---

## 8) Iterator Pattern (260–290s)

**Bạn nói**

“Thứ tám là **Iterator Pattern**. Iterator chuẩn hóa cách duyệt một tập hợp mà không phụ thuộc vào cấu trúc lưu trữ.

Trong dự án, em có interface `IIterator<T>` và iterator cho sản phẩm.”

**Bạn mở file**
- [IIterator.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Iterator/IIterator.cs)

**Bạn nói chi tiết**

“Iterator cung cấp `HasNext()` và `Next()`. Nhờ vậy, phần logic sử dụng iterator chỉ quan tâm ‘còn phần tử không’ và ‘lấy phần tử tiếp theo’, không quan tâm collection là list/array/paged list…

Pattern này giúp khi muốn đổi cách duyệt (lọc, phân trang, lazy load) thì có thể thay iterator khác mà không làm rối code sử dụng.”

---

## 9) Decorator Pattern (290–340s)

**Bạn nói**

“Thứ chín là **Decorator Pattern**. Decorator dùng để mở rộng hành vi của object bằng cách ‘bọc’ (wrap) mà không sửa class gốc.

Ví dụ trong thương mại điện tử, tổng tiền đơn có thể cộng thêm nhiều loại phí/option: gói quà, giao hàng nhanh, bảo hiểm…, và các option có thể kết hợp linh hoạt.”

**Bạn mở file**
- [OrderComponent.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Decorator/OrderComponent.cs)

**Bạn nói chi tiết**

“Decorator có `OrderComponent` làm base.

`BasicOrderComponent` là phần lõi (tổng gốc).

Các decorator như `GiftWrapDecorator`, `ExpressShippingDecorator` override `GetTotal()` bằng cách gọi tổng từ `_component.GetTotal()` rồi cộng thêm phần phụ phí.

Nhờ đó ta có thể bọc nhiều lớp: bọc gói quà rồi bọc giao hàng nhanh, tổng cuối cùng là cộng dồn đúng theo thứ tự bọc.”

---

## 10) Adapter Pattern (340–420s)

**Bạn nói**

“Cuối cùng là **Adapter Pattern**. Adapter dùng khi hệ thống muốn gọi một thành phần bên ngoài (API/SDK) nhưng không muốn phụ thuộc trực tiếp vào interface của bên ngoài.

Ở dự án, em tạo QR code cho chuyển khoản. Nếu view hard-code url QR provider, sau này đổi provider phải sửa nhiều nơi.

Adapter giúp hệ thống chỉ phụ thuộc vào `IQrCodeGenerator` do mình định nghĩa.”

**Bạn mở file**
- [IQrCodeGenerator.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Adapter/IQrCodeGenerator.cs)

**Bạn nói chi tiết**

“Trong file này em định nghĩa interface `IQrCodeGenerator` và implement cụ thể.

Controller chỉ cần gọi `_qrCodeGenerator.BuildImageUrl(data, size)` để nhận URL QR.

Nếu đổi sang provider khác, em viết một adapter khác implement `IQrCodeGenerator` mà không cần sửa view/controller nhiều.”

**Bạn gợi ý thao tác thêm (nếu muốn minh hoạ QR đang dùng adapter)**
- Mở nơi controller set QR url:
  - `Controllers/CartController.cs` (tìm `QrUrl`)
- Mở view hiển thị QR:
  - `Views/Cart/OrderSuccess.cshtml`

---

## 11) Chốt lại – pattern nào giúp phần nào trong dự án? (45–60s)

**Bạn nói**

“Để tổng kết nhanh:

- **Facade + Observer** giúp luồng đặt hàng rõ ràng: Facade gom quy trình, Observer tách hậu xử lý trừ kho.
- **Factory + Builder** giúp tạo object thống nhất và có cấu trúc.
- **Strategy** giúp mở rộng phương thức thanh toán.
- **State** giúp quản lý luồng trạng thái đơn hàng.
- **Adapter** giúp tích hợp QR provider mà không phụ thuộc cứng.
- **Iterator/Decorator/Singleton** là các pattern bổ trợ để làm code dễ tổ chức và mở rộng.

Nhờ vậy code dễ maintain, dễ mở rộng, và dễ chứng minh tiêu chí pattern của đồ án.”

---

## 12) Kết thúc (10–15s)

**Bạn nói**

“Trên đây là phần review 10 Design Patterns trong dự án TechStore. Em xin cảm ơn thầy/cô đã theo dõi.”

---

## Danh sách file pattern để bạn mở nhanh

- Singleton: [AppConfigManager.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Singleton/AppConfigManager.cs)
- Factory: [IProductFactory.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Factory/IProductFactory.cs)
- Builder: [OrderBuilder.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Builder/OrderBuilder.cs)
- Facade: [CheckoutFacade.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Facade/CheckoutFacade.cs)
- Observer: [IOrderObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/IOrderObserver.cs), [InventoryObserver.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Observer/InventoryObserver.cs)
- Strategy: [PaymentStrategy.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Strategy/PaymentStrategy.cs)
- State: [IOrderState.cs](file:///c:/Users/DELL/CuoiKy/Patterns/State/IOrderState.cs)
- Iterator: [IIterator.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Iterator/IIterator.cs)
- Decorator: [OrderComponent.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Decorator/OrderComponent.cs)
- Adapter: [IQrCodeGenerator.cs](file:///c:/Users/DELL/CuoiKy/Patterns/Adapter/IQrCodeGenerator.cs)

