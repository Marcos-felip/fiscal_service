# Contrato dos eventos fiscais

Header `X-Api-Key` em todas as chamadas, como no resto do motor.

| Ato | Rota |
|---|---|
| Cancelamento | `POST /api/nfce/cancel` · `POST /api/nfe/cancel` |
| Carta de Correção | `POST /api/eventos/carta-correcao` |
| Inutilização de faixa | `POST /api/eventos/inutilizar` |

O cancelamento continua onde sempre esteve. Movê-lo para `/api/eventos/`
quebraria o backend sem ganho nenhum.

---

## Carta de Correção — `POST /api/eventos/carta-correcao`

Evento 110110. Corrige erro de digitação em campo **não essencial** de nota já
autorizada.

```jsonc
{
  "chaveAcesso": "3126...",
  "correcao": "Corrigir o nome do bairro do destinatário",  // 15 a 1000 caracteres
  "sequenciaEvento": 1,                                      // 1 a 20
  "cpfCnpj": "51720322000146",                               // autor: o emitente
  "certificadoBase64": "...",
  "certificadoSenha": "...",
  "ambiente": "homologacao"
}
```

**Resposta**

```jsonc
{
  "sucesso": true,
  "protocolo": "131260000000001",
  "xmlEventoBase64": "...",
  "condicaoDeUso": "A Carta de Correcao e disciplinada pelo paragrafo 1o-A ..."
}
```

### O que o motor confere — e o que ele não confere

| Regra | Onde vive |
|---|---|
| Texto de 15 a 1000 caracteres | **motor** |
| `sequenciaEvento` entre 1 e 20 | **motor** |
| Sequência não repete nem salta | **backend** |
| Máximo de 20 correções na nota | **backend** |
| A correção não altera valores, datas ou as partes | **ninguém — é legal** |

As duas do backend exigem o **histórico da nota**, e o motor é stateless: ele não
sabe quantas CC-e aquele documento já teve. Quem sabe é quem tem
`fiscal_document_events`.

A terceira **não é verificável por código**. A CC-e é texto livre; deduzir
intenção de uma frase seria heurística que ou recusa correção legítima
("corrigir o endereço do transportador") ou aprova a ilegítima com ar de
validada — que é pior, porque dá ao lojista a impressão de que o sistema
conferiu.

**`condicaoDeUso` é o que o layout oferece no lugar.** É o texto legal fixo que
vai no XML (`xCondUso`) e volta no retorno — **inclusive na recusa**, porque quem
confirma a correção precisa lê-lo antes, não depois de dar certo. Mostre-o na
tela de confirmação.

---

## Inutilização — `POST /api/eventos/inutilizar`

Regulariza faixa de numeração **reservada e nunca usada**: falha definitiva na
emissão, salto na sequência. Buraco na numeração é apontamento.

```jsonc
{
  "cnpj": "51720322000146",
  "ano": 2026,
  "modelo": 55,              // 55 NF-e · 65 NFC-e
  "serie": 1,
  "numeroInicial": 1,
  "numeroFinal": 1,          // igual ao inicial: um número só é o caso comum
  "justificativa": "Numeracao reservada e nao utilizada por falha na emissao",
  "uf": "MG",
  "certificadoBase64": "...",
  "certificadoSenha": "...",
  "ambiente": "homologacao"
}
```

**Resposta**

```jsonc
{
  "sucesso": true,
  "protocolo": "131260000000002",
  "xmlInutilizacaoBase64": "..."
}
```

### Por que o retorno é diferente dos eventos

A inutilização **não é evento**. Ela age sobre uma **faixa**, não sobre um
documento: não há chave de acesso, nem `retEvento`, nem protocolo de evento — o
serviço da SEFAZ é outro (`NfeInutilizacao`, não `RecepcaoEvento`). Padronizar os
dois retornos num só esconderia isso.

Pela mesma razão a **UF vem no pedido**: sem chave de acesso, não há de onde
deduzi-la.

### O que o backend precisa fazer antes de chamar

O motor valida forma — faixa coerente, justificativa de 15 a 255, modelo 55 ou
65. O que ele **não** pode verificar é se aqueles números foram realmente usados:
ele não conhece a base.

**Inutilizar um número que tem documento autorizado é o erro caro aqui.** O
backend tem `fiscal_documents` com série e número; deve recusar a faixa que
colida com documento autorizado ou cancelado, nomeando o número e a chave. E pode
ir além: os buracos são **calculáveis** — para uma série, os números de 1 a
`proximoNumero - 1` que não têm documento são exatamente os candidatos. Sugeri-los
evita que alguém digite a faixa errada.
