using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using jafleet.Manager;
using AutoMapper;

namespace jafleet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MasterManager.ReadAll();
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<SearchModel, SearchCondition>();
            });

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{id2?}");
            });
        }
    }
}
