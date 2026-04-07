# Tổng hợp quá trình thực hiện dự án TechStore (ASP.NET Core MVC)

Tài liệu này tổng hợp các hạng mục đã được triển khai trong project **TechStore** từ đầu đến hiện tại, bao gồm các thay đổi chức năng, giao diện, database/migration và các module dành cho **User** và **Admin**.

---

## 1) Tổng quan công nghệ

- **Framework**: ASP.NET Core MVC (C#)
- **Frontend**: Razor Views (`.cshtml`) + Bootstrap 5 + Font Awesome + JavaScript (fetch/AJAX)
- **Database/ORM**: SQL Server + Entity Framework Core (Migrations)
- **Auth**: Cookie Authentication, phân quyền theo role `Admin`/`Customer`
- **State**: Session (lưu giỏ hàng)

---

## 2) Tối ưu và sửa lỗi nền tảng ban đầu

### 2.1. Cảnh báo decimal precision

- Chuẩn hóa kiểu `decimal` (giá sản phẩm) để tránh warning EF Core về precision.
- Thực hiện cấu hình cột decimal theo dạng `decimal(18,2)`.

### 2.2. Vấn đề Migrations không áp dụng

- Phát hiện việc dùng `EnsureCreated()` khiến migrations không còn được áp dụng lên DB.
- Điều chỉnh lại theo hướng ưu tiên migrations.

---

## 3) Frontend trang chủ theo mẫu GearVN (TechStore)

- Thiết kế lại UI trang chủ theo phong cách GearVN, sử dụng Bootstrap.
- Đồng bộ màu header xanh `#008BFF` và đổi branding thành **TechStore**.
- Slider đổi sang dùng ảnh local trong `wwwroot/lib/img/`.

Các file liên quan:

- `Views/Shared/_Layout.cshtml`
- `Views/Home/Index.cshtml`

---

## 4) Đối chiếu 10 Design Patterns theo yêu cầu đồ án

- Tổ chức các pattern trong thư mục `Patterns/`.
- Có triển khai các nhóm pattern theo yêu cầu đề bài (ví dụ: Factory, Facade, Strategy, Observer, ...).

---

## 5) Đăng nhập/Đăng ký + Phân quyền (Admin/Customer)

### 5.1. Role-based Authorization

- Quy ước role gồm 2 mức: `Customer` (mặc định) và `Admin`.
- User mới đăng ký luôn là `Customer`.
- `Admin` được set thủ công trong SQL (theo yêu cầu).

### 5.2. Login bằng Email + Quên mật khẩu

- Bổ sung trường `Email`, `ResetToken`, `ResetTokenExpiry` cho user.
- Login bằng email + mật khẩu.
- Quên mật khẩu tạo token “giả lập” để test (không tích hợp SMTP thật).

Các file liên quan:

- `Models/User.cs`
- `Controllers/AccountController.cs`
- `Views/Account/Login.cshtml`, `Register.cshtml`, `ForgotPassword.cshtml`, `ResetPassword.cshtml`, `AccessDenied.cshtml`

---

## 6) Quản lý sản phẩm (Admin CRUD)

- Admin CRUD sản phẩm: danh sách, tạo, sửa, xóa.
- Upload ảnh sản phẩm vào `wwwroot/images/products`.
- Ràng buộc dữ liệu: tên không rỗng/không ký tự đặc biệt, giá không âm, tồn kho >= 0, mô tả optional.
- Sửa lỗi HTTP 400 do thiếu anti-forgery token bằng cách thêm `@Html.AntiForgeryToken()`.

Các file liên quan:

- `Controllers/ProductController.cs` (admin action)
- `Views/Product/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`
- `Views/Shared/_AdminLayout.cshtml`

---

## 7) Giỏ hàng (Cart) dạng modal + AJAX + Session

### 7.1. Kiến trúc

- Giỏ hàng lưu trong Session với serialize/deserialize.
- Cập nhật giỏ hàng bằng fetch/AJAX, render lại nội dung modal bằng partial view.

### 7.2. Nghiệp vụ

- “Thêm vào giỏ” từ trang ngoài: **luôn cộng dồn +1**.
- “Thay đổi số lượng” chỉ xảy ra khi thao tác trong giỏ hàng.
- Chọn sản phẩm (checkbox) để tính tổng tiền.
- Trang Checkout.

Các file liên quan:

- `ViewModels/CartViewModel.cs`
- `Extensions/SessionExtensions.cs`
- `Controllers/CartController.cs`
- `Views/Shared/_CartPartial.cshtml`
- `Views/Shared/_Layout.cshtml`
- `Views/Cart/Checkout.cshtml`, `Views/Cart/Index.cshtml`

---

## 8) Quy trình đặt hàng + Duyệt đơn (ghi điểm)

### 8.1. Luồng đặt hàng cho khách

- Khi khách đặt hàng:
  - Tạo Order trong DB với trạng thái **Chờ thanh toán (Pending)**.
  - Chuyển sang trang **Đặt hàng thành công**.
- Nếu thanh toán bằng **chuyển khoản**:
  - Trang thành công hiển thị QR + hướng dẫn.

### 8.2. Admin duyệt đơn

- Admin vào “Quản lý đơn hàng”:
  - Xem danh sách
  - Xem chi tiết
  - Xác nhận “đã thu tiền” để đổi trạng thái sang `Paid`
  - Cập nhật trạng thái giao hàng/hoàn thành/hủy

Các file liên quan:

- `Patterns/CheckoutFacade.cs` (Facade)
- `Patterns/IOrderObserver.cs`, `Patterns/InventoryObserver.cs` (Observer)
- `Controllers/CartController.cs` (ProcessCheckout + OrderSuccess)
- `Views/Cart/OrderSuccess.cshtml`
- `Controllers/AdminOrderController.cs`
- `Views/AdminOrder/Index.cshtml`, `Views/AdminOrder/Details.cshtml`

---

## 9) Quản lý người dùng (Admin)

- Admin xem danh sách tất cả users.
- Admin **được phép xóa user Customer**.
- Admin **không thể xóa tài khoản Admin**.
- Xóa thực hiện trực tiếp trên DB.

Các file liên quan:

- `Controllers/AdminUserController.cs`
- `Views/AdminUser/Index.cshtml`
- Menu gắn trong `Views/Shared/_AdminLayout.cshtml`

---

## 10) Thống kê doanh thu (Admin)

- Dashboard thống kê theo khoảng ngày.
- Doanh thu chỉ tính đơn có trạng thái: `Paid`, `Shipping`, `Completed`.
- Doanh thu theo ngày (group `CreatedAt.Date`).
- Top sản phẩm: xếp hạng theo **số lượng mua (SL)**, nếu bằng nhau mới xét doanh thu.

Các file liên quan:

- `Controllers/AdminRevenueController.cs`
- `ViewModels/AdminRevenueViewModel.cs`
- `Views/AdminRevenue/Index.cshtml`

---

## 11) Quản lý loại sản phẩm (Category) + đồng bộ dropdown khi tạo/sửa sản phẩm

### 11.1. Chuyển từ enum sang bảng DB

- Tạo bảng `Categories`.
- `Product` chuyển từ `Category enum` sang `CategoryId` và navigation `Category`.
- Có trang Admin tạo/sửa/xóa loại.
- Khi tạo loại mới thành công, dropdown loại trong form thêm/sửa sản phẩm cập nhật theo DB.

Các file liên quan:

- `Models/Category.cs`, `Models/Product.cs`
- `Data/ApplicationDbContext.cs`
- `Controllers/AdminCategoryController.cs`
- `Views/AdminCategory/Index.cshtml`, `Views/AdminCategory/Edit.cshtml`
- `Controllers/ProductController.cs` + `Views/Product/Create.cshtml`, `Views/Product/Edit.cshtml`

---

## 12) Trang “Sản phẩm” (User) + “Đơn đã mua”

### 12.1. “Sản phẩm” trên header

- Link header “Sản phẩm” dẫn tới trang public `Product/List`.
- Giao diện hiển thị theo từng hàng theo loại (mỗi loại một hàng cuộn ngang) + nút “Xem tất cả” theo từng loại.

### 12.2. “Đơn đã mua”

- Thêm menu “Đơn đã mua” cạnh icon giỏ hàng.
- User xem danh sách đơn và lọc theo trạng thái (Pending/Paid/Shipping/Completed/Canceled).
- Hiển thị trạng thái theo thao tác Admin.

### 12.3. Gắn Order với User

- Thêm `Order.UserId` để chỉ hiển thị đơn của user đang đăng nhập.

Các file liên quan:

- `Views/Shared/_Layout.cshtml`
- `Views/Product/List.cshtml`
- `Controllers/OrderController.cs`
- `Views/Order/MyOrders.cshtml`, `Views/Order/Details.cshtml`
- `Models/Order.cs` + migration `AddOrderUserId`

---

## 13) Chặn đặt hàng khi hết tồn kho

- Khi `StockQuantity = 0`:
  - UI hiển thị “Hết hàng”, không cho bấm thêm vào giỏ.
  - Backend chặn add/update/select/checkout và hiển thị cảnh báo.

Các file liên quan:

- `Views/Home/Index.cshtml`, `Views/Product/List.cshtml`
- `Views/Shared/_CartPartial.cshtml`, `Views/Cart/Checkout.cshtml`
- `Controllers/CartController.cs`
- `Patterns/CheckoutFacade.cs`

---

## 14) Các lỗi thường gặp và cách xử lý

- **Address already in use** (`http://127.0.0.1:5232`): do chạy trùng instance.
  - `taskkill /F /IM CuoiKy.exe /T` hoặc đổi port.
- **Migration lỗi do chỉnh tay file**: file migration bị hỏng namespace/usings gây lỗi compile.
  - Khuyến nghị không sửa file migration thủ công, chỉ dùng `dotnet ef migrations add ...`.
- **NU1603 (EFCore.Design preview mismatch)**: do version package Design yêu cầu preview cũ, NuGet tự resolve sang preview khác.
  - Cách fix: đồng bộ version các package EF Core trong `.csproj`.

---

## 15) Danh sách đường dẫn tính năng chính (tham khảo)

- Trang chủ: `/`
- Danh sách sản phẩm (public): `/Product/List`
- Chi tiết sản phẩm: `/Product/Details/{id}`
- Giỏ hàng: modal + `/Cart` (trang riêng)
- Checkout: `/Cart/Checkout`
- Đơn đã mua: `/Order/MyOrders`

Admin:

- Quản lý sản phẩm: `/Product/Index`
- Quản lý loại: `/AdminCategory`
- Quản lý đơn: `/AdminOrder`
- Quản lý user: `/AdminUser`
- Thống kê doanh thu: `/AdminRevenue`

---

## 16) Ghi chú

- Tài liệu này tập trung mô tả các thay đổi chính theo yêu cầu đồ án và các vấn đề/giải pháp đã triển khai.
- Không bao gồm thông tin nhạy cảm (mật khẩu, connection string thật, khóa API).

