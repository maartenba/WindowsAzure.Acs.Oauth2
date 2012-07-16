using System.Security.Cryptography.X509Certificates;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public class ResourceServerConfiguration
    {
        public X509Certificate2 IssuerSigningCertificate { get; set; }
        public X509Certificate2 EncryptionVerificationCertificate { get; set; }
    }
}