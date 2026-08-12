using FiscalService.Domain.Common;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// CST de IPI. O layout tem dois grupos: tributado (IPITrib, com base e aliquota) e nao
/// tributado (IPINT, so com o CST).
/// </summary>
public sealed class SituacaoIpi : ValueObject
{
    /// <summary>Saidas e entradas tributadas — os unicos CST que levam valores.</summary>
    private static readonly string[] Tributados = { "00", "49", "50", "99" };

    private static readonly string[] NaoTributados =
        { "01", "02", "03", "04", "05", "51", "52", "53", "54", "55" };

    public string Codigo { get; }
    public bool EhTributado { get; }

    private SituacaoIpi(string codigo, bool ehTributado)
    {
        Codigo = codigo;
        EhTributado = ehTributado;
    }

    public static SituacaoIpi Criar(string? codigo)
    {
        return TryCriar(codigo, out var situacao)
            ? situacao
            : throw new ArgumentException(MensagemInvalida(codigo));
    }

    public static bool TryCriar(string? codigo, out SituacaoIpi situacao)
    {
        situacao = null!;

        var bruto = codigo?.Trim() ?? string.Empty;

        if (bruto.Length is 0 or > 2 || !bruto.All(char.IsDigit))
            return false;

        var normalizado = bruto.PadLeft(2, '0');

        if (Tributados.Contains(normalizado))
        {
            situacao = new SituacaoIpi(normalizado, ehTributado: true);
            return true;
        }

        if (NaoTributados.Contains(normalizado))
        {
            situacao = new SituacaoIpi(normalizado, ehTributado: false);
            return true;
        }

        return false;
    }

    public static string MensagemInvalida(string? codigo)
    {
        var informado = string.IsNullOrWhiteSpace(codigo) ? "(vazio)" : codigo.Trim();

        return $"CST de IPI '{informado}' nao existe. Valores validos: " +
               string.Join(", ", Tributados.Concat(NaoTributados).OrderBy(c => c));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Codigo;
}
