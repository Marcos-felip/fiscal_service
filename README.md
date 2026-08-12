# Fiscal Service

Microserviço .NET 8 para emissão de NFC-e (modelo 65) via DFe.NET (Zeus.Net.NFe).

O serviço é **stateless**: recebe o payload e o certificado a cada request, monta o XML,
assina, transmite para a SEFAZ, gera o DANFE e devolve o resultado. Não persiste nada.

Quem orquestra o domínio fiscal (fila, numeração, storage) é o backend NestJS —
este serviço apenas executa a emissão.

**O motor não decide imposto.** Situação tributária, base de cálculo, alíquota e valores vêm
prontos no payload; aqui eles só viram grupo de XML. O motor recusa o quadro que não consegue
montar corretamente, nunca a situação tributária que apenas não esperava. Ver
[`docs/CONTRATO_TRIBUTARIO.md`](docs/CONTRATO_TRIBUTARIO.md).

---

## Requisitos

Duas opções. **Docker é o caminho recomendado**, porque a imagem já traz o runtime
correto e as bibliotecas nativas do DANFE.

| | Docker | Local |
|---|---|---|
| Necessário | Docker + Docker Compose | .NET SDK 8+ e **runtime .NET 8** |

> **Atenção no modo local:** o SDK 10 compila projetos `net8.0` sem problema, mas
> `dotnet run` exige o runtime 8 instalado. Se faltar:
> ```
> winget install Microsoft.DotNet.AspNetCore.8
> ```

O `nuget.org` precisa estar registrado como fonte de pacotes. Confira com
`dotnet nuget list source`; se não estiver:

```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

---

## Configuração

Copie o exemplo e preencha:

```bash
cp .env.example .env
```

| Variável | Obrigatória | Padrão | Descrição |
|---|---|---|---|
| `FISCAL_API_KEY` | **sim** | — | Chave esperada no header `X-Api-Key` |
| `FISCAL_SERVICE_PORT` | não | `8080` | Porta publicada no host |
| `ASPNETCORE_ENVIRONMENT` | não | `Development` | `Development` habilita o Swagger |

> ⚠️ **Sem `FISCAL_API_KEY` definida o serviço não sobe.** É proposital: o middleware
> falha no boot em vez de deixar a API rodando sem autenticação. O log mostra:
> ```
> System.InvalidOperationException: 'ApiKey:Value' nao configurada.
> ```

O `.env` está no `.gitignore`. Certificados e senhas **não** ficam em configuração —
vêm no corpo de cada request.

---

## Como rodar

### Docker

```bash
docker compose up --build
```

### Local

```bash
dotnet run --project src/FiscalService.Api
```

O perfil em `src/FiscalService.Api/Properties/launchSettings.json` já define
`ASPNETCORE_ENVIRONMENT=Development`, que carrega o `appsettings.Development.json` com
uma API Key de desenvolvimento (`dev-api-key-change-in-production`). Ou seja: para rodar
local você **não** precisa do `.env` — ele é para Docker e produção.

> Rodar sem esse perfil (ou com `ASPNETCORE_ENVIRONMENT=Production` e sem a variável
> definida) derruba o serviço no boot, como descrito acima.

Confira se subiu:

```bash
curl http://localhost:8080/health
# {"status":"healthy","timestamp":"..."}
```

Em `Development`, o Swagger fica em `http://localhost:8080/swagger`.

---

## Endpoints

Todos exigem o header `X-Api-Key`, exceto `/health` e `/swagger`.

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/health` | Health check (sem auth) |
| `POST` | `/api/nfce/emit` | Emite a NFC-e e devolve XML autorizado + DANFE |
| `POST` | `/api/nfce/consulta` | Consulta a situação pela chave de acesso |
| `POST` | `/api/nfce/cancel` | Cancela uma NFC-e autorizada |
| `POST` | `/api/sefaz/status-servico` | Consulta a disponibilidade da SEFAZ |

### Exemplo — emissão

```bash
curl -X POST http://localhost:8080/api/nfce/emit \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: $FISCAL_API_KEY" \
  -d '{
    "emitente": {
      "cnpj": "12345678000195",
      "razaoSocial": "EMPRESA EXEMPLO LTDA",
      "nomeFantasia": "EXEMPLO",
      "inscricaoEstadual": "123456789012",
      "crt": "1",
      "logradouro": "RUA DAS FLORES",
      "numero": "100",
      "bairro": "CENTRO",
      "codigoMunicipio": "3550308",
      "municipio": "SAO PAULO",
      "uf": "SP",
      "cep": "01001000"
    },
    "destinatario": { "cpfCnpj": "12345678909", "nome": "CONSUMIDOR" },
    "itens": [
      {
        "numeroItem": 1,
        "codigoProduto": "PROD-001",
        "descricao": "REFRIGERANTE LATA 350ML",
        "ncm": "22021000",
        "cfop": "5102",
        "unidadeComercial": "UN",
        "quantidade": 2,
        "valorUnitario": 5.50,
        "gtin": "7891000100103",
        "imposto": {
          "icms":   { "situacao": "102", "origem": 0 },
          "pis":    { "situacao": "01", "vBC": 11.00, "pPIS": 1.65, "vPIS": 0.18 },
          "cofins": { "situacao": "01", "vBC": 11.00, "pCOFINS": 7.60, "vCOFINS": 0.84 }
        }
      }
    ],
    "pagamentos": [{ "tipo": "pix", "valor": 11.00 }],
    "valorTotal": 11.00,
    "certificadoBase64": "<PFX em base64>",
    "certificadoSenha": "<senha do PFX>",
    "codigoCsc": "<CSC>",
    "idCsc": "000001",
    "serie": 1,
    "numero": 123,
    "ambiente": "homologacao"
  }'
```

Resposta de sucesso:

```json
{
  "sucesso": true,
  "chaveAcesso": "35260812345678000195650010000001231089637423",
  "protocolo": "135260000000001",
  "xmlAutorizadoBase64": "...",
  "danfeBase64": "...",
  "qrCode": "https://www.homologacao.nfce.fazenda.sp.gov.br/qrcode?p=..."
}
```

`destinatario` é **opcional** — na NFC-e o consumidor pode não se identificar.

`itens[].imposto` carrega o quadro tributário do item: `icms`, `pis` e `cofins` obrigatórios,
`ipi` opcional. Os campos, as situações aceitas e o que é recusado estão em
[`docs/CONTRATO_TRIBUTARIO.md`](docs/CONTRATO_TRIBUTARIO.md).

### Valores aceitos

- `ambiente`: `producao` | `homologacao`
- `crt`: `1` (Simples Nacional) | `2` (Simples Nacional, excesso de sublimite) | `3` (Regime Normal)
- `pagamentos[].tipo`: `dinheiro`, `cheque`, `cartao_credito`, `cartao_debito`,
  `credito_loja`, `pix`, `boleto`, `vale_alimentacao`, `vale_refeicao`,
  `vale_presente`, `vale_combustivel`, `sem_pagamento`, `outro`
- `imposto.icms.situacao`: CSOSN `101`, `102`, `103`, `201`, `202`, `203`, `300`, `400`,
  `500`, `900` (Simples Nacional) ou CST `00`, `10`, `20`, `30`, `40`, `41`, `50`, `51`,
  `60`, `70`, `90` (Regime Normal)
- `imposto.pis.situacao` / `imposto.cofins.situacao`: CST `01` a `09`, `49`, `50`–`56`,
  `60`–`67`, `70`–`75`, `98`, `99`
- `imposto.ipi.situacao`: CST `00`–`05`, `49`–`55`, `99`

---

## Erros

Todos os erros usam o mesmo envelope, com mensagens em PT-BR:

```json
{
  "codigo": "VALIDACAO",
  "mensagem": "Payload invalido",
  "erros": [{ "campo": "Emitente.Uf", "mensagem": "UF do emitente e invalida" }],
  "timestamp": "2026-08-04T14:33:04Z"
}
```

| `codigo` | HTTP | Quando |
|---|---|---|
| `VALIDACAO` | 400 | Payload inválido (`erros` traz a lista campo a campo) |
| `CERT_INVALIDO` | 400 | Certificado ilegível, vencido ou sem chave privada |
| `ARG_INVALIDO` | 400 | Argumento inválido |
| `AUTH_FALHA` | 401 | `X-Api-Key` ausente ou incorreta |
| `CONFIG_INVALIDA` | 500 | `ApiKey:Value` ficou vazia após recarga de configuração |
| `ERRO_INTERNO` | 500 | Falha não tratada |

Rejeição da SEFAZ também volta como `400`, mas com o corpo da própria resposta de
emissão — `sucesso: false` e o bloco `rejeicao` com o código e o motivo do fisco:

```json
{
  "sucesso": false,
  "rejeicao": { "codigo": "539", "mensagem": "Duplicidade de NF-e", "retornoTecnico": null }
}
```

---

## Limitações conhecidas

- **O motor não calcula imposto.** Base, alíquota e valor vêm prontos. Item com base e
  alíquota mas sem o valor é recusado por campo faltante — o motor não completa a conta.
- **Partilha interestadual não é suportada** (`ICMSPart`, `ICMSUFDest`). A NFC-e deste
  serviço é sempre operação interna: `idDest` é fixo em interna e o CFOP é validado como
  `5xxx`.
- **IBS e CBS** ainda não são emitidos — etapa própria do roteiro fiscal.
- **`status-servico` exige certificado** — a SEFAZ pede certificado no handshake TLS
  mesmo para consultar disponibilidade.
- **Numeração é responsabilidade do backend NestJS.** Este serviço recebe `serie` e
  `numero` já reservados.

---

## Desenvolvimento

```bash
dotnet build                                 # compila a solution
dotnet run --project src/FiscalService.Api   # sobe a API (exige runtime .NET 8)
dotnet test                                  # exige runtime .NET 8
docker compose up --build
```

Os testes de unidade cobrem a tradução do quadro tributário para o XML — um caso por situação
tributária aceita — e as regras de recusa. `FiscalService.IntegrationTests` ainda é andaime.

### Estrutura

```
src/
  FiscalService.Api             Controllers + middlewares (auth, log, exceções)
  FiscalService.Application     UseCases (MediatR), DTOs, validators, mappers
  FiscalService.Domain          Entities, value objects, enums (zero deps externas)
    Tributacao/                 Situações tributárias, campos exigidos, coerência do quadro
  FiscalService.Infrastructure  Adapter DFe.NET, leitura de certificado, DANFE
    DFe/TradutorImposto.cs      Quadro tributário -> grupos de imposto do XML
```

### Segurança

- O log de request registra apenas método, rota, status e duração — **nunca o corpo**,
  para que certificado e senha não vazem.
- O certificado existe apenas em memória, durante o request.
