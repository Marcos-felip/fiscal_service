using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;

namespace FiscalService.Domain.ValueObjects;

public class Emitente : ValueObject
{
    public CpfCnpj Cnpj { get; }
    public string RazaoSocial { get; }
    public string? NomeFantasia { get; }
    public string InscricaoEstadual { get; }
    public Crt Crt { get; }
    public Endereco Endereco { get; }
    public string? Telefone { get; }
    public string? Email { get; }

    public Emitente(CpfCnpj cnpj, string razaoSocial, string? nomeFantasia, string inscricaoEstadual,
        Crt crt, Endereco endereco, string? telefone = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new ArgumentException("Razao social do emitente nao pode ser vazia", nameof(razaoSocial));

        if (string.IsNullOrWhiteSpace(inscricaoEstadual))
            throw new ArgumentException("Inscricao estadual do emitente nao pode ser vazia", nameof(inscricaoEstadual));

        Cnpj = cnpj ?? throw new ArgumentNullException(nameof(cnpj));
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        InscricaoEstadual = inscricaoEstadual;
        Crt = crt;
        Endereco = endereco ?? throw new ArgumentNullException(nameof(endereco));
        Telefone = telefone;
        Email = email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Cnpj;
        yield return RazaoSocial;
        yield return InscricaoEstadual;
        yield return Endereco;
    }
}
