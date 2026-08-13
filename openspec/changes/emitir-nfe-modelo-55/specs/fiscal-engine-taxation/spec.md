## ADDED Requirements

### Requirement: Totais do documento somados a partir dos itens
O motor SHALL montar os totais do documento somando os valores do quadro
tributário de cada item — base de cálculo, ICMS, substituição tributária, fundo
de combate à pobreza, PIS, COFINS e IPI. O motor SHALL NOT escrever zero em
totais cujos itens declarem valor.

Somar não é calcular: os valores continuam vindo prontos do backend. O que muda é
que o total passa a refletir os itens, em vez de ser constante.

#### Scenario: Nota com ICMS destacado
- **WHEN** os itens declaram base de cálculo e valor de ICMS
- **THEN** o total do documento traz a soma dessas bases e desses valores

#### Scenario: Nota sem imposto a destacar
- **WHEN** todos os itens usam situação tributária que não comporta valores, como CSOSN 102
- **THEN** os totais de imposto ficam em zero, porque não há o que somar

#### Scenario: Contribuições somadas
- **WHEN** os itens declaram valor de PIS e de COFINS
- **THEN** o total do documento traz a soma de cada contribuição separadamente
