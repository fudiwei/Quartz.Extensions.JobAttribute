using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Quartz;

namespace Quartz
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderQuartzJobsExtensions
    {
        /// <summary>
        /// Start to schedule Quartz.NET Jobs.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseQuartzJobs(this IApplicationBuilder builder)
        {
            builder.ApplicationServices.UseQuartzJobs();
            return builder;
        }
    }
}
