namespace HowIdentity.Common.Helpers;

using Quartz;

public class JobHelpers
{
    public static JobKey GetJobKey(string id, string name)
    {
        return JobKey.Create($"job-{id}-{name}", $"group-{id}");
    }
    
    public static TriggerKey GetTriggerKey(string id, string name)
    {
        return new TriggerKey($"trigger-{id}-{name}", $"group-{id}");
    }
}