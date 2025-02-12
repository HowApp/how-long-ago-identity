namespace HowIdentity.Infrastructure.CertificateManagement;

using System.Security.Cryptography.X509Certificates;
using HowCommon.Infrastructure.CertificateManagement;

public sealed class CertificateManager : CertificateManagerBase
{
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

    protected override void GetOrCreateCertificate()
    {
        if (File.Exists(CertPath))
        {
            Certificate = new X509Certificate2(CertPath, CertPassword);
            
            if (Certificate.NotAfter <= DateTime.UtcNow.AddDays(7))
            {
                GenerateAndSaveCertificate(CertPassword, CN);
            }
        }
        else
        {
            GenerateAndSaveCertificate(CertPassword, CN);
        }
    }

    private void GenerateAndSaveCertificate(string certPassword, string certName)
    {
        using var rsa = System.Security.Cryptography.RSA.Create(4096);
        var request = new CertificateRequest($"CN={certName}", rsa, 
            System.Security.Cryptography.HashAlgorithmName.SHA256, 
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        var pfxData = certificate.Export(X509ContentType.Pfx, certPassword);

        File.WriteAllBytes(CertPath, pfxData);

        Certificate = new X509Certificate2(pfxData, certPassword);
    }
}