namespace FiscalService.Domain.Enums;

/// <summary>Modalidade do frete (<c>modFrete</c>).</summary>
public enum ModalidadeFrete
{
    ContratacaoPorContaDoRemetente = 0,
    ContratacaoPorContaDoDestinatario = 1,
    ContratacaoPorContaDeTerceiros = 2,
    TransporteProprioPorContaDoRemetente = 3,
    TransporteProprioPorContaDoDestinatario = 4,
    SemTransporte = 9
}
