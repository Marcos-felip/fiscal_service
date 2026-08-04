using FiscalService.Application.Interfaces;
using FiscalService.Domain.Entities;

namespace FiscalService.Infrastructure.Danfe;

public class QuestPdfDanfeGenerator : IDanfeGenerator
{
    public byte[] GerarDanfe(Nfce nfce, string qrCode, string chaveAcesso)
    {
        throw new NotImplementedException("Implementar geracao de DANFE NFC-e com QuestPDF");
    }
}
