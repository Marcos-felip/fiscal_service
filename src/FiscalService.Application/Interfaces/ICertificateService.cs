using System.Security.Cryptography.X509Certificates;

namespace FiscalService.Application.Interfaces;

public interface ICertificateService
{
    X509Certificate2 Ler(string pfxBase64, string senha);
    (DateTime Validade, string Titular) ExtrairMetadados(string pfxBase64, string senha);
}
