﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;

namespace Quartz
{
    using Quartz.Extensions.JobAttribute;

    /// <summary>
    /// 
    /// </summary>
    public static class ServiceProviderQuartzJobsExtensions
    {
        class QuartaJobMeta
        {
            public IJobDetail JobDetail { get; }

            public ITrigger Trigger { get; }

            public QuartaJobMeta(IJobDetail jobDetail, ITrigger trigger)
            {
                JobDetail = jobDetail;
                Trigger = trigger;
            }
        }

        /// <summary>
        /// Start to schedule Quartz.NET Jobs.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceProvider UseQuartzJobs(this IServiceProvider provider)
        {
            // SetUp Logger
            ILoggerFactory loggerFactory = provider.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory?.CreateLogger("Quartz.NET");
            LogProvider.IsDisabled = true;
            LogProvider.SetCurrentLogProvider(new QuartzLogProvider(loggerFactory));

            // SetUp Jobs & Triggers
            IList<QuartaJobMeta> jobs = new List<QuartaJobMeta>();
            ISchedulerFactory schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
            IScheduler scheduler = schedulerFactory?.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
            QuartzJobTypeContainer container = provider.GetService<QuartzJobTypeContainer>();
            if (container != null)
            {
                foreach (Type type in container)
                {
                    QuartzJobAttribute attribute = type.GetCustomAttributes(typeof(QuartzJobAttribute), true).FirstOrDefault() as QuartzJobAttribute;
                    if (attribute == null)
                    {
                        logger?.LogWarning("`QuartzJobAttribute` is not found in class \"{0}\".", type.FullName);
                        continue;
                    }

                    string jobName = string.IsNullOrEmpty(attribute.Name) ? type.Name : attribute.Name;
                    IJobDetail jobDetail = JobBuilder.Create()
                        .OfType(type)
                        .WithIdentity(jobName, attribute.Group)
                        .RequestRecovery(attribute.RequestRecovery)
                        .StoreDurably(attribute.StoreDurably)
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(jobName + "Trigger", attribute.Group)
                        .WithPriority(attribute.Priority)
                        .WithCronSchedule(attribute.CronExpression, builder => builder.InTimeZone(TimeZoneInfo.Local))
                        .ForJob(jobDetail)
                        .StartNow()
                        .Build();

                    jobs.Add(new QuartaJobMeta(jobDetail, trigger));
                    logger?.LogWarning("Ready to schedule job \"{0}\".");
                }
            }

            IApplicationLifetime lifetime = provider.GetService<IApplicationLifetime>();
            if (lifetime != null)
            {
                // Follow the LifeCycle of ASP.NET Core
                lifetime.ApplicationStarted.Register(async () =>
                {
                    await Task.WhenAll(jobs.Select(job => scheduler.ScheduleJob(job.JobDetail, job.Trigger)));
                    await scheduler.Start();
                });
                lifetime.ApplicationStopping.Register(async () =>
                {
                    if (!scheduler.IsShutdown)
                    {
                        await scheduler.Shutdown(false);
                    }
                });
            }
            else
            {
                // Here we go!
                Task.WhenAll(jobs.Select(job => scheduler.ScheduleJob(job.JobDetail, job.Trigger))).Wait();
                scheduler.Start().Wait();
            }

            return provider;
        }
    }
}
