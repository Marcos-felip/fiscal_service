> ⚠️ Requisitos escritos em 11/08/2026 contra o entendimento da reforma naquele
> momento. **Revalide contra a NT vigente** antes de implementar.

## ADDED Requirements

### Requirement: Grupos de IBS e CBS por item
O motor SHALL montar os grupos de IBS e CBS de cada item a partir do que o
payload informar — situação tributária, classificação tributária, alíquotas e
valores — sem escolher classificação por conta própria.

#### Scenario: Item com IBS e CBS
- **WHEN** um item chega com a situação tributária, a classificação e as alíquotas de IBS e CBS
- **THEN** o XML contém os grupos correspondentes com esses valores

#### Scenario: Item sem os grupos
- **WHEN** um item chega sem o bloco de IBS/CBS
- **THEN** o XML é montado sem esses grupos, e a emissão prossegue

#### Scenario: Classificação tributária não é decidida pelo motor
- **WHEN** um item chega com classificação tributária informada
- **THEN** o motor a transporta para o XML sem substituí-la por regra própria

### Requirement: Totais de IBS e CBS
O motor SHALL compor os totais de IBS e CBS a partir dos valores dos itens.

#### Scenario: Totais somados
- **WHEN** uma nota com itens tributados por IBS e CBS é emitida
- **THEN** o grupo de totais traz os valores somados dos itens

### Requirement: Imposto Seletivo
O motor SHALL montar o grupo do Imposto Seletivo quando o item o informar.

#### Scenario: Produto sujeito ao Seletivo
- **WHEN** um item chega com os dados do Imposto Seletivo
- **THEN** o XML contém o grupo correspondente

#### Scenario: Produto não sujeito
- **WHEN** o item não informa Imposto Seletivo
- **THEN** o grupo não é gerado

### Requirement: Convivência com o regime anterior
Durante a transição, o motor SHALL emitir tanto notas com os grupos da reforma
quanto notas sem eles, conforme o payload.

#### Scenario: Nota no regime anterior
- **WHEN** uma nota é emitida sem os grupos da reforma
- **THEN** o XML sai como antes desta capacidade existir

#### Scenario: Regressão da emissão atual
- **WHEN** uma NFC-e equivalente às emitidas hoje é processada
- **THEN** o XML gerado permanece equivalente
