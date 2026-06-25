# Hướng Dẫn Deploy BookingBakery Bằng Docker

Tài liệu này hướng dẫn cách chạy toàn bộ hệ thống (Web API ASP.NET Core & MongoDB) bằng Docker và Docker Compose.

## Yêu Cầu Trước Khi Cài Đặt
* Đã cài đặt **Docker Desktop** (trên Windows/macOS) hoặc **Docker Engine** và **Docker Compose** (trên Linux).
* Docker Desktop đang hoạt động.

---

## Các File Cấu Hình Đã Thiết Lập
1. **[Dockerfile](file:///d:/FPT/OJT/Dockerfile)**: Dùng để đóng gói ứng dụng ASP.NET Core (chạy trên nền .NET 8.0).
2. **[docker-compose.yml](file:///d:/FPT/OJT/docker-compose.yml)**: Quản lý 2 container chạy song song:
   * **`bookingbakery-mongodb`**: Chứa cơ sở dữ liệu MongoDB, lưu trữ dữ liệu bền vững (persistent storage) thông qua volume `mongodb_data`.
   * **`bookingbakery-api`**: Ứng dụng Backend API kết nối tới cơ sở dữ liệu MongoDB trong mạng ảo Docker.
3. **[.dockerignore](file:///d:/FPT/OJT/.dockerignore)**: Giúp bỏ qua các file rác (như `bin`, `obj`, `.vs`) khi build Docker Image để tăng tốc độ.

---

## Hướng Dẫn Khởi Chạy

### Bước 1: Khởi chạy các container
Mở Terminal/PowerShell tại thư mục gốc của dự án (`d:\FPT\OJT`) và chạy lệnh sau:

```bash
docker-compose up -d --build
```

* `-d`: Chạy dưới nền (detached mode).
* `--build`: Tự động build lại Docker image từ mã nguồn mới nhất.

### Bước 2: Kiểm tra trạng thái các container
Bạn có thể xem các container có đang hoạt động tốt hay không bằng lệnh:

```bash
docker-compose ps
```

Hoặc kiểm tra log hoạt động của API để xem quá trình seeding dữ liệu có lỗi gì không:

```bash
docker-compose logs -f backend
```

---

## Đường Dẫn Truy Cập Sau Khi Deploy

Khi các container khởi động thành công:
* **Swagger UI (Dành cho việc kiểm thử API):**
  * Đường dẫn: [http://localhost:5109/swagger](http://localhost:5109/swagger) (hoặc trang chủ tự động redirect: [http://localhost:5109](http://localhost:5109))
* **Kết nối trực tiếp tới MongoDB (Nếu dùng MongoDB Compass hoặc các công cụ GUI):**
  * Connection String: `mongodb://localhost:27017`
  * Tên Database: `BookingBakeryDb`

---

## Lệnh Quản Lý Tiện Ích

* **Dừng toàn bộ hệ thống (Giữ lại dữ liệu database):**
  ```bash
  docker-compose down
  ```

* **Dừng hệ thống và XÓA sạch dữ liệu database (Làm mới hoàn toàn):**
  ```bash
  docker-compose down -v
  ```

* **Xem log của MongoDB:**
  ```bash
  docker-compose logs -f mongodb
  ```
