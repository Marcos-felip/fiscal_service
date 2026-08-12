using FiscalService.Domain.Common;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// CST de PIS. O codigo determina a forma de apuracao e, com ela, quais campos o item
/// precisa trazer.
/// </summary>
public sealed class SituacaoPis : ValueObject
{
    public string Codigo { get; }
    public FormaApuracaoContribuicao Forma { get; }

    private SituacaoPis(string codigo, FormaApuracaoContribuicao forma)
    {
        Codigo = codigo;
        Forma = forma;
    }

    public static SituacaoPis Criar(string? codigo)
    {
        return TryCriar(codigo, out var situacao)
            ? situacao
            : throw new ArgumentException(MensagemInvalida(codigo));
    }

    public static bool TryCriar(string? codigo, out SituacaoPis situacao)
    {
        situacao = null!;

        var normalizado = CodigosContribuicao.Normalizar(codigo);
        if (normalizado is null || !CodigosContribuicao.TryForma(normalizado, out var forma))
            return false;

        situacao = new SituacaoPis(normalizado, forma);
        return true;
    }

    public static string MensagemInvalida(string? codigo)
    {
        var informado = string.IsNullOrWhiteSpace(codigo) ? "(vazio)" : codigo.Trim();
        return $"CST de PIS '{informado}' nao existe. Valores validos: {string.Join(", ", CodigosContribuicao.Codigos)}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Codigo;
}
