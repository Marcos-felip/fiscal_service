using FiscalService.Domain.Common;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// CST de COFINS. Mesma tabela do PIS, tipo proprio para que a mensagem de recusa diga
/// qual das duas contribuicoes esta incoerente.
/// </summary>
public sealed class SituacaoCofins : ValueObject
{
    public string Codigo { get; }
    public FormaApuracaoContribuicao Forma { get; }

    private SituacaoCofins(string codigo, FormaApuracaoContribuicao forma)
    {
        Codigo = codigo;
        Forma = forma;
    }

    public static SituacaoCofins Criar(string? codigo)
    {
        return TryCriar(codigo, out var situacao)
            ? situacao
            : throw new ArgumentException(MensagemInvalida(codigo));
    }

    public static bool TryCriar(string? codigo, out SituacaoCofins situacao)
    {
        situacao = null!;

        var normalizado = CodigosContribuicao.Normalizar(codigo);
        if (normalizado is null || !CodigosContribuicao.TryForma(normalizado, out var forma))
            return false;

        situacao = new SituacaoCofins(normalizado, forma);
        return true;
    }

    public static string MensagemInvalida(string? codigo)
    {
        var informado = string.IsNullOrWhiteSpace(codigo) ? "(vazio)" : codigo.Trim();
        return $"CST de COFINS '{informado}' nao existe. Valores validos: {string.Join(", ", CodigosContribuicao.Codigos)}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Codigo;
}
