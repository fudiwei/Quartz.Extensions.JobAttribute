using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Quartz.Extensions.JobAttribute.Sample_Net5
{
    [QuartzJob("* * * * * ? ", Description = "Tell the time every seconds.", CronTimeZone = "UTC")]
    [DisallowConcurrentExecution]
    public class ClockJob : IJob
    {
        private readonly ILogger _logger;

        public ClockJob(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        async Task IJob.Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Now is " + context.FireTimeUtc.ToLocalTime());
            await Task.CompletedTask;
        }
    }
}
