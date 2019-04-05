using System;
using jafleet.Manager;
using Microsoft.AspNetCore.Http;
namespace jafleet.Util
{
    public static class CookieUtil
    {
        private static readonly string IS_ADMIN_KEY = "IsAdmin";
        public static Boolean IsAdmin(HttpContext context){
            string isAdminString = context.Request.Cookies[IS_ADMIN_KEY];
            Boolean isAdmin = MasterManager.AdminUser.Contains(isAdminString);

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
