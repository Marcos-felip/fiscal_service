# Design — Contrato tributário do item

## A decisão

**O motor traduz; quem decide imposto é o backend.**

Hoje a decisão está enterrada em C#, num repositório separado, sem ninguém
conseguir sobrescrever. Suportar um CST novo exige mexer em dois repositórios e
fazer dois deploys. Depois desta change, é dado.

O motor continua responsável por: montar o XML, assinar, transmitir, gerar DANFE
e QR Code. Deixa de ser responsável por: escolher situação tributária e inventar
valor que não recebeu.

## Forma do bloco tributário

```
item: {
  ...campos atuais,
  imposto: {
    icms:   { situacao, origem, modBC, vBC, pICMS, vICMS,
              modBCST, pMVAST, vBCST, pICMSST, vICMSST,
              vBCSTRet, vICMSSTRet, pFCP, vFCP,
              pCredSN, vCredICMSSN },
    ipi:    { situacao, vBC, pIPI, vIPI },
    pis:    { situacao, vBC, pPIS, vPIS, qBCProd, vAliqProd },
    cofins: { situacao, vBC, pCOFINS, vCOFINS, qBCProd, vAliqProd }
  }
}
```

Campos opcionais por natureza: qual subconjunto é obrigatório depende do CST, e
essa regra vira validação declarada, não `if` espalhado no adapter.

`qBCProd`/`vAliqProd` existem porque PIS e COFINS podem ser por **quantidade**,
não só por percentual. Esquecer isso é descobrir tarde que o contrato não modela
um caso real.

## Por que o motor ainda valida

Poderia-se argumentar que, se o backend decide, o motor deveria aceitar tudo. Não.

O motor valida **coerência interna do quadro recebido** — não política fiscal:

- CST que exige base e alíquota veio sem elas
- `vICMS` que não fecha com `vBC × pICMS` dentro da tolerância
- CST de ST sem os campos de ST

Isso é diferente de "CSOSN 900 não é suportado". A primeira é o motor se
recusando a montar XML incoerente; a segunda era o motor decidindo o que o
emitente pode fazer.

A regra prática: **o motor recusa o que não consegue montar corretamente, nunca o
que não gosta.**

## Compatibilidade durante a transição

Backend e motor não sobem no mesmo instante. Por uma versão, item sem `imposto`
continua aceito e cai na regra atual — ICMS por origem+CSOSN, PIS/COFINS em 07 —
com log de contrato antigo.

Isso não é permanente: a última tarefa desta change é remover o fallback depois
que o backend estiver publicando o bloco. Fallback que fica é fallback que vira
comportamento padrão, e daqui a um ano ninguém sabe por que existe.

O log é o que torna a remoção segura: dá para conferir se ainda chega alguém pelo
caminho antigo antes de apagar.

## Testes: os primeiros do repositório

`tests/` tem os projetos `UnitTests` e `IntegrationTests` sem um único arquivo de
teste. Esta change cria os primeiros, e não por completude: **o que está sendo
escrito aqui é regra fiscal**, onde o erro não aparece na emissão — aparece na
escrituração do contador, meses depois.

O mínimo:

- Um teste por CST/CSOSN suportado, conferindo o grupo XML gerado
- Quadro incoerente recusado com mensagem clara
- Item sem `imposto` resolvido pelo fallback enquanto ele existir
- O caso da nota 7 (CSOSN 102, sem valores) continuando a gerar XML idêntico ao
  de hoje — regressão do que já funciona

O último é o mais importante da lista. A emissão de NFC-e funciona hoje; esta
change não pode quebrá-la.

## O que fica de fora

- **Cálculo de imposto.** O motor não calcula base nem aplica alíquota — recebe
  pronto. Quem calcula é o backend (etapa 2 do roteiro).
- **IBS e CBS.** São grupos novos, da reforma, e têm etapa própria (6). O
  desenho do bloco tributário deve deixar espaço para eles sem forçar a barra
  agora.
- **II e ISSQN.** Fora do escopo de NF-e/NFC-e de mercadoria interna.
