namespace FiscalService.Domain.Enums;

/// <summary>
/// Finalidade da emissao (<c>finNFe</c>). O motor conhece as quatro; o recorte atual
/// aceita apenas <see cref="Normal"/>, e recusa as outras nomeando qual chegou.
/// </summary>
public enum FinalidadeNfe
{
    Normal = 1,
    Complementar = 2,
    Ajuste = 3,
    Devolucao = 4
}
