﻿using System;
using Microsoft.AspNetCore.Http;
namespace jafleet.Util
{
    public static class CookieUtil
    {
        private static readonly string IS_ADMIN_KEY = "IsAdmin";
        public static Boolean IsAdmin(HttpContext context){
            string isAdminString = context.Request.Cookies[IS_ADMIN_KEY];
            Boolean.TryParse(isAdminString, out bool isAdminTemp);

            //Cookie延長
            if(isAdminTemp){
                var cOptions = new CookieOptions()
                {
                    Expires = new DateTimeOffset(DateTime.Now.AddDays(1000))
                };
                context.Response.Cookies.Append(IS_ADMIN_KEY, isAdminString, cOptions);
            }
            return isAdminTemp;
        }
    }
}