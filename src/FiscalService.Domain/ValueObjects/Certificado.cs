using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

public class Certificado : ValueObject
{
    public string PfxBase64 { get; }
    public string Senha { get; }
    public DateTime? Validade { get; private set; }
    public string? Titular { get; private set; }

    public Certificado(string pfxBase64, string senha)
    {
        if (string.IsNullOrWhiteSpace(pfxBase64))
            throw new ArgumentException("Certificado PFX nao pode ser vazio", nameof(pfxBase64));

        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha do certificado nao pode ser vazia", nameof(senha));

        PfxBase64 = pfxBase64;
        Senha = senha;
    }

    public void DefinirMetadados(DateTime validade, string titular)
    {
        Validade = validade;
        Titular = titular;
    }

    public bool EstaVencido => Validade.HasValue && Validade.Value < DateTime.UtcNow;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PfxBase64;
    }
}
