using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace jafleet.Classes
{
    public class ConditionalAuthRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private static string[] EXCLUDE_LIST = [".CSS", ".JS", ".PNG", ".JPG", ".JPEG", ".GIF", ".ICO", "/CHECK", "/ACCOUNT/LOGIN", "/SETCOOKIE", "/API", "/MASTER", "/LOG"];

        public ConditionalAuthRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool autoLoginTarget = !EXCLUDE_LIST.Any(s => context.Request.Path.Value!.ToUpper().Contains(s));
            if (context.User.Identity!.IsAuthenticated || !autoLoginTarget)
            {
                await _next(context);
                return;
            }

            string adminKey = Environment.GetEnvironmentVariable("admin_key") ?? "";
            string adminValue = Environment.GetEnvironmentVariable("admin_value") ?? "";
            context.Request.Cookies.TryGetValue(adminKey, out string? adminCookieValue);
            Console.WriteLine($"Admin Key: {adminKey}[END]");
            Console.WriteLine($"Admin Value: {adminValue}[END]");
            Console.WriteLine($"Admin Cookie Value: {adminCookieValue}[END]");
            if (adminCookieValue == adminValue)
            {
                Console.WriteLine("Admin cookie value matches, redirecting to /Master/Index");
                context.Response.Redirect("/Account/Login");
                context.Response.Cookies.Append(adminKey, adminCookieValue, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
                return;
            }
            
            await _next(context);
        }
    }
}
