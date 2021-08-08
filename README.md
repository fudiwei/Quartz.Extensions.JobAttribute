# Quartz.Extensions.JobAttribute

[![GitHub Stars](https://img.shields.io/github/stars/fudiwei/Quartz.Extensions.JobAttribute?logo=github)](https://github.com/fudiwei/Quartz.Extensions.JobAttribute)
[![NuGet Version](https://img.shields.io/nuget/v/SKIT.Quartz.Extensions.JobAttribute.svg?sanitize=true)](https://www.nuget.org/packages/SKIT.Quartz.Extensions.JobAttribute)
[![NuGet Download](https://img.shields.io/nuget/dt/SKIT.Quartz.Extensions.JobAttribute.svg?sanitize=true)](https://www.nuget.org/packages/SKIT.Quartz.Extensions.JobAttribute)
[![License](https://img.shields.io/github/license/fudiwei/Quartz.Extensions.JobAttribute)](https://mit-license.org/)

A convenient way to create Quartz.NET jobs using attributes.

---

## Features

-   Supports scheduling jobs using _`QuartzJobAttribute`_ instead of configuration files or programming.
-   Supports dependency injection in _`IJob`_ class (depends on the library [Quartz.Extensions.DependencyInjection](https://github.com/fglaeser/Quartz.Extensions.DependencyInjection)).
-   Follows the lifecycle of ASP .NET Core.

---

## Usage

Install it:

```shell
# Install by Package Manager
> Install-Package Quartz.AspNetCore
> Install-Package SKIT.Quartz.Extensions.JobAttribute

# Install by .NET CLI
> dotnet add package Quartz.AspNetCore
> dotnet add package SKIT.Quartz.Extensions.JobAttribute
```

Here is an example:

```csharp
using Quartz;

[QuartzJob(Name = "Clocking", Description = "Tell the time every seconds.", CronExpression = "* * * * * ? ")]
public class ClockingJob : IJob
{
    private readonly ILogger _logger;

    public ClockingJob(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    Task IJob.Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Now is " + context.FireTimeUtc);
        return Task.CompletedTask;
    }
}
```

Also, there are options for _Name_, _Group_, _Description_ _CronExpression_, _Priority_, _StoreDurably_ and _RequestRecovery_.

Then register service in _ConfigureServices_ method in `Startup.cs` file.

```csharp
using Quartz;

public void ConfigureServices(IServiceCollection services)
{
    services.AddQuartzJobs();

    services.AddQuartzServer(options =>
    {
        options.WaitForJobsToComplete = true;
    });
}
```
