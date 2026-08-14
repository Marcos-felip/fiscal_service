# Desenho — NF-e modelo 55 no motor

Decisões tomadas em 13/08/2026, contra o código real e contra os dois recortes
(operação interna, destinatário pessoa jurídica).

## D1 — Caso de uso próprio, domínio compartilhado onde é honesto

`EmitirNfe` é use case, DTO e validador **separados** de `EmitirNfce`. As duas
requisições divergem no que é obrigatório: a NFC-e exige CSC e aceita consumidor
anônimo; a NF-e recusa CSC e exige destinatário completo. Um DTO só, com metade
dos campos opcionais, transformaria toda regra de obrigatoriedade em `if` de
modelo — e a validação deixaria de ser declarativa.

O que **é** compartilhado, porque é genuinamente o mesmo: `ItemFiscal`,
`ImpostoItem` e todos os `Situacao*`, `Pagamento`, `Emitente`, `Endereco`,
`Certificado`, o `TradutorImposto` e a leitura de certificado.

**`NfceItem` passa a se chamar `ItemFiscal`.** Depois da etapa 1 ele carrega o
quadro tributário e não tem nada de específico da NFC-e. Uma `Nfe` composta de
`NfceItem` seria mentira de nomenclatura em código novo — e nomenclatura errada
custa em toda leitura futura. É renomeação pura, sem mudança de comportamento.

O DTO `ItemNfceDto` **não** é renomeado: o nome dele aparece no `swagger.json`,
que o backend usa para conferir o contrato servido. Renomear DTO é mexer no
contrato publicado para ganhar estética; renomear entidade interna, não.

## D2 — Destinatário: uma classe, não um campo opcional

`Destinatario` continua como está, para a NFC-e. A NF-e ganha
`DestinatarioNfe`, com endereço e `indIEDest` **obrigatórios** por construção —
não validados depois. Um destinatário de NF-e sem endereço não deve conseguir
existir como objeto.

`indIEDest` tem os três valores da NT: `1` contribuinte, `2` isento de IE, `9`
não contribuinte. **PJ não implica contribuinte** — prestadora de serviço é
pessoa jurídica e não é contribuinte de ICMS. O recorte garante CNPJ, não
inscrição estadual. Contribuinte (`1`) exige IE; os outros dois a recusam.

## D3 — O recorte vira recusa nomeada, no validador

Nada do que está fora do recorte é ignorado em silêncio. O validador recusa, com
mensagem dizendo o que aconteceu:

| Situação | Recusa |
|---|---|
| Destinatário com CPF | "A NF-e exige destinatário pessoa jurídica; para pessoa física, emita NFC-e" |
| UF do destinatário ≠ UF do emitente | "Operação interestadual está fora do escopo atual" |
| CFOP fora de `5xxx` | "Apenas CFOP de operação interna é aceito" |
| `finNFe` ≠ normal | "Apenas a finalidade normal é aceita; complementar, ajuste e devolução ainda não" |
| `tpNF` ≠ saída | "Apenas nota de saída é aceita" |
| CSC informado | "O CSC é exclusivo da NFC-e" |
| Contribuinte sem IE | "Destinatário contribuinte exige inscrição estadual" |

O motor **recusa o que não sabe montar** — nunca monta aproximando. Um XML
interestadual sem DIFAL é aceito pela SEFAZ e escritura errado: o contador
recebe o arquivo e não tem como perceber.

## D4 — `tpNF`, `finNFe`, `indFinal` e `indPres` vêm do backend

Hoje o adapter crava os quatro. Na NFC-e o valor fixo está certo (saída, normal,
consumidor final, presencial). Na NF-e, dois deles variam de verdade:

- **`indFinal`** distingue venda para revenda (`0`) de venda para consumo (`1`).
  O mesmo produto, para o mesmo cliente, muda conforme o destino da mercadoria —
  e quem sabe isso é quem lançou a venda, não o motor.
- **`indPres`** distingue balcão de venda a distância.

`tpNF` e `finNFe` entram no contrato mesmo aceitando um valor só, porque a
alternativa é o backend não conseguir nomear o que pediu quando o recorte abrir.

## D5 — `ICMSTot` somado dos itens, nos dois modelos

`MontarTotal` hoje escreve zero em `vBC`, `vICMS`, `vST`, `vPIS`, `vCOFINS`,
`vIPI` e `vFCP`, somando apenas `vProd`. **É defeito, não simplificação.** A
SEFAZ confere o total contra o somatório dos itens; nota com ICMS destacado e
`vICMS` zerado é rejeitada.

Não foi alcançado até hoje porque toda NFC-e emitida usa CSOSN 102, cujo grupo
não tem valor algum — o zero acerta por coincidência. **A correção vale para os
dois modelos**, e é o único ponto desta change que muda comportamento da NFC-e.

A soma sai do `ImpostoItem` de cada item, que já é a fonte da verdade desde a
etapa 1. O motor continua sem calcular imposto: somar o que o backend mandou não
é decidir nada.

## D6 — DANFE em HTML, e por quê

O DANFE do modelo 55 **não existe** na biblioteca de DANFE atual: o
`Zeus.Net.NFe.Danfe.QuestPdf` só traz `DanfeNfceDocument` (cupom) e o layout de
eventos. As três alternativas foram medidas em contêiner Linux, que é onde o
motor roda:

| Caminho | Resultado |
|---|---|
| `Zeus.Net.NFe.Danfe.PdfClown` | **Não funciona.** `System.Drawing.Common` é Windows-only no .NET 8, e a chave `EnableUnixSupport` que existia no .NET 6 foi removida. Falha em `TypeInitializationException: Gdip` |
| `Zeus.Net.NFe.Danfe.Html` | **Funciona.** Gerou o DANFE do modelo 55 no contêiner, com o código de barras embutido em base64 |
| Layout próprio em QuestPDF | Funcionaria — QuestPDF é Skia, já é dependência. Custa o layout inteiro do DANFE escrito à mão, incluindo Code128 |

**Escolha: HTML.** O DANFE é representação gráfica; o documento é o XML. HTML
imprime do navegador, sem Chromium no contêiner e sem layout à mão.

O custo é de contrato: a resposta ganha `danfeContentType`, porque a NFC-e
devolve `application/pdf` e a NF-e devolve `text/html`. O backend precisa dele
para gravar e servir o arquivo — hoje ele assume PDF em todo lugar.

Se um dia o PDF for exigido, o caminho barato é PuppeteerSharp renderizando este
mesmo HTML — o que muda é o tamanho da imagem (Chromium), não o layout.

## D7 — Cancelamento e consulta não ganham código

`RecepcaoEventoCancelamento` e `NfeConsultaProtocolo` não distinguem modelo: a UF
e o CNPJ saem da própria chave de acesso. As rotas `/api/nfe/cancel` e
`/api/nfe/consulta` existem por clareza de contrato e **delegam aos handlers que
já existem**. A única diferença real é `ModeloDocumento` na configuração do
serviço, que vem da chave.
