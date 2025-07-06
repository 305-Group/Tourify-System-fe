using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Tourify_Frontend.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            bool isPublic = path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/register", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/forgot-password", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/verify-otp", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/api/GoogleAuth", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/google-callback", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/Account/google-callback", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
                         || path.Value.EndsWith("favicon.ico", StringComparison.OrdinalIgnoreCase);

            var token = context.Request.Cookies["AuthToken"];
            
            // Log để debug
            Console.WriteLine($"AuthMiddleware: Token found: {!string.IsNullOrEmpty(token)}");
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"AuthMiddleware: Token starts with 4/: {token.StartsWith("4/")}");
            }
            
            // Nếu có code từ Google OAuth, tạm thời cho phép truy cập
            if (!string.IsNullOrEmpty(token) && token.StartsWith("4/"))
            {
                Console.WriteLine("AuthMiddleware: Found Google code, allowing access");
                // Tạm thời tạo user với thông tin cơ bản
                var identity = new ClaimsIdentity(
                    new[] { 
                        new Claim(ClaimTypes.Name, "Biên Nguyễn"),
                        new Claim(ClaimTypes.Email, "Bien.nguyen24@gmail.com"),
                        new Claim("provider", "google")
                    },
                    "google");
                context.User = new ClaimsPrincipal(identity);
                await _next(context);
                return;
            }

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Kiểm tra xem có phải Google user info (base64) không
                    if (token.Contains("provider") || token.Contains("email"))
                    {
                        try
                        {
                            var userInfoBytes = Convert.FromBase64String(token);
                            var userInfoJson = System.Text.Encoding.UTF8.GetString(userInfoBytes);
                            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson);
                            
                            var identity = new ClaimsIdentity(
                                new[] { 
                                    new Claim(ClaimTypes.Name, userInfo.name),
                                    new Claim(ClaimTypes.Email, userInfo.email),
                                    new Claim("provider", userInfo.provider)
                                },
                                "google");
                            context.User = new ClaimsPrincipal(identity);
                        }
                        catch
                        {
                            // Nếu không parse được base64, thử parse JWT
                            var handler = new JwtSecurityTokenHandler();
                            var jwt = handler.ReadJwtToken(token);

                            var nameClaim = jwt.Claims.FirstOrDefault(c =>
                                c.Type == ClaimTypes.Name ||
                                c.Type == ClaimTypes.NameIdentifier ||
                                c.Type.Equals("unique_name", StringComparison.OrdinalIgnoreCase) ||
                                c.Type.Equals("username", StringComparison.OrdinalIgnoreCase));

                            if (nameClaim != null)
                            {
                                var identity = new ClaimsIdentity(
                                    new[] { new Claim(ClaimTypes.Name, nameClaim.Value) },
                                    "jwt");
                                context.User = new ClaimsPrincipal(identity);
                            }
                        }
                    }
                    else
                    {
                        // Xử lý JWT token thông thường
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);

                        var nameClaim = jwt.Claims.FirstOrDefault(c =>
                            c.Type == ClaimTypes.Name ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type.Equals("unique_name", StringComparison.OrdinalIgnoreCase) ||
                            c.Type.Equals("username", StringComparison.OrdinalIgnoreCase));

                        if (nameClaim != null)
                        {
                            var identity = new ClaimsIdentity(
                                new[] { new Claim(ClaimTypes.Name, nameClaim.Value) },
                                "jwt");
                            context.User = new ClaimsPrincipal(identity);
                        }
                    }
                }
                catch (Exception)
                {
                    // Token không hợp lệ hoặc đã hết hạn: xóa cookie và redirect
                    context.Response.Cookies.Delete("AuthToken");
                    context.Response.Redirect("/login");
                    return;
                }
            }

            // Nếu không có token nào và không phải public route, redirect to login
            if (string.IsNullOrEmpty(token) && !isPublic)
            {
                context.Response.Redirect("/login");
                return;
            }

            await _next(context);
        }

        private async Task HandleGoogleCode(HttpContext context, string code)
        {
            try
            {
                Console.WriteLine("HandleGoogleCode: Starting to exchange code for user info");
                
                // Gọi backend API để exchange code lấy thông tin user
                var client = new HttpClient();
                var jsonContent = JsonSerializer.Serialize(new { code = code });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                Console.WriteLine("HandleGoogleCode: Calling backend API");
                var response = await client.PostAsync("http://localhost:5196/api/GoogleAuth/callback", content);
                
                Console.WriteLine($"HandleGoogleCode: Backend response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"HandleGoogleCode: Backend response: {responseContent}");
                    var result = JsonDocument.Parse(responseContent).RootElement;
                    
                    if (result.TryGetProperty("email", out var emailElement) && 
                        result.TryGetProperty("name", out var nameElement))
                    {
                        var email = emailElement.GetString();
                        var name = nameElement.GetString();
                        
                        // Tạo user info để lưu vào cookie
                        var userInfo = new GoogleUserInfo
                        {
                            email = email,
                            name = name,
                            provider = "google"
                        };
                        
                        var userInfoJson = JsonSerializer.Serialize(userInfo);
                        var userInfoBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userInfoJson));
                        
                        // Lưu thông tin user vào cookie
                        context.Response.Cookies.Append("AuthToken", userInfoBase64, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.Now.AddDays(7)
                        });
                        
                        // Tạo ClaimsPrincipal với thông tin user
                        var identity = new ClaimsIdentity(
                            new[] { 
                                new Claim(ClaimTypes.Name, name),
                                new Claim(ClaimTypes.Email, email),
                                new Claim("provider", "google")
                            },
                            "google");
                        context.User = new ClaimsPrincipal(identity);
                        
                        await _next(context);
                        return;
                    }
                }
                
                // Nếu không lấy được thông tin user, xóa cookie và redirect
                context.Response.Cookies.Delete("AuthToken");
                context.Response.Redirect("/login");
            }
            catch (Exception)
            {
                // Nếu có lỗi, xóa cookie và redirect
                context.Response.Cookies.Delete("AuthToken");
                context.Response.Redirect("/login");
            }
        }
    }

    public class GoogleUserInfo
    {
        public string email { get; set; }
        public string name { get; set; }
        public string provider { get; set; }
    }
}
