﻿using jafleet;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Jobs;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;

namespace ja_fleet.Test
{
    [TestClass]
    public sealed class NotifyWorkingStatusJobTest
    {
        [TestMethod]
        public async Task ExecuteWithoutRefreshAsync()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var sch = await schedulerFactory.GetScheduler();
            await sch.Start();

            var jobDetail = JobBuilder.Create<NotifyWorkingStatusJob>()
                .WithIdentity("NotifyWorkingStatusJob")
                .Build();

            var startDate = DateTime.Now.AddSeconds(10);

            var trigger = TriggerBuilder.Create()
                .WithIdentity("NotifyWorkingStatusJob")
                .StartNow()
                .WithCronSchedule($"{startDate.Second} {startDate.Minute} {startDate.Hour} ? * * *")
                .Build();

            await sch.ScheduleJob(jobDetail, trigger);

            await Task.Delay(10 * 60 * 1000);
        }

        [TestMethod]
        public async Task ExecuteWithtRefreshAsync()
        {
            var options = new DbContextOptionsBuilder<JafleetContext>();
            options.UseNpgsql(Environment.GetEnvironmentVariable("JAFLEET_CONNECTION_STRING") ?? "");
            using JafleetContext jContext = new(options.Options);
            var targetReg = jContext.AircraftViews.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).Take(50).AsNoTracking().ToArray().OrderBy(r => Guid.NewGuid());
            var f = new RefreshWorkingStatusAndPhoto(targetReg, 15);
            _ = f.ExecuteCheckAsync();

            var schedulerFactory = new StdSchedulerFactory();
            var sch = await schedulerFactory.GetScheduler();
            await sch.Start();

            var jobDetail = JobBuilder.Create<NotifyWorkingStatusJob>()
                .WithIdentity("NotifyWorkingStatusJob")
                .Build();

            var startDate = DateTime.Now.AddSeconds(10);

            var trigger = TriggerBuilder.Create()
                .WithIdentity("NotifyWorkingStatusJob")
                .StartNow()
                .WithCronSchedule($"{startDate.Second} {startDate.Minute} {startDate.Hour} ? * * *")
                .Build();

            await sch.ScheduleJob(jobDetail, trigger);

            await Task.Delay(10 * 60 * 1000);
        }
    }
}
