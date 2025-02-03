namespace HowIdentity.Infrastructure.Scheduler;

using Quartz;

public class AppJobScheduler
{
    private readonly IScheduler _scheduler;

    public AppJobScheduler(ISchedulerFactory schedulerFactory)
    {
        _scheduler = schedulerFactory.GetScheduler().Result;
    }

    public async Task ScheduleJob(IJobDetail job, ITrigger trigger)
    {
        await _scheduler.ScheduleJob(job, trigger);
    }

    public async Task UnScheduleJob(string jobIdentity, string jobGroup)
    {
        var jobKey = new JobKey(jobIdentity, jobGroup);
        if (await _scheduler.CheckExists(jobKey))
        {
            await _scheduler.DeleteJob(jobKey);
        }
    }

    public async Task<bool> JobExist(string jobIdentity, string jobGroup)
    {
        var jobKey = new JobKey(jobIdentity, jobGroup);

        return await _scheduler.CheckExists(jobKey);
    }
}
