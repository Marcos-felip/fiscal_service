using FiscalService.Domain.Entities;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Totais do documento, somados a partir do quadro tributario dos itens.
///
/// Somar nao e calcular: os valores continuam vindo prontos do backend. O que este tipo
/// impede e o total constante — a SEFAZ confere o total contra o somatorio dos itens, e
/// nota com ICMS destacado e total zerado e rejeitada.
///
/// Antes disto o adapter escrevia zero em todos os campos de imposto e somava apenas
/// <c>vProd</c>. Acertava por coincidencia, porque toda NFC-e emitida ate hoje usa CSOSN 102,
/// cujo grupo nao tem valor algum.
/// </summary>
public sealed record TotaisDocumento(
    decimal VBC,
    decimal VIcms,
    decimal VFcp,
    decimal VBCST,
    decimal VST,
    decimal VFcpST,
    decimal VProd,
    decimal VIpi,
    decimal VPis,
    decimal VCofins,
    decimal VNF)
{
    public static TotaisDocumento Somar(IEnumerable<ItemFiscal> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var lista = itens as IReadOnlyCollection<ItemFiscal> ?? itens.ToList();

        var vBC = Duas(lista.Sum(i => i.Imposto.Icms.VBC ?? 0m));
        var vIcms = Duas(lista.Sum(i => i.Imposto.Icms.VIcms ?? 0m));
        var vFcp = Duas(lista.Sum(i => i.Imposto.Icms.VFcp ?? 0m));

        // ST retida anteriormente (vICMSSTRet) NAO entra em vST: ela ja foi recolhida por
        // outro contribuinte e ja esta dentro do preco. So a ST devida nesta operacao soma.
        var vBCST = Duas(lista.Sum(i => i.Imposto.Icms.VBCST ?? 0m));
        var vST = Duas(lista.Sum(i => i.Imposto.Icms.VIcmsST ?? 0m));
        var vFcpST = Duas(lista.Sum(i => i.Imposto.Icms.VFcpST ?? 0m));

        var vProd = Duas(lista.Sum(i => i.ValorTotal));
        var vIpi = Duas(lista.Sum(i => i.Imposto.Ipi?.VIpi ?? 0m));
        var vPis = Duas(lista.Sum(i => i.Imposto.Pis.VPis ?? 0m));
        var vCofins = Duas(lista.Sum(i => i.Imposto.Cofins.VCofins ?? 0m));

        // vNF e o que o destinatario paga: produtos mais o que e cobrado por fora.
        // ICMS proprio, PIS e COFINS ja estao dentro do preco e nao somam.
        var vNF = Duas(vProd + vST + vFcpST + vIpi);

        return new TotaisDocumento(vBC, vIcms, vFcp, vBCST, vST, vFcpST, vProd, vIpi, vPis, vCofins, vNF);
    }

    private static decimal Duas(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
