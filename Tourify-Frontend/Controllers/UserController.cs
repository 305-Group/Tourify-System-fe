using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Tourify_Frontend.Controllers
{
    public class UserController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        
        public UserController(ILogger<HomeController> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        
        [HttpGet]
        [Route("/user/profile")]
        public async Task<IActionResult> ViewProfile()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");
            
            // Giải mã JWT để lấy userId
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");
            
            var userId = int.Parse(userIdClaim.Value);
            
            // Gọi API lấy thông tin người dùng
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var getUserUrl = $"{baseUrl}/User/{userId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await client.GetAsync(getUserUrl);

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login", "Account");

            var content = await response.Content.ReadAsStringAsync();
            var userData = JsonConvert.DeserializeObject<dynamic>(content);

            return View(userData);
        }
    }
}