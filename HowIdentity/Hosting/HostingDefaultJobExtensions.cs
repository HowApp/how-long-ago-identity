namespace HowIdentity.Hosting;

using Infrastructure.Scheduler;
using Infrastructure.Scheduler.Jobs;
using Quartz;

public static class HostingDefaultJobExtensions
{
    public static async Task InitializeDefaultJob(this WebApplication app)
    {
        var scheduler = app.Services.GetRequiredService<AppJobScheduler>();

        var job = JobBuilder.Create<UpdateUserMicroservicesJob>()
            .WithIdentity(UpdateUserMicroservicesJob.JobKey)
            .Build();
            
        // Trigger the job to run now, and then every 8 hours
        var trigger = TriggerBuilder.Create()
            .WithIdentity(UpdateUserMicroservicesJob.TriggerKey)
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(8)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}