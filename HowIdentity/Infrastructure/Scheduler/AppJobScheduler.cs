namespace HowIdentity.Infrastructure.Scheduler;

using Quartz;

public class AppJobScheduler
{
    private readonly IScheduler _scheduler;

    public AppJobScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
}