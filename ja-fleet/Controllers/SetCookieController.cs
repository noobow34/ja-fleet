using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class SetCookieController : Controller
    {
        public IActionResult Index(string key,string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return BadRequest("Key and value must be provided.");
            }

            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(5)
            });

            return Content("OK");
        }
    }
}
