namespace FiscalService.Domain.Enums;

/// <summary>Indicador de presenca do comprador no estabelecimento (<c>indPres</c>).</summary>
public enum PresencaComprador
{
    NaoSeAplica = 0,
    Presencial = 1,
    InternetOuTelefone = 2,
    Teleatendimento = 3,
    EntregaEmDomicilio = 4,
    PresencialForaDoEstabelecimento = 5,
    Outros = 9
}
