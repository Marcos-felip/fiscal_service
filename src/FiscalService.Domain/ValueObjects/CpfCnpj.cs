using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

public class CpfCnpj : ValueObject
{
    public string Valor { get; }

    public CpfCnpj(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CPF/CNPJ nao pode ser vazio", nameof(valor));

        Valor = valor.Trim();
    }

    public bool EhCnpj => Valor.Length == 14;
    public bool EhCpf => Valor.Length == 11;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
