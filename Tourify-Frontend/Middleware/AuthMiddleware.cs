using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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
            var token = context.Request.Cookies["AuthToken"];
            var isAuthEndpoint = context.Request.Path.StartsWithSegments("/login") ||
                               context.Request.Path.StartsWithSegments("/register") ||
                               context.Request.Path.StartsWithSegments("/forgot-password") ||
                               context.Request.Path.StartsWithSegments("/verify-otp");

            if (string.IsNullOrEmpty(token) && !isAuthEndpoint)
            {
                context.Response.Redirect("/login");
                return;
            }

            await _next(context);
        }
    }
} 