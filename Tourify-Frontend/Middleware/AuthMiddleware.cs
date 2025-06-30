using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

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
                         || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
                         || path.Value.EndsWith("favicon.ico", StringComparison.OrdinalIgnoreCase);

            var token = context.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
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
                catch (Exception)
                {
                    // Token không hợp lệ hoặc đã hết hạn: xóa cookie và redirect
                    context.Response.Cookies.Delete("AuthToken");
                    context.Response.Redirect("/login");
                    return;
                }
            }

            if (string.IsNullOrEmpty(token) && !isPublic)
            {
                context.Response.Redirect("/login");
                return;
            }

            await _next(context);
        }
    }
}
