using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

/// <summary>
/// Destinatario da NFC-e. Na NFC-e o consumidor e opcional: a nota pode ser
/// emitida apenas com o documento, apenas com o nome, ou sem identificacao alguma.
/// </summary>
public class Destinatario : ValueObject
{
    public CpfCnpj? Documento { get; }
    public string? Nome { get; }
    public Endereco? Endereco { get; }

    public Destinatario(CpfCnpj? documento, string? nome, Endereco? endereco = null)
    {
        Documento = documento;
        Nome = nome;
        Endereco = endereco;
    }

    public bool Identificado => Documento is not null || !string.IsNullOrWhiteSpace(Nome);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Documento?.Valor ?? string.Empty;
        yield return Nome ?? string.Empty;
    }
}
