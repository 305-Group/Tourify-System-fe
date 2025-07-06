using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Tourify_Frontend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("login-url")]
        public async Task<IActionResult> GetLoginUrl()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("http://localhost:5196/api/GoogleAuth/login-url");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Return the original URL from backend (keep backend redirect URI)
                    if (result.TryGetProperty("url", out var urlElement))
                    {
                        var url = urlElement.GetString();
                        // Log the URL for debugging
                        Console.WriteLine($"Google OAuth URL: {url}");
                        
                        // Log redirect URI separately to avoid syntax error
                        if (url.Contains("redirect_uri="))
                        {
                            var redirectUriStart = url.IndexOf("redirect_uri=") + 13;
                            var redirectUriEnd = url.IndexOf("&", redirectUriStart);
                            if (redirectUriEnd == -1) redirectUriEnd = url.Length;
                            var redirectUri = url.Substring(redirectUriStart, redirectUriEnd - redirectUriStart);
                            Console.WriteLine($"Redirect URI in URL: {redirectUri}");
                        }
                        else
                        {
                            Console.WriteLine("Redirect URI not found in URL");
                        }
                        
                        return Ok(new { url = url });
                    }
                    
                    return Ok(result);
                }
                
                return BadRequest("Failed to get login URL from backend");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> HandleCallback([FromQuery] string code, [FromQuery] string error)
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    // Redirect to login page with error
                    return RedirectToAction("Login", "Account", new { error = error });
                }
                
                if (string.IsNullOrEmpty(code))
                {
                    return RedirectToAction("Login", "Account", new { error = "No authorization code received" });
                }
                
                var client = _httpClientFactory.CreateClient();
                var jsonContent = JsonSerializer.Serialize(new { code = code });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync("http://localhost:5196/api/GoogleAuth/callback", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("token", out var tokenElement))
                    {
                        var token = tokenElement.GetString();
                        // Redirect to login page with token
                        return RedirectToAction("Login", "Account", new { token = token });
                    }
                    else if (result.TryGetProperty("code", out var codeElement))
                    {
                        var authCode = codeElement.GetString();
                        // Redirect to login page with code
                        return RedirectToAction("Login", "Account", new { code = authCode });
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account", new { error = "No token or code received from backend" });
                    }
                }
                
                return RedirectToAction("Login", "Account", new { error = "Failed to authenticate with Google" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Account", new { error = ex.Message });
            }
        }
    }


} 