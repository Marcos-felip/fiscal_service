namespace FiscalService.Domain.Exceptions;

public class FiscalRejectionException : Exception
{
    public string CodigoRejeicao { get; }
    public string Motivo { get; }

    public FiscalRejectionException(string codigoRejeicao, string motivo)
        : base($"Rejeicao [{codigoRejeicao}]: {motivo}")
    {
        CodigoRejeicao = codigoRejeicao;
        Motivo = motivo;
    }
}
