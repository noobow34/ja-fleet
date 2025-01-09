using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using jafleet.Manager;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.Extensions.WebEncoders;
using System.Text.Unicode;
using jafleet.Classes;

namespace jafleet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContextPool<jafleetContext>(
                options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );
            services.Configure<WebEncoderOptions>(options => {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            services.AddControllersWithViews();
            services.AddSingleton<IConfiguration>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,jafleetContext context)
        {

            app.UseExceptionHandler("/Home/Error");

            app.UseLoggingMiddleware();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("EditStore", "E/Store",
                    defaults: new { controller = "E", action = "Store" });
                endpoints.MapControllerRoute("Edit1", "e/{id?}",
                    defaults: new { controller = "E", action = "Index" });
                endpoints.MapControllerRoute("Edit2", "E/{id?}",
                    defaults: new { controller = "E", action = "Index" });
                endpoints.MapControllerRoute("Log", "log/{id?}",
                    defaults: new { controller = "log", action = "Index" });
                endpoints.MapControllerRoute("AircraftDetail1", "AircraftDetail/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "Index" });
                endpoints.MapControllerRoute("AircraftDetail2", "AD/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "Index" });
                endpoints.MapControllerRoute("AircraftDetail3", "ADN/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "IndexNohead" });
                endpoints.MapControllerRoute("AircraftDetail4", "ADNB/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "IndexNoheadBack" });
                endpoints.MapControllerRoute("Logy", "logy",
                    defaults: new { controller = "log", action = "Yesterday" });
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}/{id2?}");
            });

            MasterManager.ReadAll(context);
        }
    }
}
