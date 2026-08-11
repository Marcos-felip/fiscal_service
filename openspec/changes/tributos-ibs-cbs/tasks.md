> ⚠️ **Antes de qualquer tarefa:** revalide a proposta e os requisitos contra a
> NT vigente. Foram escritos em 11/08/2026 e a tabela de `cClassTrib` e os grupos
> de IBS/CBS mudam entre versões do Informe Técnico.

## 1. Revalidação

- [ ] 1.1 Identificar a NT e a versão do Informe Técnico vigentes
- [ ] 1.2 Conferir qual versão a `Zeus.Net.NFe.NFCe` instalada implementa; atualizar o pacote se necessário
- [ ] 1.3 Rodar `openspec-update-change` sobre esta change com o que foi apurado
- [ ] 1.4 Confirmar o que é obrigatório na data da implementação — em 08/2026 a SEFAZ-MG autorizava sem os grupos

## 2. Contrato

- [ ] 2.1 Bloco de IBS/CBS no item, ao lado dos demais tributos
- [ ] 2.2 Classificação tributária como **dado transportado**, nunca enum fixo no motor
- [ ] 2.3 Imposto Seletivo no item
- [ ] 2.4 Totais de IBS e CBS

## 3. Tradução

- [ ] 3.1 Grupos de IBS e CBS no adapter
- [ ] 3.2 Grupo do Imposto Seletivo
- [ ] 3.3 Totais somados dos itens
- [ ] 3.4 Item sem os grupos continua sendo emitido

## 4. Testes

- [ ] 4.1 Item com IBS/CBS gera os grupos com os valores informados
- [ ] 4.2 Item sem o bloco não gera os grupos
- [ ] 4.3 Totais somados corretamente
- [ ] 4.4 Imposto Seletivo presente e ausente
- [ ] 4.5 **Regressão:** emissão sem os grupos permanece equivalente à de hoje
- [ ] 4.6 `dotnet test` verde

## 5. Documentação

- [ ] 5.1 Registrar no `AGENTS.md` do motor a versão da NT implementada — sem isso a próxima revalidação recomeça do zero
- [ ] 5.2 Publicar o contrato para a change irmã do backend
