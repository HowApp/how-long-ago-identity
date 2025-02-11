namespace HowIdentity.Infrastructure.CertificateManagement;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common.Configurations;
using Common.Constants;

public class CertificateManagerBase
{
    protected static string CertPassword = string.Empty;
    protected static string CN = string.Empty;
    protected static string CertFileName = string.Empty;
    protected static string CertPath = string.Empty;
    
    protected X509Certificate2 Certificate;
    
    public X509Certificate2 GetCertificate()
    {
        DoNeedThrow();
        
        if (Certificate is null)
        {
            GetOrCreateCertificate();
        }

        return Certificate;
    }
    
    public void SetUpManagerConfig(IConfiguration configuration)
    {
        var certConfig = new CertificateConfiguration();
        configuration.Bind(nameof(CertificateConfiguration), certConfig);

        CertPassword = certConfig.Password;
        CN = certConfig.CN;
        CertFileName = certConfig.CertFileName;
        CertPath = GetCertificatePath(CertFileName);
        
        GetOrCreateCertificate();
        DoNeedThrow();
    }
    
    protected virtual void GetOrCreateCertificate()
    {
        if (File.Exists(CertPath))
        {
            Certificate = new X509Certificate2(CertPath, CertPassword);
            
            if (Certificate.NotAfter <= DateTime.UtcNow)
            {
                throw new CryptographicException("Service certificate is expired");
            }
        }
        else
        {
            throw new FileNotFoundException("Certificate not found");
        }
    }
    
    private string  GetCertificatePath(string certName)
    {
        var directory = string.Empty;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dev-cert", CertificateConstant.ProductName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dev-cert", CertificateConstant.ProductName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dev-cert", CertificateConstant.ProductName);
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        EnsureDirectoryExist(directory);

        return Path.Combine(directory, certName);
    }

    private void EnsureDirectoryExist(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void DoNeedThrow()
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(CertPassword), CertPassword);
        ArgumentException.ThrowIfNullOrEmpty(nameof(CN), CN);
        ArgumentException.ThrowIfNullOrEmpty(nameof(CertFileName), CertFileName);
        ArgumentException.ThrowIfNullOrEmpty(nameof(CertPath), CertPath);
    }
}