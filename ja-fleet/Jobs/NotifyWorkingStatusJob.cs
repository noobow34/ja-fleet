using EnumStringValues;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Noobow.Commons.Constants;
using Noobow.Commons.Utils;
using Quartz;

namespace jafleet.Jobs
{
    public class NotifyWorkingStatusJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            DbContextOptionsBuilder<JafleetContext>? Options = new();
            Options.UseNpgsql(Environment.GetEnvironmentVariable("JAFLEET_CONNECTION_STRING") ?? "");
            using JafleetContext jc = new (Options!.Options);
            var log = jc.Logs.Where(l => l.LogType == LogType.WORKING_NOTIFY_TEXT && l.LogDate!.Value.Date == DateTime.Now.Date).OrderByDescending(l => l.LogId).FirstOrDefault();
            if (log != null) {
                await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), log.LogDetail!);
            }
            else
            {
                if (RefreshWorkingStatusAndPhoto.Processing)
                {
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "RefreshWorkingStatusAndPhoto実行中です。終了したら通知します。");
                    int waitCount = 0;
                    while (RefreshWorkingStatusAndPhoto.Processing)
                    {
                        await Task.Delay(5 * 60 * 1_000);
                        waitCount++;
                        if (waitCount > 36)
                        {
                            await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "RefreshWorkingStatusAndPhotoが完了しないため処理を中止します。");
                            return;
                        }
                    }
                    var log2 = jc.Logs.Where(l => l.LogType == LogType.WORKING_NOTIFY_TEXT && l.LogDate!.Value.Date == DateTime.Now.Date).OrderByDescending(l => l.LogId).FirstOrDefault();
                    if (log2 != null)
                    {
                        await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), log2.LogDetail!);
                    }
                }
                else
                {
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "本日はRefreshWorkingStatusAndPhotoが実行されていません。状況を確認してください。");
                }
            }
        }
    }
}
