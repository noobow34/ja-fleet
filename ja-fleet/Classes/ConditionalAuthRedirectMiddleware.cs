namespace jafleet.Classes
{
    public class ConditionalAuthRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public ConditionalAuthRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login") || context.User.Identity!.IsAuthenticated)
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
                    return;
                }
            }
            
            await _next(context);
        }
    }
}
