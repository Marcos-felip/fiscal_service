## 1. Contrato

- [ ] 1.1 `ImpostoDto` com `IcmsDto`, `IpiDto`, `PisDto` e `CofinsDto`
- [ ] 1.2 `IcmsDto`: situação, origem, `modBC`, `vBC`, `pICMS`, `vICMS`, `modBCST`, `pMVAST`, `vBCST`, `pICMSST`, `vICMSST`, `vBCSTRet`, `vICMSSTRet`, `pFCP`, `vFCP`, `pCredSN`, `vCredICMSSN`
- [ ] 1.3 `PisDto` e `CofinsDto` com as duas formas: percentual (`vBC` + `pPIS`) e quantidade (`qBCProd` + `vAliqProd`)
- [ ] 1.4 `ItemDto` recebe `Imposto` opcional enquanto o fallback existir
- [ ] 1.5 Documentar no `AGENTS.md` do motor o contrato novo e a data prevista de remoção do fallback

## 2. Domínio

- [ ] 2.1 Value objects de situação tributária para ICMS (CST e CSOSN), IPI, PIS e COFINS
- [ ] 2.2 Tabela declarando, por situação, quais campos são obrigatórios — a regra num lugar só, fora do adapter
- [ ] 2.3 Validação de coerência: base sem alíquota, valor divergente de base × alíquota, ST sem campos de ST
- [ ] 2.4 Tolerância de centavos igual à já usada nos somatórios

## 3. Tradução para o XML

- [ ] 3.1 Reescrever `MontarImposto` como tradutor, sem escolher CST
- [ ] 3.2 ICMS do Simples: ICMSSN101, 102, 201, 202, 203, 500 e 900
- [ ] 3.3 ICMS do Regime Normal: ICMS00, 10, 20, 30, 40, 51, 60, 70, 90
- [ ] 3.4 Grupos de ST, FCP e partilha interestadual
- [ ] 3.5 IPI, quando informado
- [ ] 3.6 PIS e COFINS pelas duas formas, **removendo o CST 07 fixo**
- [ ] 3.7 `ValidacoesFiscais` deixa de manter lista curta de CST/CSOSN suportado

## 4. Compatibilidade

- [ ] 4.1 Item sem `imposto` continua resolvido pela regra anterior
- [ ] 4.2 Log de nível informativo registrando uso do contrato antigo, com a chave da nota
- [ ] 4.3 **Última tarefa da change:** remover o fallback e tornar `imposto` obrigatório, depois de confirmar pelo log que ninguém mais usa o caminho antigo

## 5. Testes — os primeiros do repositório

- [ ] 5.1 Criar a estrutura em `tests/FiscalService.UnitTests` (hoje só existe andaime)
- [ ] 5.2 Um teste por situação tributária suportada, conferindo o grupo XML gerado
- [ ] 5.3 Quadro incoerente recusado, com a mensagem certa: falta de campo, valor divergente, ST incompleta
- [ ] 5.4 PIS/COFINS por quantidade
- [ ] 5.5 **Regressão:** CSOSN 102 sem valores gera XML equivalente ao de hoje
- [ ] 5.6 Item sem bloco tributário resolvido pelo fallback, enquanto ele existir
- [ ] 5.7 `dotnet test` verde

## 6. Handoff

- [ ] 6.1 Publicar o contrato novo para a change irmã do backend
- [ ] 6.2 Registrar em `README.md`/`AGENTS.md` que o motor não decide mais imposto — quem decide é o backend
