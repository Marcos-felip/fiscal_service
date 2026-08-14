# Fiscal Service — Instrucoes para Agentes

## Projeto

Microservico .NET 8 para emissao de NFC-e (modelo 65) e NF-e (modelo 55) via DFe.NET.
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
- **Endpoints**: `/api/nfce/emit`, `/api/nfce/consulta`, `/api/nfce/cancel`, `/api/nfe/emit`, `/api/nfe/consulta`, `/api/nfe/cancel`, `/api/nfe/danfe`, `/api/sefaz/status-servico`
- **Health check**: `GET /health` (sem auth)
- **Auth**: API Key via header `X-Api-Key` (middleware, exceto `/health` e `/swagger`)

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

### Contrato tributario do item

Cada item traz o bloco `imposto` com `icms`, `pis`, `cofins` e, opcionalmente, `ipi` —
situacao tributaria, base de calculo, aliquota e valores. Ver `docs/CONTRATO_TRIBUTARIO.md`
para os campos, as situacoes aceitas e as regras de recusa.

Onde o codigo mora:

| Camada | Onde |
|---|---|
| DTOs | `Application/DTOs/ImpostoDto.cs`, `IcmsDto`, `PisDto`, `CofinsDto`, `IpiDto` |
| Situacoes e tabela de campos obrigatorios | `Domain/Tributacao/SituacaoIcms.cs` e irmaos |
| Validacao de coerencia | `Domain/Tributacao/ValidacaoQuadroTributario.cs` |
| Traducao para o XML | `Infrastructure/DFe/TradutorImposto.cs` |

O bloco `imposto` e **obrigatorio** em todo item. A situacao tributaria e a origem da
mercadoria vem de dentro dele — nao existe mais `origem`/`csosn` no nivel do item.

**Aceitar uma situacao tributaria nova e mexer numa linha da tabela em `SituacaoIcms`, mais o
grupo correspondente em `TradutorImposto`.** Se voce se pegar escrevendo `if` de situacao
tributaria no adapter, e sinal de que a regra foi para o lugar errado.

### Contrato da NF-e modelo 55

Rotas proprias, DTO proprio, dominio proprio: `EmitirNfeRequest`, `Nfe`,
`DestinatarioNfe`. O item e o quadro tributario sao **os mesmos** da NFC-e.

Campos e recusas em [docs/CONTRATO_NFE.md](./docs/CONTRATO_NFE.md). O essencial:

| | NFC-e | NF-e |
|---|---|---|
| Destinatario | opcional | obrigatorio e completo, com `indIEDest` |
| CSC | obrigatorio | **recusado** |
| QR Code | sim | nao existe |
| `tpNF`/`finNFe`/`indFinal`/`indPres` | fixos no motor | vem do backend |
| DANFE | PDF (cupom, QuestPDF) | **HTML** (retrato, `Zeus.Net.NFe.Danfe.Html`) |

**O DANFE do modelo 55 e HTML por restricao medida, nao por preferencia:** o
layout retrato pronto vive no pacote PdfClown, que depende de
`System.Drawing.Common` — Windows-only no .NET 8, e o motor roda em contentor
Linux. A resposta traz `danfeContentType` para o backend saber o que recebeu.

**Recorte vigente (13/08/2026): venda interna, saida, finalidade normal,
destinatario pessoa juridica.** O que esta fora e recusado nomeando o motivo, em
`EmitirNfeValidator` — nunca montado por aproximacao.

## Regras criticas

1. **Stateless**: nao persiste nada. Toda informacao vem por request.
2. **Certificado nunca em log**: o middleware de logging NUNCA deve registrar `CertificadoBase64` ou `CertificadoSenha`.
3. **SEFAZ pode ser lenta/indisponivel**: o NestJS trata timeout/retry, este servico apenas retorna o resultado.
4. **Ambiente**: `producao` (1) ou `homologacao` (2) — sempre recebido por request.
5. **Numeracao**: responsabilidade do NestJS (reserva atomica). Este servico recebe serie+numero ja definidos.
6. **O motor nao decide imposto, e nao calcula.** Situacao tributaria e valores vem prontos do
   backend; aqui eles so viram grupo de XML. O motor recusa o quadro que nao consegue montar
   corretamente — nunca a situacao tributaria que apenas nao esperava.
7. **Somar nao e calcular.** Os totais do documento (`ICMSTot`) sao somados dos itens em
   `TotaisDocumento` — a SEFAZ confere o total contra o somatorio, e total constante e
   rejeicao garantida em nota com imposto destacado.
8. **Mensagem de recusa nao muda com o locale.** Valores em mensagem passam por
   `FormatoFiscal.Valor`, sempre pt-BR: sem isso a mesma recusa sai "10,00" no Windows de
   desenvolvimento e "10.00" no contentor.
9. **Pagamento eletronico leva o grupo `card`.** Cartao, vales, boleto e PIX exigem o grupo
   (YA04); dinheiro, cheque, credito de loja, sem pagamento e outros o recusam. A lista esta
   em `PagamentosEletronicos`, no adapter. Faltar o grupo e a **rejeicao 391**, que fala em
   "dados do cartao" mesmo quando o pagamento foi PIX — e chega depois de a numeracao ter
   sido consumida. `tpIntegra` e sempre 2 (nao integrado): o sistema nao fala com TEF.
