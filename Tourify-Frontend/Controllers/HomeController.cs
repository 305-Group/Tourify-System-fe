using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tourify_Frontend.Models; // Đảm bảo namespace này khớp với dự án của bạn

namespace Tourify_Frontend.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Action để hiển thị trang đăng nhập
    [HttpGet]
    [Route("/login")]
    public IActionResult Login()
    {
        return View(); // Trả về View có tên Login (Login.cshtml)
    }

    // Action để xử lý dữ liệu POST từ form đăng nhập
    [HttpPost]
    [Route("/login")]
    // Đã thay đổi tham số đầu vào từ 'email' thành 'username'
    public IActionResult Login(string username, string password)
    {
        // Đây là nơi bạn sẽ thêm logic xác thực người dùng bằng username
        // Ví dụ (chỉ để minh họa, trong thực tế bạn sẽ dùng database hoặc Identity):
        if (username == "admin" && password == "password123") // Thay đổi điều kiện xác thực
        {
            // Đăng nhập thành công, chuyển hướng đến trang chủ
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Đăng nhập thất bại, có thể thêm lỗi vào ModelState
            // Hoặc truyền thông báo lỗi qua ViewData/ViewBag
            ViewData["ErrorMessage"] = "Tên người dùng hoặc mật khẩu không đúng."; // Cập nhật thông báo lỗi
            return View(); // Hiển thị lại trang Login với thông báo lỗi
        }
    }

    [HttpGet]
    [Route("/forgot-password")]
    public IActionResult ForgotPassword()
    {
        return View(); // Trả về View có tên ForgotPassword (ForgotPassword.cshtml)
    }

    // Action để xử lý dữ liệu POST từ form Quên mật khẩu
    [HttpPost]
    [Route("/forgot-password")]
    public IActionResult ForgotPassword(string email) // Vẫn giữ email cho chức năng khôi phục mật khẩu
    {
        // Đây là nơi bạn sẽ thêm logic gửi email khôi phục mật khẩu.
        // Ví dụ:
        if (!string.IsNullOrEmpty(email))
        {
            // Thường thì bạn sẽ:
            // 1. Tìm người dùng bằng email trong cơ sở dữ liệu.
            // 2. Tạo một token khôi phục mật khẩu duy nhất.
            // 3. Lưu token đó vào database và gán cho người dùng.
            // 4. Gửi một email chứa liên kết khôi phục (ví dụ: /reset-password?token=XYZ&email=ABC)
            //    đến địa chỉ email đã nhập.
            _logger.LogInformation($"Yêu cầu khôi phục mật khẩu cho email: {email}");

            // Giả định thành công và chuyển hướng đến trang thông báo
            // rằng email đã được gửi.
            // Tạo một View mới cho trang này hoặc sử dụng ViewData để hiển thị thông báo.
            ViewData["SuccessMessage"] = "Nếu địa chỉ email tồn tại trong hệ thống, một liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
            return View("ForgotPasswordConfirmation"); // Bạn sẽ cần tạo View này
        }
        // Nếu email rỗng hoặc không hợp lệ (kiểm tra thêm server-side validation)
        ViewData["ErrorMessage"] = "Vui lòng nhập địa chỉ email hợp lệ.";
        return View(); // Hiển thị lại trang ForgotPassword với thông báo lỗi
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}