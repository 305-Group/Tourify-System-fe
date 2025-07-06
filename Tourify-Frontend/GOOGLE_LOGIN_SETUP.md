# Google Login Setup Guide

## Tổng quan
Tính năng đăng nhập Google đã được tích hợp vào ứng dụng Tourify Frontend. Flow hoạt động như sau:

1. User nhấn nút "Google" trên trang login
2. Frontend gọi API `/api/GoogleAuth/login-url` để lấy Google OAuth URL
3. User được redirect đến Google để xác thực
4. Google callback về frontend với authorization code: `/login?code=4/0AVMBsJ...`
5. Frontend lưu code vào cookie `auth_token` và redirect vào trang Tour
6. AuthMiddleware nhận diện Google code và cho phép truy cập

## Cấu hình Backend

### 1. Google OAuth Credentials
Đảm bảo backend API (localhost:5196) đã được cấu hình với:
- Google Client ID
- Google Client Secret
- Redirect URI: `http://localhost:5196/api/GoogleAuth/callback`

### 2. API Endpoints
Backend cần có các endpoint sau:
- `GET /api/GoogleAuth/login-url` - Trả về Google OAuth URL với redirect_uri trỏ về frontend
- Google sẽ callback trực tiếp về frontend với authorization code

## Cấu hình Frontend

### 1. CORS Policy
Đã thêm CORS policy trong `Program.cs` để cho phép gọi API từ backend.

### 2. Routes
- `/api/GoogleAuth/login-url` - Proxy đến backend API
- `/login` - Xử lý Google callback với code parameter
- `/Account/google-callback` - Redirect handler (không còn sử dụng)

### 3. JavaScript Files
- `wwwroot/js/google-login.js` - Xử lý Google login flow
- `Views/Account/Login.cshtml` - Đã thêm id cho button Google

### 4. Authentication Middleware
Đã cập nhật `AuthMiddleware.cs` để:
- Kiểm tra cả cookie `AuthToken` và `auth_token`
- Nhận diện Google authorization code (bắt đầu bằng "4/") và coi như đã authenticated
- Cho phép truy cập các route Google Auth mà không cần authentication

## Cách sử dụng

1. Chạy backend API trên port 5196
2. Chạy frontend trên port 5171
3. Truy cập trang login
4. Nhấn nút "Google" để đăng nhập
5. Sau khi xác thực thành công, user sẽ được redirect đến `/Tour/Index`

## Troubleshooting

### Lỗi CORS
- Kiểm tra CORS policy trong `Program.cs`
- Đảm bảo backend cho phép requests từ frontend

### Lỗi Redirect URI
- Đảm bảo Google Console được cấu hình với redirect URI: `http://localhost:5171/login`
- Google sẽ callback trực tiếp về frontend với authorization code

### Lỗi Token
- Kiểm tra JWT token format từ backend
- Đảm bảo token được lưu đúng vào cookie

## Security Notes

- Token được lưu với `SameSite=Lax` để bảo mật
- Cookie có expiry time 7 ngày
- Tất cả API calls đều sử dụng HTTPS trong production 