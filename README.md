# 🎬 QLRapPhim – Hệ thống quản lý rạp phim

> Ứng dụng web ASP.NET Core MVC hỗ trợ quản lý phim, lịch chiếu, ghế ngồi, đặt vé và thanh toán VNPay cho rạp phim.

---

## 🍿 Giới thiệu

**QLRapPhim** là dự án quản lý rạp phim được xây dựng bằng **ASP.NET Core 9.0** và **Entity Framework Core**.

Ứng dụng cung cấp các chức năng chính:

- 🎞️ Quản lý phim và thể loại phim
- 🕒 Quản lý suất chiếu, phòng chiếu, ghế ngồi
- 🎟️ Đặt vé theo luồng chọn suất chiếu → chọn ghế → thanh toán
- 🥤 Quản lý đồ ăn kèm (bắp, nước, ...)
- 👤 Xác thực người dùng bằng ASP.NET Core Identity
- 💳 Tích hợp thanh toán VNPay (sandbox)
- 📜 Theo dõi lịch sử đặt vé

---

## 🎥 Công nghệ sử dụng

- 🧩 **ASP.NET Core MVC** (.NET 9)
- 🗄️ **Entity Framework Core**
- 🔐 **ASP.NET Core Identity**
- 💾 **SQL Server** (kết nối qua `UseSqlServer`)
- 💳 **VNPay Sandbox**
- 🎨 **Bootstrap + Razor Views**

---

## 🧱 Cấu trúc thư mục chính

```text
QLRapPhim/
├── Controllers/        # Điều hướng request và xử lý nghiệp vụ
├── Models/             # Entity + ViewModel + DbContext
├── Views/              # Giao diện Razor
├── Areas/Identity/     # Trang đăng nhập/đăng ký
├── Migrations/         # EF Core migrations
├── wwwroot/            # Static files (css, js, images)
├── Program.cs          # Cấu hình DI, middleware, route
└── appsettings.json    # Connection string + cấu hình Email/VNPay
```

---

## 🎫 Chức năng nổi bật

### 👨‍💼 Quản trị
- 🎬 CRUD phim, loại phim
- 🏛️ CRUD phòng chiếu
- 💺 CRUD ghế ngồi
- 🕒 CRUD suất chiếu
- 🥤 CRUD đồ ăn
- 💵 CRUD giá vé

### 🙋 Người dùng
- 🔐 Đăng ký / đăng nhập
- 🔎 Xem danh sách phim
- 🪑 Chọn suất chiếu, chọn ghế
- 🍟 Chọn vé và đồ ăn
- 💳 Thanh toán VNPay
- 📖 Xem lịch sử và chi tiết đặt vé

---

## 🚀 Hướng dẫn chạy dự án

### 1) Yêu cầu môi trường

- ✅ .NET SDK 9.0
- ✅ SQL Server (Express/Developer đều được)
- ✅ IDE: Visual Studio 2022+ hoặc VS Code + C# extension

### 2) Clone source

```bash
git clone <repo-url>
cd LapTrinhWeb/QLRapPhim
```

### 3) Cấu hình `appsettings.json`

Cập nhật các mục sau cho môi trường của bạn:

- 🗄️ `ConnectionStrings:D-T`
- 📧 `EmailSettings` (SMTP)
- 💳 `VnPay` (sandbox hoặc production)

> ⚠️ Khuyến nghị: Không commit thông tin nhạy cảm (password, secret key) vào Git. Nên dùng User Secrets hoặc biến môi trường.

### 4) Cập nhật database

```bash
dotnet ef database update
```

### 5) Chạy ứng dụng

```bash
dotnet run
```

Mặc định truy cập URL được in ra terminal (thường là `https://localhost:xxxx`).

---

## 🧪 Tài khoản & dữ liệu mẫu

- Bạn có thể tạo tài khoản mới tại trang đăng ký của hệ thống.
- Nếu cần dữ liệu mẫu nhanh, tham khảo file `database.sql` để seed dữ liệu ban đầu.

---

## 📌 Ghi chú triển khai

- 🔒 Cần cấu hình HTTPS khi test luồng thanh toán VNPay.
- 🧾 Đảm bảo URL callback (`ReturnUrl`) trùng khớp domain/port đang chạy.
- 🛡️ Ở môi trường production, bật cấu hình bảo mật phù hợp (secret manager, logging, hạn chế lộ thông tin nhạy cảm).

---

## 👨‍💻 Tác giả

Dự án phục vụ học tập/phát triển hệ thống quản lý rạp phim.
