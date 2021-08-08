using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Quartz
{
    using Quartz.Extensions.JobAttribute;

    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionQuartzJobsExtensions
    {
        /// <summary>
        /// Adds Quartz.NET services (SchedulerFactory, Triggers, Jobs, etc.) to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
        {
            return services.AddQuartzJobs((q) => { });
        }

        /// <summary>
        /// Adds Quartz.NET services (SchedulerFactory, Triggers, Jobs, etc.) to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, Action<IServiceCollectionQuartzConfigurator> configure)
        {
            return services.AddQuartzJobs(new NameValueCollection(), configure);
        }

        /// <summary>
        /// Adds Quartz.NET services (SchedulerFactory, Triggers, Jobs, etc.) to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="properties"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, NameValueCollection properties, Action<IServiceCollectionQuartzConfigurator> configure)
        {
            IList<Type> listJobType = ReflectionHelper.GetAssembliesTypes()
                .Where(t => ReflectionHelper.IsQuartzJobClass(t) && Attribute.IsDefined(t, ReflectionHelper.TypeOfQuartzJobAttribute))
                .ToList();

            return services.AddQuartz(properties, (quartz) =>
            {
                quartz.UseMicrosoftDependencyInjectionJobFactory();

                foreach (Type jobType in listJobType)
                {
                    QuartzJobAttribute jobAttr = jobType.GetCustomAttributes(typeof(QuartzJobAttribute), true).First() as QuartzJobAttribute;
                    string jobName = string.IsNullOrEmpty(jobAttr.Name) ? jobType.Name : jobAttr.Name;

                    quartz
                        .AddJob(jobType, configure: (config) =>
                        {
                            config.WithIdentity(jobName, jobAttr.Group);
                            config.WithDescription(jobAttr.Description);
                            config.StoreDurably(jobAttr.StoreDurably);
                            config.RequestRecovery(jobAttr.RequestRecovery);
                        })
                        .AddTrigger(config =>
                        {
                            config.ForJob(jobName);
                            config.WithIdentity(jobName + "Trigger", jobAttr.Group);
                            config.WithPriority(jobAttr.Priority);
                            config.WithCronSchedule(jobAttr.CronExpression, builder => builder.InTimeZone(TimeZoneInfo.Local));
                            config.StartNow();
                        });
                }

                configure?.Invoke(quartz);
            });
        }
    }
}
