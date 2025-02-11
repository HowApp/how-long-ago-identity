namespace HowIdentity.Infrastructure.CertificateManagement;

using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Common.Configurations;
using Common.Constants;

public sealed class CertificateManager
{
    private static readonly string CertPath = GetCertificatePath("how_app_microservices.pfx");

    private static CertificateManager _instance;

    private static readonly object Lock = new object();

    private CertificateManager()
    {
    }

    public static CertificateManager GetInstance()
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new CertificateManager();
                }
            }
        }
        return _instance;
    }

    public X509Certificate2 GetOrCreateCertificate(IConfiguration configuration)
    {
        var certPassword = new CertificateConfiguration();
        configuration.Bind(nameof(CertificateConfiguration), certPassword);

        if (File.Exists(CertPath))
        {
            var cert = new X509Certificate2(CertPath, certPassword.Password);
            
            if (cert.NotAfter <= DateTime.UtcNow.AddDays(7))
            {
                return GenerateAndSaveCertificate(certPassword.Password);
            }

            return cert;
        }

        return GenerateAndSaveCertificate(certPassword.Password);
    }
    
    private X509Certificate2 GenerateAndSaveCertificate(string certPassword)
    {
        using var rsa = System.Security.Cryptography.RSA.Create(4096);
        var request = new CertificateRequest("CN=my-microservices", rsa, 
            System.Security.Cryptography.HashAlgorithmName.SHA256, 
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        var pfxData = certificate.Export(X509ContentType.Pfx, certPassword);

        File.WriteAllBytes(CertPath, pfxData);

        return new X509Certificate2(pfxData, certPassword);
    }
    
    private static string  GetCertificatePath(string certName)
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

    private static void EnsureDirectoryExist(string directory)
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
}