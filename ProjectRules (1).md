# AI AGENT RULES & DEVELOPMENT GUIDELINES
Là một AI Agent hỗ trợ phát triển dự án ASP.NET Core 9.0 MVC, bạn phải tuân thủ nghiêm ngặt các quy tắc sau đây trong suốt quá trình tạo mã và giải thích:

## 1. Quy chuẩn Viết mã (C# Coding Conventions)
- Tuân thủ Microsoft C# Coding Conventions.
- Naming: Dùng `PascalCase` cho tên Class, Record, Struct, Method, và Property. Dùng `camelCase` cho tham số (parameters) và biến cục bộ (local variables).
- Private fields/attributes: Sử dụng `camelCase` (Ví dụ: `applicationDbContext`).
- Interfaces: Bắt buộc bắt đầu bằng chữ `I` (Ví dụ: `IProductFactory`).
- Tránh sử dụng `var` một cách vô tội vạ, chỉ dùng `var` khi kiểu dữ liệu đã rõ ràng ở vế phải (Ví dụ: `var list = new List<Product>();`).

## 2. Kiến trúc & Nguyên tắc Thiết kế (Architecture & SOLID)
- Thin Controllers: Giữ cho Controllers càng mỏng càng tốt. Chỉ chứa logic điều hướng và nhận/trả HTTP Request/Response. 
- Business Logic: Đẩy logic nghiệp vụ vào các thư mục `Services` hoặc `Patterns`(ưu tiên).
- Nguyên tắc SOLID: Ưu tiên Single Responsibility Principle (SRP) - mỗi class/method chỉ làm một việc duy nhất.

## 3. Triển khai Design Patterns (Quan trọng nhất)
- Căn cứ vào cuốn "Design Patterns in C#" của Vaskaran Sarcar để triển khai mã.
- Khi áp dụng bất kỳ Design Pattern nào trong 10 pattern yêu cầu, bắt buộc phải có comment định danh rõ ràng ở đầu Class/Method.
  - Cú pháp: `// [Design Pattern: Tên_Pattern] - [Nhóm: Creational/Structural/Behavioral]`
  - Mô tả ngắn gọn: `// Mục đích: Tại sao lại dùng pattern này ở đây?`
- Tách biệt các interface/abstract class của pattern ra các file riêng biệt để dễ quản lý, thay vì gộp chung vào một file lớn.
- Khi áp dụng một Design Pattern mới phải luôn kiểm tra xem có gây xung đột hay vướng lỗi logic gì với các Design Pattern hiện có hay không. Nếu có, thông báo cho người dùng và đề xuất giải pháp.

## 4. Entity Framework Core (Code-First) & Database
- Models/Entities chỉ nên chứa các thuộc tính (Properties) thuần túy định nghĩa dữ liệu.
- Tên bảng: Sử dụng số nhiều (Pluralize) cho tên bảng (Ví dụ: `Products`, `Orders`).

## 5. Quy tắc Trả lời & Tạo mã của AI Agent
- Xử lý lỗi (Error Handling): Đảm bảo các đoạn code thao tác với Database có bọc `try-catch` nếu cần, và không bao giờ "nuốt" exception (nuốt lỗi) mà không ghi log.

## 6. Giao tiếp
- Nếu người dùng yêu cầu một tính năng vi phạm nguyên tắc MVC hoặc làm gãy Design Pattern đang có, hãy cảnh báo và đề xuất giải pháp tốt hơn. 