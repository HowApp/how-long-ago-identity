namespace HowIdentity.Infrastructure.Scheduler.Jobs;

using CertificateManagement;
using Common.Constants;
using Common.Helpers;
using HowCommon.Configurations;
using Quartz;

public class EnsureCertificateUpToDate : IJob
{
    private static readonly string JobTypeId = IdentityJobConstant.Internal;
    private static readonly string JobName = "certificate-management";
    
    public static JobKey JobKey => JobHelpers.GetJobKey(JobTypeId, JobName);
    public static TriggerKey TriggerKey => JobHelpers.GetTriggerKey(JobTypeId, JobName);
    
    private readonly ILogger<EnsureCertificateUpToDate> _logger;
    private readonly CertificateConfiguration _certConfig;

    public EnsureCertificateUpToDate(ILogger<EnsureCertificateUpToDate> logger, IConfiguration configuration)
    {
        _logger = logger;
        _certConfig = configuration.GetSection(nameof(CertificateConfiguration)).Get<CertificateConfiguration>();
    }

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            var certificateManager = CertificateManager.GetInstance();
            certificateManager.SetUpManagerConfig(_certConfig);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        
        return Task.CompletedTask;
    }
}