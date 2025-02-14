namespace HowIdentity.Hosting;

using Infrastructure.Scheduler;
using Infrastructure.Scheduler.Jobs;
using Quartz;

public static class HostingDefaultJobExtensions
{
    public static async Task InitializeDefaultJob(this WebApplication app)
    {
        var scheduler = app.Services.GetRequiredService<AppJobScheduler>();

        var microservicesJob = JobBuilder.Create<UpdateUserMicroservicesJob>()
            .WithIdentity(UpdateUserMicroservicesJob.JobKey)
            .Build();
            
        // Trigger the job to run now, and then every 8 hours
        var microservicesTrigger = TriggerBuilder.Create()
            .WithIdentity(UpdateUserMicroservicesJob.TriggerKey)
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(8)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(microservicesJob, microservicesTrigger);
        
        var certificateJob = JobBuilder.Create<EnsureCertificateUpToDate>()
            .WithIdentity(EnsureCertificateUpToDate.JobKey)
            .Build();
            
        // Trigger the job to run after 20 minutes from app start, and then every 8 hours
        var certificateTrigger = TriggerBuilder.Create()
            .WithIdentity(EnsureCertificateUpToDate.TriggerKey)
            .StartAt(DateTime.UtcNow.AddMinutes(20))
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(144)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(certificateJob, certificateTrigger);
    }
}