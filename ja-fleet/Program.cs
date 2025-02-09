﻿using jafleet.Classes;
using jafleet.Commons.EF;
using jafleet.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using Quartz;
using Quartz.Impl;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Type = System.Type;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddDbContextPool<JafleetContext>(
    options => options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<WebEncoderOptions>(options =>
{
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

var options = new DbContextOptionsBuilder<JafleetContext>();
options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
using JafleetContext context = new(options.Options);
MasterManager.ReadAll(context);

var schedulerFactory = new StdSchedulerFactory();
IScheduler sch = await schedulerFactory.GetScheduler();
await sch.Start();
var scs = context.SchedulerDefs.Where(s => s.Enabled).AsNoTracking().ToArray();
foreach (var sc in scs)
{
    Type? type = Type.GetType(sc.ClassName);

    if (type is not null)
    {
        var jobDetail = JobBuilder.Create(type)
                        .WithIdentity(sc.ClassName)
                        .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(sc.ClassName)
            .StartNow()
            .WithCronSchedule(sc.CronDef)
            .Build();

        await sch.ScheduleJob(jobDetail, trigger);

        Console.WriteLine($"【{sc.ClassName}:{sc.CronDef}】を登録しました。");
    }
}

app.Run();