using FiscalService.Application.Interfaces;
using FiscalService.Infrastructure.Certificate;
using FiscalService.Infrastructure.Danfe;
using FiscalService.Infrastructure.DFe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiscalService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFiscalEngine, DFeNetAdapter>();
        services.AddScoped<ICertificateService, CertificateReader>();
        services.AddScoped<IDanfeGenerator, QuestPdfDanfeGenerator>();

        return services;
    }
}
