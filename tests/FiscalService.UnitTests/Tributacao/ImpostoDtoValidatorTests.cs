using FiscalService.Application.DTOs;
using FiscalService.Application.Validators;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// O caminho que o backend enxerga: payload entra, e o que volta e a mensagem de recusa.
/// Aqui se confere que a validacao de dominio chega ate a resposta HTTP em vez de morrer
/// como excecao no meio da emissao.
/// </summary>
public class ImpostoDtoValidatorTests
{
    private readonly ImpostoDtoValidator _validator = new();

    [Fact]
    public void Quadro_coerente_e_aceito()
    {
        var quadro = Quadro(new IcmsDto("00", 0, ModBC: 3, VBC: 100m, PIcms: 18m, VIcms: 18m));

        _validator.Validate(quadro).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Situacao_de_icms_inexistente_e_recusada_listando_as_validas()
    {
        var quadro = Quadro(new IcmsDto("77", 0));

        var resultado = _validator.Validate(quadro);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("'77' nao existe"));
    }

    [Fact]
    public void Situacao_inexistente_nao_dispara_tambem_a_lista_de_campos_faltando()
    {
        // Sem situacao reconhecida nao ha o que exigir — a recusa tem que ser uma so.
        var quadro = Quadro(new IcmsDto("77", 0));

        _validator.Validate(quadro).Errors.Should().ContainSingle();
    }

    [Fact]
    public void Campo_faltando_para_o_cst_e_recusado_apontando_o_campo()
    {
        var quadro = Quadro(new IcmsDto("00", 0, ModBC: 3, VBC: 100m));

        var resultado = _validator.Validate(quadro);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("ICMS CST 00") && e.ErrorMessage.Contains("pICMS"));
    }

    [Fact]
    public void Valor_divergente_e_recusado_apontando_a_divergencia()
    {
        var quadro = Quadro(new IcmsDto("00", 0, ModBC: 3, VBC: 100m, PIcms: 18m, VIcms: 5m));

        var resultado = _validator.Validate(quadro);

        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("diverge de vBC x pICMS"));
    }

    [Fact]
    public void St_incompleta_e_recusada_indicando_os_campos_exigidos()
    {
        var quadro = Quadro(new IcmsDto("202", 0));

        var resultado = _validator.Validate(quadro);

        resultado.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("vBCST") && e.ErrorMessage.Contains("pICMSST"));
    }

    [Fact]
    public void Pis_por_quantidade_e_aceito_sem_aliquota_percentual()
    {
        var quadro = new ImpostoDto(
            new IcmsDto("102", 0),
            new PisDto("03", QBCProd: 10m, VAliqProd: 0.5m, VPis: 5m),
            new CofinsDto("03", QBCProd: 10m, VAliqProd: 2.3m, VCofins: 23m));

        _validator.Validate(quadro).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Origem_fora_da_tabela_e_recusada()
    {
        var quadro = Quadro(new IcmsDto("102", 9));

        _validator.Validate(quadro).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("Origem da mercadoria"));
    }

    [Fact]
    public void Cst_de_pis_inexistente_e_recusado()
    {
        var quadro = new ImpostoDto(
            new IcmsDto("102", 0),
            new PisDto("88"),
            new CofinsDto("07"));

        _validator.Validate(quadro).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("CST de PIS '88'"));
    }

    private static ImpostoDto Quadro(IcmsDto icms)
        => new(icms, new PisDto("07"), new CofinsDto("07"));
}
