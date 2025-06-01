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
    [HttpGet] // Chỉ định rằng đây là Action xử lý yêu cầu GET
    [Route("/login")]
    public IActionResult Login()
    {
        return View(); // Trả về View có tên Login (Login.cshtml)
    }

    // Action để xử lý dữ liệu POST từ form đăng nhập
    [HttpPost] // Chỉ định rằng đây là Action xử lý yêu cầu POST
    [Route("/login")]
    public IActionResult Login(string email, string password)
    {
        // Đây là nơi bạn sẽ thêm logic xác thực người dùng
        // Ví dụ:
        if (email == "test@example.com" && password == "password123")
        {
            // Đăng nhập thành công, chuyển hướng đến trang chủ
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Đăng nhập thất bại, có thể thêm lỗi vào ModelState
            // Hoặc truyền thông báo lỗi qua ViewData/ViewBag
            ViewData["ErrorMessage"] = "Email hoặc mật khẩu không đúng.";
            return View(); // Hiển thị lại trang Login với thông báo lỗi
        }
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