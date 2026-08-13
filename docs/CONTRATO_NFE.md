# Contrato da NF-e modelo 55

`POST /api/nfe/emit` — header `X-Api-Key`, corpo JSON.

> **Recorte vigente (13/08/2026): venda interna, saída, finalidade normal,
> destinatário pessoa jurídica.** O que está fora é **recusado nomeando o
> motivo** — o motor nunca monta um XML aproximando um caso que não sabe
> representar. Ver a tabela de recusas ao final.

## O que difere da NFC-e

| | NFC-e (modelo 65) | NF-e (modelo 55) |
|---|---|---|
| Destinatário | opcional | **obrigatório e completo**, com endereço e `indIEDest` |
| CSC | obrigatório | **recusado** |
| QR Code | no XML e na resposta | não existe |
| `tpNF`, `finNFe`, `indFinal`, `indPres` | fixos no motor | **vêm do backend** |
| Transporte, volumes, cobrança | não existem | opcionais |
| DANFE | PDF (cupom) | **HTML** (retrato) |
| Item e quadro tributário | idênticos | idênticos |

O bloco `imposto` do item **não muda**: é o mesmo da etapa 1, documentado em
[CONTRATO_TRIBUTARIO.md](./CONTRATO_TRIBUTARIO.md).

## Requisição

```jsonc
{
  "emitente": { /* igual ao da NFC-e */ },

  "destinatario": {
    "cpfCnpj": "11223344000186",       // CNPJ; CPF é recusado
    "nome": "Construtora Norte Mineira LTDA",
    "logradouro": "Avenida Ovidio de Abreu",
    "numero": "1200",
    "complemento": null,
    "bairro": "Centro",
    "codigoMunicipio": "3143302",      // IBGE, 7 dígitos
    "municipio": "Montes Claros",
    "uf": "MG",                        // deve ser a mesma do emitente
    "cep": "39400001",
    "indicadorIe": 1,                  // 1 contribuinte, 2 isento, 9 não contribuinte
    "inscricaoEstadual": "0011223340012",
    "telefone": "3832211000",
    "email": null
  },

  "itens": [ /* igual ao da NFC-e, com o bloco imposto */ ],
  "pagamentos": [ { "tipo": "dinheiro", "valor": 100.00 } ],
  "valorTotal": 100.00,

  "certificadoBase64": "...",
  "certificadoSenha": "...",
  "serie": 1,
  "numero": 1,
  "ambiente": "homologacao",

  "naturezaOperacao": "VENDA DE MERCADORIA",   // opcional
  "tipoOperacao": 1,                           // 0 entrada, 1 saída
  "finalidade": 1,                             // 1 normal, 2 complementar, 3 ajuste, 4 devolução
  "consumidorFinal": false,                    // indFinal
  "presenca": 1,                               // indPres

  "transporte": {                              // opcional
    "modalidade": 0,                           // 0,1,2,3,4 ou 9
    "transportadora": {
      "cpfCnpj": "11223344000186",
      "nome": "Transportadora Braga ME",
      "inscricaoEstadual": "0011223340012",
      "endereco": "Rua das Cargas, 50",
      "municipio": "Montes Claros",
      "uf": "MG"
    },
    "veiculo": { "placa": "HAB1234", "uf": "MG", "rntc": "12345" },
    "volumes": [
      { "quantidade": 3, "especie": "CAIXA", "marca": "SF",
        "numeracao": "001", "pesoLiquido": 12.5, "pesoBruto": 13.2 }
    ]
  },

  "cobranca": {                                // opcional; presente na venda a prazo
    "numeroFatura": "001",
    "valorOriginal": 100.00,
    "valorDesconto": null,
    "valorLiquido": 100.00,
    "duplicatas": [
      { "numero": "001/1", "vencimento": "2026-09-13", "valor": 50.00 },
      { "numero": "001/2", "vencimento": "2026-10-13", "valor": 50.00 }
    ]
  }
}
```

**Sem o grupo `transporte`**, a nota declara "sem frete" (`modFrete` 9) — que é o
caso de quem retira no balcão. **Sem `cobranca`**, o grupo não sai no XML.

## Resposta

```jsonc
{
  "sucesso": true,
  "chaveAcesso": "3126...",
  "protocolo": "131260000762680",
  "xmlAutorizadoBase64": "...",
  "danfeBase64": "...",
  "danfeContentType": "text/html; charset=utf-8"
}
```

**`danfeContentType` é novo e importa.** O DANFE da NF-e é **HTML**, não PDF: o
layout retrato pronto da biblioteca depende de `System.Drawing.Common`, que é
Windows-only no .NET 8, e o motor roda em contêiner Linux. Quem grava e serve o
arquivo precisa ler este campo em vez de assumir PDF.

Na rejeição:

```jsonc
{
  "sucesso": false,
  "rejeicao": { "codigo": "204", "mensagem": "Duplicidade de NF-e", "retornoTecnico": null }
}
```

## Recusas do recorte

Todas voltam como `400` com a lista de erros de validação — antes de qualquer
chamada à SEFAZ, e sem consumir numeração.

| Situação | Mensagem |
|---|---|
| Destinatário com CPF | "A NF-e exige destinatário pessoa jurídica com CNPJ válido; para pessoa física, emita NFC-e" |
| Destinatário ausente | "Destinatario e obrigatorio na NF-e modelo 55" |
| UF diferente da do emitente | "Operacao interestadual esta fora do escopo atual: emitente em MG, destinatario em SP" |
| CFOP fora de `5xxx` | "CFOP deve ter 4 digitos e comecar com 5 (operacao interna)" |
| `finalidade` ≠ 1 | "Apenas a finalidade normal e aceita no escopo atual; recebida: devolucao" |
| `tipoOperacao` ≠ 1 | "Apenas nota de saida e aceita no escopo atual" |
| CSC informado | "O CSC e exclusivo da NFC-e e nao deve ser enviado na NF-e" |
| Contribuinte sem IE | "Destinatario declarado contribuinte exige inscricao estadual" |
| Isento/não contribuinte com IE | "Destinatario declarado isento ou nao contribuinte nao pode ter inscricao estadual" |

**PJ não implica contribuinte.** Prestadora de serviço é pessoa jurídica e não é
contribuinte de ICMS. O recorte garante que o destinatário tem **CNPJ**, não que
ele tem inscrição estadual — os três valores de `indicadorIe` continuam válidos.

## Cancelamento, consulta e DANFE

- `POST /api/nfe/cancel` e `POST /api/nfe/consulta` — mesmo corpo das rotas de
  NFC-e. Os eventos da SEFAZ não distinguem modelo: UF e CNPJ saem da própria
  chave de acesso, e o modelo também (posições 21 e 22).
- `POST /api/nfe/danfe` — `{ "xmlAutorizado": "..." }` devolve
  `{ "danfeBase64", "danfeContentType" }`. Serve para reimprimir sem guardar o
  binário.
