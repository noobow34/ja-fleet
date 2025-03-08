﻿using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace jafleet.Jobs
{
    public class RefreshRetiredAircraftPhoto : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
            var options = new DbContextOptionsBuilder<JafleetContext>();
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            using JafleetContext jContext = new(options.Options);
            var targetRegRetired = jContext.AircraftViews.Where(a => a.OperationCode == OperationCode.RETIRE_UNREGISTERED).AsNoTracking().ToArray().OrderBy(r => Guid.NewGuid());
            var refreshPhoto = new RefreshPhoto(targetRegRetired, 15);
            await refreshPhoto.ExecuteRefreshAsync();
        }
    }
}
