using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace jafleet.Classes
{
    public class ConditionalAuthRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private static string[] EXCLUDE_LIST = [".CSS", ".JS", ".PNG", ".JPG", ".JPEG", ".GIF", ".ICO", "/CHECK"];

        public ConditionalAuthRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool loggingTarget = !EXCLUDE_LIST.Any(s => context.Request.Path.Value!.ToUpper().Contains(s));
            if (context.Request.Path.StartsWithSegments("/Account/Login")
                || context.User.Identity!.IsAuthenticated
                || context.Request.Path.StartsWithSegments("/SetCookie")
                || context.Request.Path.StartsWithSegments("/api")
                || context.Request.Path.StartsWithSegments("/Master")
                || !loggingTarget)
            {
                await _next(context);
                return;
            }

            context.Request.Cookies.TryGetValue("IS_ADMIN", out string? isAdmin);
            if (!string.IsNullOrEmpty(isAdmin))
            {
                string isAdminValue = Environment.GetEnvironmentVariable("IS_ADMIN") ?? "";
                if (isAdmin == isAdminValue)
                {
                    context.Response.Redirect("/Account/Login");
                    context.Response.Cookies.Append("IS_ADMIN", isAdmin, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(365)
                    });
                    return;
                }
            }
            
            await _next(context);
        }
    }
}
