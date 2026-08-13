namespace FiscalService.Application.Interfaces;

/// <summary>
/// Gerador do DANFE do modelo 55. Porta separada da <see cref="IDanfeGenerator"/> porque os
/// dois documentos nao sao o mesmo em nada alem do nome: um e cupom de bobina, o outro e
/// folha retrato — e, hoje, nem o formato de saida coincide.
/// </summary>
public interface IDanfeNfeGenerator
{
    /// <summary>Gera o DANFE a partir do XML autorizado (nfeProc) devolvido pela SEFAZ.</summary>
    DanfeGerado Gerar(string xmlAutorizado, byte[]? logo = null);
}

/// <summary>
/// O conteudo do DANFE junto do seu tipo. O tipo viaja com o conteudo porque quem grava e
/// serve o arquivo nao tem como saber, so olhando os bytes, se recebeu PDF ou HTML.
/// </summary>
public record DanfeGerado(byte[] Conteudo, string ContentType)
{
    public const string Pdf = "application/pdf";
    public const string Html = "text/html; charset=utf-8";
}
