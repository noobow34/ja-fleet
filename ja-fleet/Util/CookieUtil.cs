using jafleet.Manager;
namespace jafleet.Util
{
    public static class CookieUtil
    {
        private static readonly string IS_ADMIN_KEY = "IsAdmin";
        public static bool IsAdmin(HttpContext context){
            string isAdminString = context.Request.Cookies[IS_ADMIN_KEY];
            bool isAdmin = MasterManager.AdminUser.Contains(isAdminString);

            //Cookie延長
            if(isAdmin)
            {
                var cOptions = new CookieOptions()
                {
                    Expires = new DateTimeOffset(DateTime.Now.AddDays(1000))
                };
                context.Response.Cookies.Append(IS_ADMIN_KEY, isAdminString, cOptions);
            }

            return isAdmin;
        }
    }
}
