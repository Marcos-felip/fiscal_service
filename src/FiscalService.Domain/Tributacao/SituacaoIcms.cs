using FiscalService.Domain.Common;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Situacao tributaria do ICMS — CST no Regime Normal (2 digitos), CSOSN no Simples
/// Nacional (3 digitos).
///
/// Aqui esta a tabela do que cada situacao exige. Fica num lugar so, e fora do adapter,
/// justamente para que aceitar uma situacao nova seja mexer numa linha de tabela e nao
/// cacar <c>if</c> espalhado pela montagem do XML.
/// </summary>
public sealed class SituacaoIcms : ValueObject
{
    private sealed record Regra(GrupoIcms Grupo, CampoIcms[] Obrigatorios);

    /// <summary>CST do Regime Normal.</summary>
    private static readonly IReadOnlyDictionary<string, Regra> RegrasCst = new Dictionary<string, Regra>
    {
        // Tributada integralmente.
        ["00"] = new(GrupoIcms.Icms00,
            new[] { CampoIcms.ModBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms }),

        // Tributada e com cobranca do ICMS por substituicao tributaria.
        ["10"] = new(GrupoIcms.Icms10,
            new[]
            {
                CampoIcms.ModBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms,
                CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST
            }),

        // Tributada com reducao de base de calculo.
        ["20"] = new(GrupoIcms.Icms20,
            new[] { CampoIcms.ModBC, CampoIcms.PRedBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms }),

        // Isenta ou nao tributada e com cobranca do ICMS por substituicao tributaria.
        ["30"] = new(GrupoIcms.Icms30,
            new[] { CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST }),

        // Isenta / nao tributada / suspensao: o grupo comporta so origem e CST.
        ["40"] = new(GrupoIcms.Icms40, Array.Empty<CampoIcms>()),
        ["41"] = new(GrupoIcms.Icms40, Array.Empty<CampoIcms>()),
        ["50"] = new(GrupoIcms.Icms40, Array.Empty<CampoIcms>()),

        // Diferimento.
        ["51"] = new(GrupoIcms.Icms51,
            new[] { CampoIcms.ModBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms }),

        // ICMS cobrado anteriormente por substituicao tributaria.
        ["60"] = new(GrupoIcms.Icms60,
            new[] { CampoIcms.VBCSTRet, CampoIcms.VIcmsSTRet }),

        // Reducao de base e cobranca do ICMS por substituicao tributaria.
        ["70"] = new(GrupoIcms.Icms70,
            new[]
            {
                CampoIcms.ModBC, CampoIcms.PRedBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms,
                CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST
            }),

        // Outras.
        ["90"] = new(GrupoIcms.Icms90,
            new[] { CampoIcms.ModBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms })
    };

    /// <summary>CSOSN do Simples Nacional.</summary>
    private static readonly IReadOnlyDictionary<string, Regra> RegrasCsosn = new Dictionary<string, Regra>
    {
        // Tributada com permissao de credito.
        ["101"] = new(GrupoIcms.IcmsSn101,
            new[] { CampoIcms.PCredSn, CampoIcms.VCredIcmsSn }),

        // Sem permissao de credito / isencao / imune / nao tributada: so origem e CSOSN.
        ["102"] = new(GrupoIcms.IcmsSn102, Array.Empty<CampoIcms>()),
        ["103"] = new(GrupoIcms.IcmsSn102, Array.Empty<CampoIcms>()),
        ["300"] = new(GrupoIcms.IcmsSn102, Array.Empty<CampoIcms>()),
        ["400"] = new(GrupoIcms.IcmsSn102, Array.Empty<CampoIcms>()),

        // Com permissao de credito e com cobranca do ICMS por substituicao tributaria.
        ["201"] = new(GrupoIcms.IcmsSn201,
            new[]
            {
                CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST,
                CampoIcms.PCredSn, CampoIcms.VCredIcmsSn
            }),

        // Sem permissao de credito / isencao, e com cobranca do ICMS por ST.
        ["202"] = new(GrupoIcms.IcmsSn202,
            new[] { CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST }),
        ["203"] = new(GrupoIcms.IcmsSn202,
            new[] { CampoIcms.ModBCST, CampoIcms.VBCST, CampoIcms.PIcmsST, CampoIcms.VIcmsST }),

        // ICMS cobrado anteriormente por substituicao tributaria.
        ["500"] = new(GrupoIcms.IcmsSn500,
            new[] { CampoIcms.VBCSTRet, CampoIcms.VIcmsSTRet }),

        // Outros.
        ["900"] = new(GrupoIcms.IcmsSn900,
            new[] { CampoIcms.ModBC, CampoIcms.VBC, CampoIcms.PIcms, CampoIcms.VIcms })
    };

    public string Codigo { get; }
    public GrupoIcms Grupo { get; }
    public bool EhSimplesNacional { get; }
    public IReadOnlyCollection<CampoIcms> CamposObrigatorios { get; }

    private SituacaoIcms(string codigo, Regra regra, bool ehSimplesNacional)
    {
        Codigo = codigo;
        Grupo = regra.Grupo;
        EhSimplesNacional = ehSimplesNacional;
        CamposObrigatorios = regra.Obrigatorios;
    }

    public static SituacaoIcms Criar(string? codigo)
    {
        return TryCriar(codigo, out var situacao)
            ? situacao
            : throw new ArgumentException(MensagemInvalida(codigo));
    }

    public static bool TryCriar(string? codigo, out SituacaoIcms situacao)
    {
        situacao = null!;

        var normalizado = Normalizar(codigo);
        if (normalizado is null)
            return false;

        // CSOSN tem 3 digitos e CST tem 2; nenhum codigo existe nos dois formatos,
        // entao o proprio codigo diz de qual regime ele e — nao precisa do CRT.
        if (RegrasCsosn.TryGetValue(normalizado, out var csosn))
        {
            situacao = new SituacaoIcms(normalizado, csosn, ehSimplesNacional: true);
            return true;
        }

        if (RegrasCst.TryGetValue(normalizado, out var cst))
        {
            situacao = new SituacaoIcms(normalizado, cst, ehSimplesNacional: false);
            return true;
        }

        return false;
    }

    public static string MensagemInvalida(string? codigo)
    {
        var informado = string.IsNullOrWhiteSpace(codigo) ? "(vazio)" : codigo.Trim();

        return $"Situacao tributaria de ICMS '{informado}' nao existe. " +
               $"CST (Regime Normal): {string.Join(", ", RegrasCst.Keys)}. " +
               $"CSOSN (Simples Nacional): {string.Join(", ", RegrasCsosn.Keys)}";
    }

    private static string? Normalizar(string? codigo)
    {
        var bruto = codigo?.Trim() ?? string.Empty;

        if (bruto.Length == 0 || !bruto.All(char.IsDigit))
            return null;

        // "0" e "5" chegam como CST de um digito.
        return bruto.Length == 1 ? bruto.PadLeft(2, '0') : bruto;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Codigo;
}
