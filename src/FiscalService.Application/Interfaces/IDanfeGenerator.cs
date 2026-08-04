namespace FiscalService.Application.Interfaces;

public interface IDanfeGenerator
{
    /// <summary>
    /// Gera o PDF do DANFE NFC-e a partir do XML autorizado (nfeProc) devolvido pela SEFAZ.
    /// O QR Code e a chave de acesso ja estao contidos no proprio XML (infNFeSupl).
    /// </summary>
    byte[] GerarDanfe(string xmlAutorizado, byte[]? logo = null);
}
