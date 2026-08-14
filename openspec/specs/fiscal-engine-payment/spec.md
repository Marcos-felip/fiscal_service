# fiscal-engine-payment Specification

## Purpose
TBD - created by archiving change grupo-card-em-pagamento-eletronico. Update Purpose after archive.
## Requirements
### Requirement: Pagamento eletrônico leva o grupo de cartões
O motor SHALL montar o grupo `card` no detalhe de pagamento sempre que a forma
for eletrônica — cartão de crédito, cartão de débito, os vales, boleto e PIX — e
SHALL NOT montá-lo para dinheiro, cheque, crédito de loja, sem pagamento ou
outros.

A SEFAZ recusa a nota com a rejeição 391 quando o grupo falta numa forma
eletrônica, e recusa por grupo indevido quando ele aparece numa forma que não o
comporta. A regra vale para NFC-e e NF-e.

#### Scenario: Venda paga em PIX
- **WHEN** uma nota é emitida com pagamento em PIX
- **THEN** o XML contém o grupo de cartões dentro do detalhe daquele pagamento

#### Scenario: Venda paga em cartão
- **WHEN** uma nota é emitida com cartão de crédito ou de débito
- **THEN** o XML contém o grupo de cartões

#### Scenario: Venda paga em dinheiro
- **WHEN** uma nota é emitida com pagamento em dinheiro
- **THEN** o XML **não** contém o grupo de cartões

#### Scenario: Formas de pagamento misturadas
- **WHEN** a nota tem um pagamento em dinheiro e outro em PIX
- **THEN** apenas o detalhe do PIX leva o grupo de cartões

### Requirement: Integração declarada como não integrada
O motor SHALL declarar o tipo de integração do pagamento como **não integrado ao
sistema de automação**, e SHALL NOT informar CNPJ de credenciadora, bandeira ou
código de autorização.

É o que corresponde à realidade: o sistema não se comunica com TEF nem com
credenciadora. Declarar integração existente tornaria obrigatórios campos que
não há como preencher, trocando uma rejeição por outra.

#### Scenario: Pagamento eletrônico sem dados de credenciadora
- **WHEN** o grupo de cartões é montado
- **THEN** ele traz apenas o tipo de integração, com o valor "não integrado"

