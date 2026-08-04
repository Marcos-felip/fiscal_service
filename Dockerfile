FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FiscalService.Domain/FiscalService.Domain.csproj", "src/FiscalService.Domain/"]
COPY ["src/FiscalService.Application/FiscalService.Application.csproj", "src/FiscalService.Application/"]
COPY ["src/FiscalService.Infrastructure/FiscalService.Infrastructure.csproj", "src/FiscalService.Infrastructure/"]
COPY ["src/FiscalService.Api/FiscalService.Api.csproj", "src/FiscalService.Api/"]
RUN dotnet restore "src/FiscalService.Api/FiscalService.Api.csproj"

COPY . .
WORKDIR "/src/src/FiscalService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FiscalService.Api.dll"]
