## Why

**Venda paga em PIX não emite.** Em 13/08/2026, a primeira venda com pagamento
em PIX foi recusada pela SEFAZ com:

> **Rejeição 391** — Não informados os dados do cartão de crédito / débito nas
> Formas de Pagamento da Nota Fiscal

A mensagem fala em cartão, e o pagamento era PIX. A causa é a mesma: o layout
exige o grupo `card` (YA04) para **todo pagamento eletrônico**, não só para
cartão, e o motor nunca o monta — `detPag` sai apenas com `indPag`, `tPag` e
`vPag`.

Isso não é caso de borda. Uma lanchonete em 2026 recebe metade do faturamento em
PIX, e hoje **nenhuma dessas vendas vira nota**: a rejeição vem depois de a
numeração já ter sido consumida, então cada tentativa também queima um número.

O mesmo vale para cartão de crédito, débito, vale-refeição e boleto — todos na
lista do grupo. Só não foram alcançados ainda porque as vendas de teste usaram
dinheiro.

## What Changes

- **`detPag` passa a levar o grupo `card`** quando a forma de pagamento é
  eletrônica, conforme a lista do layout: cartão de crédito e débito, os quatro
  vales, boleto e PIX.
- **`tpIntegra` = 2 (não integrado).** É o valor verdadeiro: o sistema não fala
  com TEF nem com credenciadora. Com 2, o CNPJ da credenciadora, a bandeira e o
  código de autorização deixam de ser exigidos — declarar 1 sem ter a integração
  produziria rejeição por campo obrigatório ausente.
- **Dinheiro, cheque, crédito de loja, sem pagamento e outros continuam sem o
  grupo**, porque informá-lo fora da lista é rejeição por grupo indevido.
- Vale para **NFC-e e NF-e**: a montagem dos pagamentos é a mesma nos dois.

## Capabilities

### Modified Capabilities
- `fiscal-engine-payment`: o detalhe de pagamento passa a distinguir eletrônico
  de não eletrônico e a montar o grupo exigido para o primeiro.

## Impact

- `FiscalService.Infrastructure/DFe/DFeNetAdapter.cs` — montagem de `detPag`,
  compartilhada pelos dois modelos.
- **Sem mudança de contrato.** O backend continua mandando `tipo: "pix"`; quem
  decide o grupo é o motor, que já é quem traduz a forma de pagamento.
- **Sem mudança no backend nem no frontend.**
