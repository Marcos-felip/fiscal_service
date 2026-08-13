using FiscalService.Domain.Common;

namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Recusa quadro tributario que o motor nao consegue montar corretamente — nunca situacao
/// tributaria que o motor apenas nao esperava. A diferenca importa: a primeira e o motor
/// se protegendo de gerar XML incoerente; a segunda seria o motor decidindo o que o
/// emitente pode fazer, que e exatamente o que esta change tira daqui.
///
/// O que se confere:
/// <list type="bullet">
/// <item>campo que a situacao tributaria exige e nao veio;</item>
/// <item>base sem aliquota (e o inverso), inclusive nos grupos opcionais;</item>
/// <item>valor que nao fecha com base x aliquota, fora da tolerancia de centavos.</item>
/// </list>
///
/// O valor do ICMS-ST fica de fora da conferencia aritmetica de proposito: ele sai de
/// <c>vBCST x pICMSST</c> menos o ICMS proprio, e a subtracao muda conforme a situacao
/// (30 e 201/202 nao tem ICMS proprio, 10 e 70 tem). Errar essa conta aqui recusaria nota
/// valida em producao, que e justamente o problema que esta change existe para resolver.
/// Dos campos de ST se confere presenca e coerencia entre eles, nao o resultado.
/// </summary>
public static class ValidacaoQuadroTributario
{
    public static IReadOnlyList<string> Validar(ImpostoItem imposto)
    {
        var erros = new List<string>();

        ValidarIcms(imposto.Icms, erros);
        ValidarPis(imposto.Pis, erros);
        ValidarCofins(imposto.Cofins, erros);

        if (imposto.Ipi is not null)
            ValidarIpi(imposto.Ipi, erros);

        return erros;
    }

    // ------------------------------------------------------------------------------ ICMS

    private static void ValidarIcms(IcmsItem icms, List<string> erros)
    {
        var situacao = icms.Situacao;
        var rotulo = $"ICMS {(situacao.EhSimplesNacional ? "CSOSN" : "CST")} {situacao.Codigo}";

        var faltando = situacao.CamposObrigatorios
            .Where(campo => !icms.Valor(campo).HasValue)
            .Select(campo => campo.Tag())
            .ToList();

        if (faltando.Count > 0)
        {
            erros.Add($"{rotulo}: essa situacao tributaria exige {string.Join(", ", faltando)}, " +
                      "e o item nao trouxe esse(s) campo(s)");
        }

        // Pares que so fazem sentido juntos. Nos grupos em que sao obrigatorios a regra
        // acima ja pegou; isto cobre os grupos em que sao opcionais (900, por exemplo).
        ExigirJuntos(erros, rotulo, (CampoIcms.VBC, icms.VBC), (CampoIcms.PIcms, icms.PIcms));
        ExigirJuntos(erros, rotulo, (CampoIcms.VBCST, icms.VBCST), (CampoIcms.PIcmsST, icms.PIcmsST));
        ExigirJuntos(erros, rotulo, (CampoIcms.VBCST, icms.VBCST), (CampoIcms.VIcmsST, icms.VIcmsST));
        ExigirJuntos(erros, rotulo, (CampoIcms.VBCSTRet, icms.VBCSTRet), (CampoIcms.VIcmsSTRet, icms.VIcmsSTRet));
        ExigirJuntos(erros, rotulo, (CampoIcms.PFcp, icms.PFcp), (CampoIcms.VFcp, icms.VFcp));
        ExigirJuntos(erros, rotulo, (CampoIcms.PFcpST, icms.PFcpST), (CampoIcms.VFcpST, icms.VFcpST));
        ExigirJuntos(erros, rotulo, (CampoIcms.PCredSn, icms.PCredSn), (CampoIcms.VCredIcmsSn, icms.VCredIcmsSn));

        // vBC ja chega reduzido quando ha pRedBC, entao a conta e direta.
        ConferirValor(erros, rotulo, "vICMS", icms.VBC, icms.PIcms, icms.VIcms, "vBC x pICMS");
    }

    // ------------------------------------------------------------------------- PIS/COFINS

    private static void ValidarPis(PisItem pis, List<string> erros)
    {
        var rotulo = $"PIS CST {pis.Situacao.Codigo}";

        ValidarContribuicao(
            erros,
            rotulo,
            pis.Situacao.Forma,
            aliquotaPercentual: ("pPIS", pis.PPis),
            valor: ("vPIS", pis.VPis),
            baseCalculo: pis.VBC,
            quantidade: pis.QBCProd,
            aliquotaUnitaria: pis.VAliqProd,
            temPercentual: pis.TemPercentual,
            temQuantidade: pis.TemQuantidade);
    }

    private static void ValidarCofins(CofinsItem cofins, List<string> erros)
    {
        var rotulo = $"COFINS CST {cofins.Situacao.Codigo}";

        ValidarContribuicao(
            erros,
            rotulo,
            cofins.Situacao.Forma,
            aliquotaPercentual: ("pCOFINS", cofins.PCofins),
            valor: ("vCOFINS", cofins.VCofins),
            baseCalculo: cofins.VBC,
            quantidade: cofins.QBCProd,
            aliquotaUnitaria: cofins.VAliqProd,
            temPercentual: cofins.TemPercentual,
            temQuantidade: cofins.TemQuantidade);
    }

    private static void ValidarContribuicao(
        List<string> erros,
        string rotulo,
        FormaApuracaoContribuicao forma,
        (string Tag, decimal? Valor) aliquotaPercentual,
        (string Tag, decimal? Valor) valor,
        decimal? baseCalculo,
        decimal? quantidade,
        decimal? aliquotaUnitaria,
        bool temPercentual,
        bool temQuantidade)
    {
        switch (forma)
        {
            case FormaApuracaoContribuicao.NaoTributada:
                // O grupo so comporta o CST; valor informado aqui nao teria onde entrar no XML.
                if (temPercentual || temQuantidade || valor.Valor.HasValue)
                {
                    erros.Add($"{rotulo}: essa situacao tributaria nao comporta base de calculo, " +
                              "aliquota nem valor — o grupo do XML leva apenas o CST");
                }
                return;

            case FormaApuracaoContribuicao.Percentual:
                ExigirTodos(erros, rotulo, ("vBC", baseCalculo), aliquotaPercentual, valor);
                ConferirValor(erros, rotulo, valor.Tag, baseCalculo, aliquotaPercentual.Valor, valor.Valor,
                    $"vBC x {aliquotaPercentual.Tag}");
                return;

            case FormaApuracaoContribuicao.Quantidade:
                ExigirTodos(erros, rotulo, ("qBCProd", quantidade), ("vAliqProd", aliquotaUnitaria), valor);
                ConferirProduto(erros, rotulo, valor.Tag, quantidade, aliquotaUnitaria, valor.Valor,
                    "qBCProd x vAliqProd");
                return;

            case FormaApuracaoContribuicao.Outras:
                if (!temPercentual && !temQuantidade)
                {
                    erros.Add($"{rotulo}: informe a apuracao por percentual (vBC e {aliquotaPercentual.Tag}) " +
                              "ou por quantidade (qBCProd e vAliqProd)");
                    return;
                }

                if (temPercentual && temQuantidade)
                {
                    erros.Add($"{rotulo}: a apuracao e por percentual ou por quantidade, nunca as duas — " +
                              $"vieram vBC/{aliquotaPercentual.Tag} e qBCProd/vAliqProd no mesmo item");
                    return;
                }

                if (temPercentual)
                {
                    ExigirTodos(erros, rotulo, ("vBC", baseCalculo), aliquotaPercentual, valor);
                    ConferirValor(erros, rotulo, valor.Tag, baseCalculo, aliquotaPercentual.Valor, valor.Valor,
                        $"vBC x {aliquotaPercentual.Tag}");
                }
                else
                {
                    ExigirTodos(erros, rotulo, ("qBCProd", quantidade), ("vAliqProd", aliquotaUnitaria), valor);
                    ConferirProduto(erros, rotulo, valor.Tag, quantidade, aliquotaUnitaria, valor.Valor,
                        "qBCProd x vAliqProd");
                }

                return;
        }
    }

    // ------------------------------------------------------------------------------- IPI

    private static void ValidarIpi(IpiItem ipi, List<string> erros)
    {
        var rotulo = $"IPI CST {ipi.Situacao.Codigo}";

        if (!ipi.Situacao.EhTributado)
        {
            if (ipi.VBC.HasValue || ipi.PIpi.HasValue || ipi.VIpi.HasValue)
            {
                erros.Add($"{rotulo}: essa situacao tributaria nao comporta base de calculo, " +
                          "aliquota nem valor — o grupo do XML leva apenas o CST");
            }

            return;
        }

        ExigirTodos(erros, rotulo, ("vBC", ipi.VBC), ("pIPI", ipi.PIpi), ("vIPI", ipi.VIpi));
        ConferirValor(erros, rotulo, "vIPI", ipi.VBC, ipi.PIpi, ipi.VIpi, "vBC x pIPI");
    }

    // -------------------------------------------------------------------------- auxiliares

    private static void ExigirJuntos(
        List<string> erros,
        string rotulo,
        (CampoIcms Campo, decimal? Valor) primeiro,
        (CampoIcms Campo, decimal? Valor) segundo)
    {
        if (primeiro.Valor.HasValue == segundo.Valor.HasValue)
            return;

        var informado = primeiro.Valor.HasValue ? primeiro.Campo : segundo.Campo;
        var ausente = primeiro.Valor.HasValue ? segundo.Campo : primeiro.Campo;

        erros.Add($"{rotulo}: {informado.Tag()} veio sem {ausente.Tag()} — os dois andam juntos");
    }

    private static void ExigirTodos(
        List<string> erros,
        string rotulo,
        params (string Tag, decimal? Valor)[] campos)
    {
        var faltando = campos.Where(c => !c.Valor.HasValue).Select(c => c.Tag).ToList();

        if (faltando.Count > 0 && faltando.Count < campos.Length)
        {
            erros.Add($"{rotulo}: faltam os campos {string.Join(", ", faltando)}");
        }
        else if (faltando.Count == campos.Length)
        {
            erros.Add($"{rotulo}: essa situacao tributaria exige {string.Join(", ", campos.Select(c => c.Tag))}, " +
                      "e o item nao trouxe nenhum deles");
        }
    }

    /// <summary>Confere valor apurado por aliquota percentual.</summary>
    private static void ConferirValor(
        List<string> erros,
        string rotulo,
        string tagValor,
        decimal? baseCalculo,
        decimal? aliquota,
        decimal? valor,
        string formula)
    {
        if (!baseCalculo.HasValue || !aliquota.HasValue || !valor.HasValue)
            return;

        var esperado = decimal.Round(baseCalculo.Value * aliquota.Value / 100m, 2, MidpointRounding.AwayFromZero);

        if (!ToleranciaFiscal.Equivalentes(esperado, valor.Value))
        {
            erros.Add($"{rotulo}: {tagValor} informado ({FormatoFiscal.Valor(valor.Value)}) " +
                      $"diverge de {formula} ({FormatoFiscal.Valor(esperado)})");
        }
    }

    /// <summary>Confere valor apurado por quantidade x aliquota por unidade.</summary>
    private static void ConferirProduto(
        List<string> erros,
        string rotulo,
        string tagValor,
        decimal? quantidade,
        decimal? aliquotaUnitaria,
        decimal? valor,
        string formula)
    {
        if (!quantidade.HasValue || !aliquotaUnitaria.HasValue || !valor.HasValue)
            return;

        var esperado = decimal.Round(quantidade.Value * aliquotaUnitaria.Value, 2, MidpointRounding.AwayFromZero);

        if (!ToleranciaFiscal.Equivalentes(esperado, valor.Value))
        {
            erros.Add($"{rotulo}: {tagValor} informado ({FormatoFiscal.Valor(valor.Value)}) " +
                      $"diverge de {formula} ({FormatoFiscal.Valor(esperado)})");
        }
    }
}
