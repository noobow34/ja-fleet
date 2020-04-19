using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace jafleet.Classes
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _services;

        private static string[] EXCLUDE_LIST = new string[] { ".CSS", ".JS", ".PNG",".JPG", ".JPEG", ".GIF" };

        public LoggingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _services = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            bool loggingTarget = !EXCLUDE_LIST.Any(s => httpContext.Request.Path.Value.ToUpper().Contains(s));
            Console.WriteLine($"{httpContext.Request.Path.Value.ToUpper()},{loggingTarget}");
            AccessLog log = null;
            if (loggingTarget)
            {
                log = new AccessLog
                {
                    RequestTime = DateTime.Now
                    ,
                    RequestIp = httpContext.Request.Headers?["X-Real-IP"].FirstOrDefault() != null ?
                                httpContext.Request.Headers["X-Real-IP"].First() : httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    ,
                    RequestPath = httpContext.Request.Path
                    ,
                    RequestQuery = httpContext.Request.QueryString.ToString() != string.Empty ? httpContext.Request.QueryString.ToString() : null
                    ,
                    RequestCookies = string.Concat(httpContext.Request.Cookies)
                    ,
                    UserAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault()
                    ,
                    Referer = httpContext.Request.Headers["Referer"].FirstOrDefault()
                    ,
                    IsAdmin = CookieUtil.IsAdmin(httpContext)
                };
            }
            Console.WriteLine("object complete");
            Stopwatch sw = null;
            if (loggingTarget) { sw = new Stopwatch(); sw.Start(); }
            await _next(httpContext);
            if (loggingTarget) sw.Stop();
            if (loggingTarget && log != null)
            {
                _ = Task.Run(() =>
                {
                    Console.WriteLine("task.run");
                    log.ResponseCode = httpContext.Response.StatusCode;
                    log.ResponseTime = sw.ElapsedMilliseconds;
                    try
                    {
                        log.RequestHostname = Dns.GetHostEntry(log.RequestIp).HostName;
                    }
                    catch { }
                    using var serviceScope = _services.CreateScope();
                    using var context = serviceScope.ServiceProvider.GetService<jafleetContext>();
                    context.AccessLog.Add(log);
                    Console.WriteLine("add");
                    context.SaveChanges();
                    Console.WriteLine("save");
                });
            }
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
