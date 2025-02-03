namespace HowIdentity.Infrastructure.Scheduler.Jobs;

using Quartz;

public class TestJob : IJob
{
    private readonly ILogger<TestJob> _logger;

    public TestJob(ILogger<TestJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("TestJob executed");
        return Task.CompletedTask;
    }
}