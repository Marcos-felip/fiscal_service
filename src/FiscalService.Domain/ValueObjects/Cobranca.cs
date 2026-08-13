using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

/// <summary>
/// Grupo de cobranca da NF-e: a fatura e suas duplicatas. Existe quando a venda e a prazo —
/// e o que o destinatario usa para conferir o que deve e quando.
/// </summary>
public class Cobranca : ValueObject
{
    public string? NumeroFatura { get; }
    public decimal? ValorOriginal { get; }
    public decimal? ValorDesconto { get; }
    public decimal? ValorLiquido { get; }

    private readonly List<Duplicata> _duplicatas = new();
    public IReadOnlyCollection<Duplicata> Duplicatas => _duplicatas.AsReadOnly();

    public Cobranca(string? numeroFatura, decimal? valorOriginal, decimal? valorDesconto,
        decimal? valorLiquido, IEnumerable<Duplicata>? duplicatas = null)
    {
        NumeroFatura = numeroFatura;
        ValorOriginal = valorOriginal;
        ValorDesconto = valorDesconto;
        ValorLiquido = valorLiquido;

        if (duplicatas is not null)
            _duplicatas.AddRange(duplicatas);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return NumeroFatura ?? string.Empty;
        yield return ValorLiquido ?? 0m;
        yield return _duplicatas.Count;
    }
}

public class Duplicata : ValueObject
{
    public string Numero { get; }
    public DateTime Vencimento { get; }
    public decimal Valor { get; }

    public Duplicata(string numero, DateTime vencimento, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Numero da duplicata e obrigatorio", nameof(numero));

        if (valor <= 0)
            throw new ArgumentException("Valor da duplicata deve ser maior que zero", nameof(valor));

        Numero = numero.Trim();
        Vencimento = vencimento;
        Valor = valor;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Numero;
        yield return Vencimento;
        yield return Valor;
    }
}
