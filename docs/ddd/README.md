# Documentação DDD

**Quadro no Miro:** https://miro.com/app/board/uXjVEXtrgrQ=/?share_link_id=578425858571

| Arquivo | Conteúdo |
|---|---|
| [`event-storming.png`](event-storming.png) | Exportação do Event Storming do Miro |
| [`domain-storytelling.png`](domain-storytelling.png) | Exportação do Domain Storytelling do Miro |
| [`event-storming.md`](event-storming.md) | Event Storming em texto dos três fluxos (criação da OS, acompanhamento da OS, peças e insumos): atores, comandos, eventos e políticas, como estão no código |
| [`modelo-de-dominio.md`](modelo-de-dominio.md) | Bounded contexts, agregados, entidades, objetos de valor, regras de negócio e a relação com o código |
| [`linguagem-ubiqua.md`](linguagem-ubiqua.md) | Linguagem ubíqua do MVP, com o nome de cada termo no código |
| [`glossario.pdf`](glossario.pdf) | Glossário completo do domínio da oficina, mais amplo que o MVP |
| [`../arquitetura/c4.png`](../arquitetura/c4.png) e [`../arquitetura/workspace.dsl`](../arquitetura/workspace.dsl) | Diagramas C4, níveis 1 a 3 |

## Do Miro para o código

O Event Storming do Miro modela a oficina inteira, incluindo partes que **ficaram fora do MVP**. A tabela mostra o que mudou na implementação. A lista completa está em [`modelo-de-dominio.md`](modelo-de-dominio.md#decisões-do-mvp).

| No Event Storming | No código (MVP) |
|---|---|
| Orçamento como agregado próprio | O orçamento faz parte do agregado `OrdemDeServico` (itens + total) |
| Aprovar orçamento e depois iniciar execução | Aprovar já coloca a OS em `EmExecucao` |
| Baixa de cada peça ao ser aplicada | Baixa de todas as peças da OS de uma vez, ao finalizar |
| Orçamento complementar e aprovação parcial | Fora do MVP |
| Controle de qualidade como etapa | Fora do MVP |
| Notificações ao cliente | Fora do MVP: o cliente consulta a OS pela API |
| Saldo mínimo e ajuste por contagem | Fora do MVP |
