# Contrato tributário do item

> Change `contrato-tributario-do-item` — etapa 1 do roteiro fiscal.
> Este documento é o handoff para as changes irmãs em `gestao_fiscal_backend` e
> `gestao_fiscal_frontend`.

## O que mudou

**O motor não decide mais imposto.** Antes, `MontarImposto` recebia só `origem` e `csosn` e
completava o resto com regra fixa em C#: toda nota saía com PIS e COFINS em CST 07, e apenas
as situações que se resolvem sem valores eram aceitas.

Agora cada item carrega o quadro tributário completo, e o motor traduz para os grupos de
imposto do XML. Quem decide situação tributária e calcula valores é o backend.

O motor continua responsável por montar o XML, assinar, transmitir, gerar DANFE e QR Code.

## Onde entra no payload

O bloco `imposto` é um campo novo dentro de cada item de `POST /api/nfce/emit`:

```json
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
```

O bloco `imposto` é **obrigatório** em todo item, e dentro dele `icms`, `pis` e `cofins`.
`ipi` é opcional — a maioria das NFC-e não destaca IPI.

Os campos `origem` e `csosn` que existiam no nível do item **saíram do contrato**: a situação
tributária e a origem da mercadoria agora vêm de dentro de `imposto.icms`.

Os nomes dos campos são os mesmos das tags do layout da NF-e (`vBC`, `pICMS`, `pMVAST`, …),
com a caixa preservada.

## Campos

### `icms`

| Campo | Tipo | Descrição |
|---|---|---|
| `situacao` | string | CST (2 dígitos, Regime Normal) ou CSOSN (3 dígitos, Simples Nacional) |
| `origem` | int | Origem da mercadoria, 0 a 8 |
| `modBC` | int? | Modalidade da base de cálculo, 0 a 3. Ausente = 3 (valor da operação) |
| `vBC` | decimal? | Base de cálculo, **já reduzida** quando há `pRedBC` |
| `pRedBC` | decimal? | Percentual de redução da base |
| `pICMS` | decimal? | Alíquota |
| `vICMS` | decimal? | Valor do imposto |
| `modBCST` | int? | Modalidade da base de ST, 0 a 6. Ausente = 4 (margem de valor agregado) |
| `pMVAST` | decimal? | Margem de valor agregado da ST |
| `pRedBCST` | decimal? | Percentual de redução da base de ST |
| `vBCST` | decimal? | Base de cálculo da ST |
| `pICMSST` | decimal? | Alíquota da ST |
| `vICMSST` | decimal? | Valor da ST |
| `vBCSTRet` | decimal? | Base da ST retida anteriormente (CST 60 / CSOSN 500) |
| `vICMSSTRet` | decimal? | Valor da ST retida anteriormente |
| `pFCP` | decimal? | Alíquota do fundo de combate à pobreza |
| `vFCP` | decimal? | Valor do FCP |
| `vBCFCPST` | decimal? | Base do FCP retido por ST |
| `pFCPST` | decimal? | Alíquota do FCP-ST |
| `vFCPST` | decimal? | Valor do FCP-ST |
| `pCredSN` | decimal? | Alíquota de crédito do Simples Nacional |
| `vCredICMSSN` | decimal? | Valor do crédito do Simples |

### `pis` e `cofins`

| Campo | Tipo | Descrição |
|---|---|---|
| `situacao` | string | CST de PIS/COFINS, 2 dígitos |
| `vBC` | decimal? | Base de cálculo — apuração por percentual |
| `pPIS` / `pCOFINS` | decimal? | Alíquota percentual |
| `qBCProd` | decimal? | Quantidade vendida — apuração por quantidade |
| `vAliqProd` | decimal? | Alíquota por unidade de produto |
| `vPIS` / `vCOFINS` | decimal? | Valor da contribuição |

Duas formas de apuração, e o payload escolhe pelos campos que preenche: **percentual**
(`vBC` + alíquota) ou **quantidade** (`qBCProd` + `vAliqProd`). Nunca as duas juntas.

### `ipi` (opcional)

| Campo | Tipo | Descrição |
|---|---|---|
| `situacao` | string | CST de IPI, 2 dígitos |
| `vBC` | decimal? | Base de cálculo |
| `pIPI` | decimal? | Alíquota |
| `vIPI` | decimal? | Valor |
| `cEnq` | string? | Código de enquadramento legal. Ausente = `999` |

## Situações tributárias aceitas

### ICMS — Simples Nacional (CSOSN)

| CSOSN | Grupo no XML | Campos exigidos |
|---|---|---|
| `101` | `ICMSSN101` | `pCredSN`, `vCredICMSSN` |
| `102`, `103`, `300`, `400` | `ICMSSN102` | nenhum — só origem e CSOSN |
| `201` | `ICMSSN201` | `modBCST`, `vBCST`, `pICMSST`, `vICMSST`, `pCredSN`, `vCredICMSSN` |
| `202`, `203` | `ICMSSN202` | `modBCST`, `vBCST`, `pICMSST`, `vICMSST` |
| `500` | `ICMSSN500` | `vBCSTRet`, `vICMSSTRet` |
| `900` | `ICMSSN900` | `modBC`, `vBC`, `pICMS`, `vICMS` |

### ICMS — Regime Normal (CST)

| CST | Grupo no XML | Campos exigidos |
|---|---|---|
| `00` | `ICMS00` | `modBC`, `vBC`, `pICMS`, `vICMS` |
| `10` | `ICMS10` | os de `00` mais `modBCST`, `vBCST`, `pICMSST`, `vICMSST` |
| `20` | `ICMS20` | `modBC`, `pRedBC`, `vBC`, `pICMS`, `vICMS` |
| `30` | `ICMS30` | `modBCST`, `vBCST`, `pICMSST`, `vICMSST` |
| `40`, `41`, `50` | `ICMS40` | nenhum — só origem e CST |
| `51` | `ICMS51` | `modBC`, `vBC`, `pICMS`, `vICMS` |
| `60` | `ICMS60` | `vBCSTRet`, `vICMSSTRet` |
| `70` | `ICMS70` | os de `20` mais `modBCST`, `vBCST`, `pICMSST`, `vICMSST` |
| `90` | `ICMS90` | `modBC`, `vBC`, `pICMS`, `vICMS` |

### PIS e COFINS (CST)

| CST | Grupo no XML | Campos exigidos |
|---|---|---|
| `01`, `02` | `PISAliq` / `COFINSAliq` | `vBC`, alíquota, valor |
| `03` | `PISQtde` / `COFINSQtde` | `qBCProd`, `vAliqProd`, valor |
| `04` a `09` | `PISNT` / `COFINSNT` | nenhum — só o CST |
| `49`, `50`–`56`, `60`–`67`, `70`–`75`, `98`, `99` | `PISOutr` / `COFINSOutr` | uma das duas formas, mais o valor |

### IPI (CST)

| CST | Grupo no XML | Campos exigidos |
|---|---|---|
| `00`, `49`, `50`, `99` | `IPITrib` | `vBC`, `pIPI`, `vIPI` |
| `01`–`05`, `51`–`55` | `IPINT` | nenhum — só o CST |

## O que o motor recusa

O motor recusa quadro que **não consegue montar corretamente** — nunca situação tributária
que apenas não esperava. Na prática:

- campo que a situação exige e não veio;
- base sem alíquota, e o inverso, inclusive nos grupos onde o par é opcional;
- valor que não fecha com base × alíquota, fora da tolerância de um centavo;
- situação não tributada trazendo base, alíquota ou valor;
- situação de "outras operações" trazendo as duas formas de apuração ao mesmo tempo.

Todas voltam como `400` / `VALIDACAO`, com a lista campo a campo:

```json
{
  "codigo": "VALIDACAO",
  "mensagem": "Payload invalido",
  "erros": [
    {
      "campo": "Itens[0].Imposto",
      "mensagem": "ICMS CST 00: essa situacao tributaria exige modBC, vBC, pICMS, vICMS, e o item nao trouxe esse(s) campo(s)"
    }
  ]
}
```

**O motor não calcula.** Item com base e alíquota mas sem o valor do imposto é recusado por
campo faltante — o motor não completa a conta.

O valor do ICMS-ST não passa por conferência aritmética: ele sai de `vBCST × pICMSST` menos o
ICMS próprio, e a subtração muda conforme a situação. Dos campos de ST se confere presença e
coerência entre eles, não o resultado.

## Fora do escopo

- **Partilha interestadual** (`ICMSPart`, `ICMSUFDest`). A NFC-e deste serviço é sempre
  operação interna: `idDest` é fixo em interna e o CFOP é validado como `5xxx`.
- **Cálculo de imposto** — etapa 2 do roteiro, no backend.
- **IBS e CBS** — etapa 6.
- **II e ISSQN** — fora do escopo de NF-e/NFC-e de mercadoria interna.

## Quebra de contrato

Esta é uma mudança **incompatível** com o contrato anterior. Item sem o bloco `imposto` é
recusado:

```json
{
  "codigo": "VALIDACAO",
  "mensagem": "Payload invalido",
  "erros": [
    { "campo": "Itens[0].Imposto", "mensagem": "Quadro tributario do item ('imposto') e obrigatorio" }
  ]
}
```

O que saiu do payload do item:

| Campo | Substituído por |
|---|---|
| `origem` | `imposto.icms.origem` |
| `csosn` | `imposto.icms.situacao` |

O motor não escolhe mais situação tributária a partir do CRT do emitente, e PIS/COFINS não
saem mais fixos em CST 07. O `crt` do emitente continua no payload — ele vai para a tag `CRT`
do XML, mas não influencia mais a montagem dos grupos de imposto.
