# Fiscal Service — Instrucoes para Agentes

## Projeto

Microservico .NET 8 para emissao de NFC-e (modelo 65) via DFe.NET.
Este servico e **stateless**: recebe payload + certificado por request,
processa (monta XML, assina, transmite SEFAZ, gera DANFE) e devolve resultado.

## Repositorio irmao

- **Backend NestJS:** `/home/marcos/Projetos/gestao_fiscal_backend/`
  - Orquestra o dominio fiscal, fila BullMQ, storage Supabase S3
  - Chama este servico via HTTP REST com API Key (`X-Api-Key`)

## Arquitetura

- **DDD + Clean Architecture** em 4 layers:
  - `FiscalService.Api` — Controllers ASP.NET + Middleware
  - `FiscalService.Application` — UseCases MediatR (CQRS), DTOs, Validators, Interfaces, Mappers
  - `FiscalService.Domain` — Entities, Value Objects, Enums, Exceptions (zero deps externas)
  - `FiscalService.Infrastructure` — DFe.NET adapter, Certificate reader, QuestPDF DANFE generator

## Convencoes de codigo

- **Nomes**: ingles para estrutura (`Controller`, `Handler`, `Request`), portugues para dominio (`Nfce`, `CpfCnpj`, `InscricaoEstadual`)
- **CQRS**: MediatR com `IRequest<TResponse>` + `IRequestHandler<TRequest, TResponse>`
- **DTOs**: sempre `Request`/`Response` (nunca `Dto` no nome do use case)
- **Validacao**: FluentValidation com `AbstractValidator<T>`
- **Mappers**: manuais, static classes (sem AutoMapper)
- **Domain**: `BaseEntity` com `Id` + `CreatedAt` + `UpdatedAt`, `ValueObject` com equality components
- **Exceptions de dominio**: `FiscalRejectionException` (rejeicao SEFAZ), `InvalidCertificateException` (certificado invalido)
- **Mensagens de erro**: sempre em PT-BR
- **Endpoints**: `/api/nfce/emit`, `/api/nfce/consulta`, `/api/nfce/cancel`, `/api/sefaz/status-servico`
- **Health check**: `GET /health` (sem auth)
- **Auth**: API Key via header `X-Api-Key` (middleware, exceto `/health` e `/swagger`)

## Pacotes principais

| Projeto | Pacotes |
|---------|---------|
| Api | MediatR, FluentValidation.AspNetCore, Serilog.AspNetCore, Swashbuckle |
| Application | MediatR.Contracts, FluentValidation |
| Domain | (nenhuma dep externa) |
| Infrastructure | DFe.NET, QuestPDF, QRCoder |

## Comandos

```bash
dotnet build
dotnet test
dotnet run --project src/FiscalService.Api
docker compose up --build
```

## Contrato com o NestJS (backend)

O NestJS chama este servico com:
- Header: `X-Api-Key: <chave>`
- Body JSON com todos os dados da NFC-e (emitente, destinatario, itens, pagamentos, certificado base64+senha, CSC, ambiente, serie, numero)
- Espera resposta com: sucesso/rejeicao, chave de acesso, protocolo, XML autorizado (base64), DANFE PDF (base64), QR Code

## Regras criticas

1. **Stateless**: nao persiste nada. Toda informacao vem por request.
2. **Certificado nunca em log**: o middleware de logging NUNCA deve registrar `CertificadoBase64` ou `CertificadoSenha`.
3. **SEFAZ pode ser lenta/indisponivel**: o NestJS trata timeout/retry, este servico apenas retorna o resultado.
4. **Ambiente**: `producao` (1) ou `homologacao` (2) — sempre recebido por request.
5. **Numeracao**: responsabilidade do NestJS (reserva atomica). Este servico recebe serie+numero ja definidos.
