using jafleet.Classes;
using jafleet.Commons.EF;
using jafleet.Manager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddDbContextPool<jafleetContext>(
    options => options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<WebEncoderOptions>(options => {
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IConfiguration>(config);

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

app.UseLoggingMiddleware();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.MapControllerRoute(
    name: "EditStore",
    pattern: "E/Store",
    defaults: new { controller = "E", action = "Store" }
);
app.MapControllerRoute(
    name: "Edit1",
    pattern: "e/{id?}",
    defaults: new { controller = "E", action = "Index" }
);
app.MapControllerRoute(
    name: "Edit2",
    pattern: "E/{id?}",
    defaults: new { controller = "E", action = "Index" }
);
app.MapControllerRoute(
    name: "Log",
    pattern: "log/{id?}",
    defaults: new { controller = "log", action = "Index" }
);
app.MapControllerRoute(
    name: "AircraftDetail1",
    pattern: "AircraftDetail/{id?}",
    defaults: new { controller = "AircraftDetail", action = "Index" }
);
app.MapControllerRoute(
    name: "AircraftDetail2",
    pattern: "AD/{id?}",
    defaults: new { controller = "AircraftDetail", action = "Index" }
);
app.MapControllerRoute(
    name: "AircraftDetail3",
    pattern: "ADN/{id?}",
    defaults: new { controller = "AircraftDetail", action = "IndexNohead" }
);
app.MapControllerRoute(
    name: "AircraftDetail4",
    pattern: "ADNB/{id?}",
    defaults: new { controller = "AircraftDetail", action = "IndexNoheadBack" }
);
app.MapControllerRoute(
    name: "Logy",
    pattern: "logy",
    defaults: new { controller = "log", action = "Yesterday" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}/{id2?}"
);

var options = new DbContextOptionsBuilder<jafleetContext>();
options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
using jafleetContext context = new (options.Options);
MasterManager.ReadAll(context);

app.Run();