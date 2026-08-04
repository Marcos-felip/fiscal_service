---
name: nfce-validation
description: Valida regras de negocio para emissao de NFC-e conforme manual SEFAZ — campos obrigatorios, codigos fiscais e regras brasileiras.
---

Use esta skill quando houver alteracao nos validators, DTOs ou regras de montagem da NFC-e.

## Campos obrigatorios NFC-e (modelo 65)

### Emitente
- CNPJ valido (14 digitos)
- Razao social
- Inscricao estadual
- CRT (1=Simples, 2=Simples excesso, 3=Normal)
- Endereco completo com codigo IBGE do municipio

### Itens
- NCM (8 digitos, tabela NCM IBGE)
- CFOP (4 digitos, tabela CFOP)
- CSOSN (Simples Nacional) ou CST (Regime Normal)
- Unidade comercial (UN, KG, LT, etc.)
- Quantidade > 0
- Valor unitario > 0
- Origem da mercadoria (0-8)
- GTIN ou "SEM GTIN"

### Pagamentos
- Ao menos 1 pagamento
- tPag valido (01=Dinheiro, 03=Credito, 04=Debito, 15=Boleto, 17=PIX, 90=Vale, 99=Outro)
- Soma dos pagamentos = total da nota (tolerancia R$0,01)
- Troco calculado quando pagamento em dinheiro excede o total

### Cancelamento
- Justificativa minimo 15 caracteres
- Apenas NFC-e AUTORIZADO pode ser cancelado
- Prazo de cancelamento (varia por UF, geralmente ate 24h)

### Rejeicoes comuns
- 203: CNPJ do emitente invalido
- 204: Inscricao estadual invalida
- 225: NCM invalido
- 226: CFOP invalido
- 501: Rejeicao: prazo de cancelamento excedido
- 573: Duplicidade de nota

## Regras brasileiras
- Validacao de CNPJ (mod 11, pesos 5,4,3,2,9,8,7,6,5,4,3,2)
- Validacao de IE por UF (cada estado tem regra propria)
- Codigo IBGE do municipio (tabela IBGE)

Referencias:
- Manual NFC-e SEFAZ: http://www.nfe.fazenda.gov.br/portal/
- Tabela NCM: https://www.gov.br/receitafederal/pt-br/assuntos/aduana/manuais/ncm
