using FiscalService.Domain.Entities;

namespace FiscalService.Application.Interfaces;

public interface IDanfeGenerator
{
    byte[] GerarDanfe(Nfce nfce, string qrCode, string chaveAcesso);
}
