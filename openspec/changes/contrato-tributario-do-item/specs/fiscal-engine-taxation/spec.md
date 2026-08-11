## ADDED Requirements

### Requirement: O item carrega o quadro tributário completo
O contrato de emissão SHALL aceitar, por item, a situação tributária, a base de
cálculo, a alíquota e os valores de ICMS, IPI, PIS e COFINS. O motor SHALL montar
os grupos de imposto do XML a partir desses dados e SHALL NOT escolher situação
tributária por conta própria.

#### Scenario: Quadro completo traduzido
- **WHEN** um item chega com CST de ICMS 00, base, alíquota e valor
- **THEN** o XML gerado contém o grupo ICMS00 com exatamente esses valores

#### Scenario: PIS e COFINS vêm do payload
- **WHEN** um item chega com CST de PIS 04 e CST de COFINS 04
- **THEN** o XML carrega esses códigos, e não um valor fixo definido pelo motor

#### Scenario: Situação sem valores
- **WHEN** um item chega com CSOSN 102, que não comporta valores de ICMS
- **THEN** o XML gera o grupo ICMSSN102 apenas com origem e CSOSN

### Requirement: Situações tributárias antes recusadas passam a ser possíveis
O motor SHALL aceitar as situações tributárias que exigem base e alíquota, agora
que o payload as traz: CSOSN `101`, `201`, `202`, `203`, `900` e CST de ICMS
`00`, `10`, `20`, `51`, `60`, `70`, `90`, além dos já suportados.

#### Scenario: Simples com permissão de crédito
- **WHEN** um item chega com CSOSN 101, `pCredSN` e `vCredICMSSN`
- **THEN** o XML gera ICMSSN101 com o crédito informado, que o destinatário poderá aproveitar

#### Scenario: Substituição tributária
- **WHEN** um item chega com CST 10, com os valores de ICMS próprio e de ST
- **THEN** o XML gera o grupo com ICMS e ICMSST preenchidos

#### Scenario: Regime Normal tributado integralmente
- **WHEN** um emitente de Regime Normal envia um item com CST 00
- **THEN** a emissão é possível — antes era recusada por falta de valores no contrato

### Requirement: O motor recusa quadro incoerente, não política fiscal
O motor SHALL recusar item cujo quadro tributário não possa ser montado
corretamente e SHALL explicar o que falta. O motor SHALL NOT recusar situação
tributária apenas por não ser a esperada.

#### Scenario: CST que exige valores chega sem eles
- **WHEN** um item chega com CST 00 sem base de cálculo ou sem alíquota
- **THEN** a emissão é recusada com mensagem indicando qual campo falta para aquele CST

#### Scenario: Valor que não fecha com base e alíquota
- **WHEN** o valor de ICMS informado diverge de base × alíquota além da tolerância de centavos
- **THEN** a emissão é recusada apontando a divergência

#### Scenario: ST sem os campos de ST
- **WHEN** um item chega com CST de substituição tributária sem os valores de ST
- **THEN** a emissão é recusada indicando os campos exigidos

#### Scenario: PIS por quantidade
- **WHEN** um item chega com PIS calculado por quantidade (`qBCProd` e `vAliqProd`)
- **THEN** o XML é montado por quantidade, sem exigir alíquota percentual

### Requirement: Compatibilidade temporária com o contrato antigo
Enquanto o fallback existir, o motor SHALL aceitar item sem o bloco tributário,
resolvendo-o pela regra anterior, e SHALL registrar em log que a requisição usou
o contrato antigo.

#### Scenario: Item sem bloco tributário
- **WHEN** um item chega sem o bloco de imposto, apenas com origem e CSOSN
- **THEN** o XML é montado pela regra anterior e o log registra o uso do contrato antigo

#### Scenario: Regressão da emissão atual
- **WHEN** uma nota é emitida com CSOSN 102 e sem valores, como as de hoje
- **THEN** o XML gerado é equivalente ao produzido antes desta change

### Requirement: O motor não calcula imposto
O motor SHALL NOT calcular base de cálculo, aplicar alíquota nem derivar valor de
imposto. Todo valor que entra no XML SHALL vir do payload.

#### Scenario: Ausência de cálculo
- **WHEN** um item chega com base e alíquota mas sem o valor do imposto
- **THEN** a emissão é recusada por campo faltante, e o motor não calcula o valor para completar
