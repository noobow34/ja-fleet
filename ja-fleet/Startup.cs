using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using jafleet.Manager;
using AutoMapper;
using jafleet.Models;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
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
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<SearchModel, SearchConditionInModel>();
                cfg.CreateMap<SearchConditionInModel, SearchModel>();
                cfg.CreateMap<Aircraft, AircraftHistory>();
            });

        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

#if DEBUG
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            services.AddDbContextPool<jafleetContext>(
                options => options.UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging().UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 4), ServerType.MariaDb);
                    }
            ));
#else
            services.AddDbContextPool<jafleetContext>(
                options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 4), ServerType.MariaDb);
                    }
            ));
#endif
            services.Configure<WebEncoderOptions>(options => {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMvc().AddNewtonsoftJson();
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
