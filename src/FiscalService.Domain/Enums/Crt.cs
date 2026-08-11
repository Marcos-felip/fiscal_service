namespace FiscalService.Domain.Enums;

public enum Crt
{
    SimplesNacional = 1,
    SimplesNacionalExcessoSublimite = 2,
    RegimeNormal = 3,

    /// <summary>
    /// Microempreendedor Individual. Tributa por CSOSN, como os demais do
    /// Simples — so o Regime Normal usa CST de ICMS.
    /// </summary>
    SimplesNacionalMei = 4
}
