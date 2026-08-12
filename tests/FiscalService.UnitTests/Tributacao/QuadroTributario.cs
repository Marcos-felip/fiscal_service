using System.Xml.Linq;
using DFe.Utils;
using FiscalService.Domain.Tributacao;
using FiscalService.Infrastructure.DFe;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// Monta quadros tributarios coerentes para os testes e serializa o resultado, para que
/// cada teste afirme sobre o XML que a SEFAZ receberia — nao sobre o objeto intermediario.
/// </summary>
internal static class QuadroTributario
{
    /// <summary>
    /// Preenche o minimo que a situacao exige, com valores que fecham entre si. Os campos
    /// saem da propria tabela de obrigatorios do dominio: situacao nova entra nos testes
    /// sem mexer aqui.
    /// </summary>
    internal static IcmsItem Icms(string situacao, int origem = 0)
    {
        var vo = SituacaoIcms.Criar(situacao);
        var exige = vo.CamposObrigatorios;

        return new IcmsItem
        {
            Situacao = vo,
            Origem = origem,
            ModBC = exige.Contains(CampoIcms.ModBC) ? 3 : null,
            VBC = exige.Contains(CampoIcms.VBC) ? 100.00m : null,
            PRedBC = exige.Contains(CampoIcms.PRedBC) ? 30.00m : null,
            PIcms = exige.Contains(CampoIcms.PIcms) ? 18.00m : null,
            VIcms = exige.Contains(CampoIcms.VIcms) ? 18.00m : null,
            ModBCST = exige.Contains(CampoIcms.ModBCST) ? 4 : null,
            VBCST = exige.Contains(CampoIcms.VBCST) ? 150.00m : null,
            PIcmsST = exige.Contains(CampoIcms.PIcmsST) ? 18.00m : null,
            VIcmsST = exige.Contains(CampoIcms.VIcmsST) ? 9.00m : null,
            VBCSTRet = exige.Contains(CampoIcms.VBCSTRet) ? 150.00m : null,
            VIcmsSTRet = exige.Contains(CampoIcms.VIcmsSTRet) ? 27.00m : null,
            PCredSn = exige.Contains(CampoIcms.PCredSn) ? 2.50m : null,
            VCredIcmsSn = exige.Contains(CampoIcms.VCredIcmsSn) ? 2.50m : null
        };
    }

    /// <summary>PIS nao tributado — usado quando o teste so olha para o ICMS.</summary>
    internal static PisItem PisNaoTributado() => new() { Situacao = SituacaoPis.Criar("07") };

    internal static CofinsItem CofinsNaoTributado() => new() { Situacao = SituacaoCofins.Criar("07") };

    internal static ImpostoItem Completo(
        IcmsItem? icms = null,
        PisItem? pis = null,
        CofinsItem? cofins = null,
        IpiItem? ipi = null)
    {
        return new ImpostoItem
        {
            Icms = icms ?? Icms("102"),
            Pis = pis ?? PisNaoTributado(),
            Cofins = cofins ?? CofinsNaoTributado(),
            Ipi = ipi
        };
    }

    /// <summary>Traduz o quadro e devolve o XML do grupo <c>imposto</c>.</summary>
    internal static XElement Xml(ImpostoItem quadro)
        => XDocument.Parse(FuncoesXml.ClasseParaXmlString(TradutorImposto.Montar(quadro))).Root!;

    /// <summary>Primeiro elemento com esse nome, ignorando o namespace do layout.</summary>
    internal static XElement? Elemento(this XElement raiz, string nome)
        => raiz.DescendantsAndSelf().FirstOrDefault(e => e.Name.LocalName == nome);

    internal static string? Texto(this XElement raiz, string nome)
        => raiz.Elemento(nome)?.Value;
}
