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
using jafleet.Util;
using System.Text.Encodings.Web;
using Microsoft.Extensions.WebEncoders;
using System.Text.Unicode;

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

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
            .AddConsole()
            .AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            services.AddDbContextPool<jafleetContext>(
                options => options.UseLoggerFactory(loggerFactory).UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new Version(10, 3), ServerType.MariaDb);
                    }
            ));

            services.Configure<WebEncoderOptions>(options => {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,jafleetContext context)
        {

            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute("EditStore", "E/Store",
                    defaults: new { controller = "E", action = "Store" });
                routes.MapRoute("Edit1", "e/{id?}",
                    defaults: new { controller = "E", action = "Index" });
                routes.MapRoute("Edit2", "E/{id?}",
                    defaults: new { controller = "E", action = "Index" });
                routes.MapRoute("Log", "log/{id?}",
                    defaults: new { controller = "log", action = "Index" });
                routes.MapRoute("AircraftDetail1", "AircraftDetail/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "Index" });
                routes.MapRoute("AircraftDetail1", "AD/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "Index" });
                routes.MapRoute("AircraftDetail1", "ADN/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "IndexNohead" });
                routes.MapRoute("AircraftDetail1", "ADNB/{id?}",
                    defaults: new { controller = "AircraftDetail", action = "IndexNoheadBack" });
                routes.MapRoute("Logy", "logy",
                    defaults: new { controller = "log", action = "Yesterday" });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{id2?}");
            });

            MasterManager.ReadAll(context);
        }
    }
}
