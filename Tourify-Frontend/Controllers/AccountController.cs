using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Linq;
using static Tourify_Frontend.Models.UserModel;

namespace Tourify_Frontend.Controllers
{
    public class AccountController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AccountController(ILogger<HomeController> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
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
            // Kiểm tra nếu đã có token thì chuyển hướng đến trang Tour
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Tour");
            }
            return View();
        }

        // Action để xử lý dữ liệu POST từ form đăng nhập
        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                    return View();
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var loginUrl = $"{baseUrl}/User/login";

                var loginData = new
                {
                    username = email,
                    password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(loginUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Login API Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                    _logger.LogInformation($"Deserialized token: {loginResponse?.token}");
                    if (!string.IsNullOrEmpty(loginResponse?.token))
                    {
                        // Lưu token vào cookie
                        Response.Cookies.Append("AuthToken", loginResponse.token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.Now.AddDays(7)
                        });

                        return RedirectToAction("Index", "Tour");
                    }
                }

                ViewData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ViewData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View();
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

        // Action để hiển thị trang đăng ký (GET request)
        [HttpGet]
        [Route("/register")] // <-- Định tuyến URL gọn hơn
        public IActionResult Register()
        {
            return View(); // Trả về View có tên Register (Register.cshtml)
        }

        // Action để xử lý dữ liệu POST từ form đăng ký
        [HttpPost]
        [Route("/register")] // <-- Định tuyến URL gọn hơn
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                    return View();
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                // Check if email already exists
                var checkEmailUrl = $"{baseUrl}/User";
                var checkEmailResponse = await _httpClient.GetAsync(checkEmailUrl);

                if (checkEmailResponse.IsSuccessStatusCode)
                {
                    var usersJson = await checkEmailResponse.Content.ReadAsStringAsync();
                    
                    try
                    {
                        // Parse JSON và lấy danh sách email
                        var jsonDocument = JsonDocument.Parse(usersJson);
                        var emailList = new List<string>();
                        
                        foreach (var user in jsonDocument.RootElement.EnumerateArray())
                        {
                            if (user.TryGetProperty("email", out var emailElement) && 
                                emailElement.ValueKind != JsonValueKind.Null && 
                                !string.IsNullOrEmpty(emailElement.GetString()))
                            {
                                emailList.Add(emailElement.GetString().ToLower());
                            }
                        }

                        // Kiểm tra email nhập vào có trong mảng không
                        if (emailList.Contains(email.ToLower()))
                        {
                            ViewData["ErrorMessage"] = "Email này đã được sử dụng. Vui lòng sử dụng email khác.";
                            return View();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing user data: {ex.Message}");
                    }
                }

                var registerUrl = $"{baseUrl}/auth/register";

                var userData = new
                {   
                    email = email,
                    password = password,
                    username = name
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(userData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(registerUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Lưu email vào TempData để sử dụng ở bước verify OTP
                    TempData["VerifyEmail"] = email;
                    // Chuyển hướng đến trang verify OTP
                    return RedirectToAction("VerifyOTP");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Registration failed: {errorContent}");
                    ViewData["ErrorMessage"] = "Đăng ký thất bại. Vui lòng thử lại sau.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                ViewData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View();
            }
        }

        // Action để hiển thị trang verify OTP
        [HttpGet]
        [Route("/verify-otp")]
        public IActionResult VerifyOTP()
        {
            if (TempData["VerifyEmail"] == null)
            {
                return RedirectToAction("Register");
            }
            ViewData["Email"] = TempData["VerifyEmail"];
            return View();
        }

        // Action để xử lý verify OTP
        [HttpPost]
        [Route("/verify-otp")]
        public async Task<IActionResult> VerifyOTP(string email, string otp)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                {
                    ViewData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                    return View();
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var verifyUrl = $"{baseUrl}/auth/verify-otp";

                // Tạo request body chính xác theo API
                var verifyData = new
                {
                    email = email.ToLower(),  // Chuyển về chữ thường
                    otp = otp.Trim()         // Loại bỏ khoảng trắng
                };

                // Tạo request với headers đầy đủ
                var request = new HttpRequestMessage(HttpMethod.Post, verifyUrl);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                
                var jsonContent = JsonSerializer.Serialize(verifyData);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xác thực OTP thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    string errorMessage = "Mã OTP không đúng. Vui lòng thử lại.";
                    if (responseContent.Contains("User not found"))
                    {
                        errorMessage = "Không tìm thấy thông tin người dùng. Vui lòng đăng ký lại.";
                    }
                    else if (responseContent.Contains("OTP expired"))
                    {
                        errorMessage = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.";
                    }

                    ViewData["ErrorMessage"] = errorMessage;
                    ViewData["Email"] = email;
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                ViewData["Email"] = email;
                return View();
            }
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }

    public class LoginResponse
    {
        public string token { get; set; }
    }
}
// cái này dùng cho những cái xử lý dành cho auth (mấy bạn có thể tự tạo thêm controller trong quá trinh làm nha)