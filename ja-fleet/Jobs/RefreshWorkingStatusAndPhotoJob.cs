using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace jafleet.Jobs
{
    public class RefreshWorkingStatusAndPhotoJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
            var options = new DbContextOptionsBuilder<JafleetContext>();
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            using JafleetContext jContext = new(options.Options);
            var targetReg = jContext.AircraftViews.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).AsNoTracking().ToArray().OrderBy(r => Guid.NewGuid());
            var check = new RefreshWorkingStatusAndPhoto(targetReg, 15);
            await check.ExecuteCheckAsync();
        }
    }
}
