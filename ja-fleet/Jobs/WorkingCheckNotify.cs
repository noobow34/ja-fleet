using EnumStringValues;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Noobow.Commons.Constants;
using Noobow.Commons.Utils;
using Quartz;

namespace jafleet.Jobs
{
    public class WorkingCheckNotify : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            DbContextOptionsBuilder<JafleetContext>? Options = new();
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
            Options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            using JafleetContext jc = new (Options!.Options);
            var log = jc.Logs.Where(l => l.LogType == LogType.WORKIN_NOTIFY_TEXT).OrderByDescending(l => l.LogId).FirstOrDefault();
            if (log != null) {
                await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), log.LogDetail!);
            }
        }
    }
}
