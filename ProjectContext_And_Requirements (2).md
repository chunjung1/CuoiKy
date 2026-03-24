# THÔNG TIN TỔNG QUAN DỰ ÁN: WEBSITE QUẢN LÝ CỬA HÀNG BÁN ĐỒ CÔNG NGHỆ (TECH STORE)

## 1. Mục tiêu dự án
Xây dựng một website thương mại điện tử bán đồ công nghệ. 
Yêu cầu bắt buộc: Áp dụng ít nhất 10 Mẫu thiết kế phần mềm (Design Patterns) từ Gang of Four (GoF) vào mã nguồn.

## 2. Mô hình kinh doanh
- Sản phẩm được chia thành 5 danh mục cố định: Điện thoại, Laptop, Bàn phím, Chuột, Tai nghe.
- Mô hình quản lý: 1 cửa hàng duy nhất và 1 kho hàng tương ứng (không có đa chi nhánh).

## 3. Yêu cầu Công nghệ (Tech Stack)
- Framework: ASP.NET Core MVC (phiên bản .NET 9.0).
- ORM: Entity Framework Core.
- Cơ sở dữ liệu: SQL Server.
- Phương pháp tiếp cận CSDL: Code-First (Tạo Models/Entities bằng C# trước, sau đó Migration để sinh DB).
- Kiến trúc thư mục: Sử dụng tính năng `Areas` của ASP.NET Core để tách biệt giao diện/logic của Admin và Khách hàng (Customer) trong cùng một project.

## 4. Cấu trúc thư mục dự kiến
TechStore/
├── Areas/
│   └── Admin/ (Controllers, Views cho Quản trị viên)
├── Controllers/ (Cho Khách hàng)
├── Models/ (Các lớp Entities dùng cho EF Core Code-First)
├── Patterns/ (Thư mục CHỨA LOGIC CỦA 10 DESIGN PATTERNS)
├── Views/ (Giao diện Khách hàng)
├── Data/ (Chứa ApplicationDbContext)
└── Program.cs

## 5. Các Module Cốt lõi
1. Module Sản phẩm (Product Catalog): Hiển thị, tìm kiếm, lọc (Khách hàng); Thêm/Sửa/Xóa (Admin).
2. Module Giỏ hàng (Shopping Cart): Thêm/bớt sản phẩm, tính tổng tiền.
3. Module Đặt hàng & Thanh toán (Checkout): Chọn phương thức thanh toán, tính phí, chốt đơn.
4. Module Quản lý Kho (Inventory): Trừ số lượng kho khi đặt hàng thành công, cộng lại khi hủy đơn.
5. Module Tài khoản (Auth): Đăng ký, Đăng nhập, phân quyền Admin/Customer.
6. Module Quản lý Đơn hàng (Order Management): Cập nhật trạng thái đơn hàng (Chờ xử lý, Đang giao hàng, Đã hoàn thành, Đã hủy).

## 6. Danh sách 10 Design Patterns cần áp dụng (Gợi ý triển khai)
AI Agent cần chú ý thiết kế mã nguồn tuân thủ các pattern sau và đặt file hợp lý (ưu tiên gom vào folder `Patterns` nếu là logic thuần):
1. [Creational] Factory Method: Tạo các đối tượng sản phẩm khác nhau (Phone, Laptop, Keyboard, Mouse, Headphone) dựa trên lớp cha `Product`.
2. [Creational] Singleton: Quản lý cấu hình hệ thống (App Configuration) hoặc quản lý phiên làm việc.
3. [Creational] Builder: Khởi tạo đối tượng Đơn hàng (Order) phức tạp qua nhiều bước (thêm sản phẩm, tính toán phí ship, áp mã giảm giá).
4. [Structural] Composite: Cấu trúc các mặt hàng trong Giỏ hàng (CartItem) để tính tổng giá trị dễ dàng.
5. [Structural] Decorator: Thêm linh hoạt các tùy chọn cho đơn hàng lúc runtime (VD: Phí gói quà, Phí giao hỏa tốc) mà không sửa core Order.
6. [Structural] Facade: Xây dựng lớp `CheckoutFacade` gom các bước (kiểm tra kho, tạo đơn, trừ tiền) thành 1 hàm duy nhất cho Controller gọi.
7. [Behavioral] Strategy: Xử lý các phương thức thanh toán khác nhau (Cash, Bank Transfer) hoặc các thuật toán tính mã giảm giá.
8. [Behavioral] Observer: Kết nối Module Kho và Module Đơn hàng. Khi Order tạo thành công (Subject), Inventory (Observer) tự động trừ số lượng.
9. [Behavioral] State: Quản lý trạng thái Đơn hàng (Pending -> Shipping -> Completed -> Canceled), quy định hành vi nào được phép ở mỗi trạng thái.
10. [Behavioral] Iterator: Tạo cơ chế duyệt qua danh sách Sản phẩm (trong danh mục hoặc trong giỏ hàng) một cách chuẩn mực.

## 7. Hướng dẫn Dành cho AI Agent
- Bước 1: Khởi tạo Project ASP.NET Core MVC 9.0, cài đặt các package EF Core SQL Server và Tools.
- Bước 2: Thiết lập `ApplicationDbContext` và chuỗi kết nối (Connection String).
- Bước 3: Tạo các Models (Entities) theo phương pháp Code-First cho Product, Order, User,... . Chú ý áp dụng Factory Method cho Product ngay từ đầu (có thể dùng TPH - Table Per Hierarchy trong EF Core).
- Bước 4: Viết mã nguồn cho các Design Patterns một cách có tổ chức, comment (chú thích) rõ ràng tên pattern bằng tiếng Việt/tiếng Anh để dễ theo dõi.
- Bước 5: Cài đặt logic cho các Controllers và Views theo mô hình MVC, sử dụng Areas cho phần Admin.
