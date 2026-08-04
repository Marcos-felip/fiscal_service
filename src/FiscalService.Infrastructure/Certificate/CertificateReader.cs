using System.Security.Cryptography.X509Certificates;
using FiscalService.Application.Interfaces;
using FiscalService.Domain.Exceptions;

namespace FiscalService.Infrastructure.Certificate;

public class CertificateReader : ICertificateService
{
    public X509Certificate2 Ler(string pfxBase64, string senha)
    {
        try
        {
            var pfxBytes = Convert.FromBase64String(pfxBase64);
            var certificate = new X509Certificate2(pfxBytes, senha,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            if (certificate.NotAfter < DateTime.UtcNow)
            {
                throw new InvalidCertificateException($"Certificado vencido em {certificate.NotAfter:dd/MM/yyyy}");
            }

            return certificate;
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidCertificateException("Falha ao ler certificado PFX", ex);
        }
    }

    public (DateTime Validade, string Titular) ExtrairMetadados(string pfxBase64, string senha)
    {
        try
        {
            var pfxBytes = Convert.FromBase64String(pfxBase64);
            using var certificate = new X509Certificate2(pfxBytes, senha);

            return (certificate.NotAfter, certificate.Subject);
        }
        catch (Exception ex)
        {
            throw new InvalidCertificateException("Falha ao extrair metadados do certificado", ex);
        }
    }
}
