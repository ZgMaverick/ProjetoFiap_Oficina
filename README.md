# Oficina Mecânica — Tech Challenge Fase 1 (SOAT)


## Objetivo

MVP do back-end de um sistema de atendimento e execução de serviços para uma oficina mecânica. O sistema substitui as anotações manuais e as planilhas por uma API que controla ordens de serviço, clientes, veículos, serviços e o estoque de peças, e permite que o cliente acompanhe e aprove o orçamento.

## Solução

- **Ordem de Serviço (OS):** o cliente é identificado pelo CPF/CNPJ (e cadastrado na hora, se for novo), o veículo pela placa. Depois entram os serviços e as peças/insumos, e o orçamento é calculado automaticamente e enviado ao cliente para aprovação.
- **Status:** `Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, além de `Cancelada`. O status muda sozinho conforme as ações (enviar orçamento, aprovar, finalizar...), e o domínio bloqueia transições inválidas. Toda mudança fica no histórico da OS.
- **Cliente:** consulta a OS e aprova ou recusa o orçamento pela API, sem login, informando o número da OS e o CPF/CNPJ.
- **Estoque:** a peça é reservada quando entra no orçamento e baixada quando a OS é finalizada. Se faltar saldo, a inclusão é recusada.
- **Gestão:** CRUD de clientes, veículos, serviços e peças/insumos; listagem e detalhe das OS; tempo médio de execução.
- **Segurança:** JWT nas APIs administrativas; validação de CPF/CNPJ (dígitos verificadores) e de placa (padrão antigo e Mercosul).

## Arquitetura

Monólito em camadas, seguindo DDD:

```
Api  ──►  Application  ──►  Domain
 │             ▲
 └──►  Infrastructure
```

| Camada | Projeto | Responsabilidade |
|---|---|---|
| Apresentação | `Oficina.Api` | Controllers REST, JWT, Swagger, tratamento de erros |
| Aplicação | `Oficina.Application` | Casos de uso (um serviço para a OS e um para cada cadastro) e interfaces de repositório |
| Domínio | `Oficina.Domain` | Entidades, agregados (`OrdemDeServico`, `PecaInsumo`), objetos de valor (`Documento`, `Placa`) e regras de negócio. Não depende de nenhuma outra camada |
| Infraestrutura | `Oficina.Infrastructure` | EF Core, repositório genérico, unidade de trabalho, migrations |

Cada operação é gravada numa única transação (`IUnidadeDeTrabalho`). A reserva de estoque usa controle de concorrência otimista: se duas OS disputarem a última peça, a segunda recebe `409`.

O diagrama C4 (níveis 1 a 3) está em [`docs/arquitetura/workspace.dsl`](docs/arquitetura/workspace.dsl). Para abrir no Structurizr:

```bash
docker run -it --rm -p 8081:8080 -v "$(pwd)/docs/arquitetura:/usr/local/structurizr" structurizr/lite
```

## Tecnologias

- C# / .NET 10 (ASP.NET Core Web API)
- Entity Framework Core 10 + Npgsql
- PostgreSQL 17
- JWT Bearer
- OpenAPI + Swagger UI
- xUnit, EF Core InMemory, `Microsoft.AspNetCore.Mvc.Testing`, Coverlet, ReportGenerator
- Docker e Docker Compose
- SonarQube, Trivy e `dotnet list package --vulnerable` (análise de vulnerabilidades)

## Banco de dados: PostgreSQL

- Os dados são relacionais por natureza (cliente → veículo → OS → itens), e o enunciado pede tratamento de transações. O PostgreSQL oferece transações ACID completas.
- A coluna de sistema `xmin` serve como versão da linha para concorrência otimista no estoque, sem precisar de uma coluna extra.
- É gratuito e open source, tem imagem oficial leve no Docker e um provider maduro para o EF Core (Npgsql).

## Pré-requisitos

- **Para rodar:** [Docker Desktop](https://www.docker.com/products/docker-desktop/), e mais nada.
- **Para desenvolver e rodar os testes:** [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Como executar

As senhas e a chave do JWT não ficam no repositório. Elas vêm de um arquivo `.env`, que está no `.gitignore`:

```bash
cp .env.example .env      # troque os valores, se quiser
docker compose up --build
```

Sobe o PostgreSQL e a API. A API aplica as migrations ao iniciar. Sem o `.env`, o compose para e diz qual variável está faltando.

- API: http://localhost:8080
- Para parar e apagar os dados: `docker compose down -v`

**Sem Docker para a API** (só o banco no Docker). As configurações vão para o user-secrets do .NET, fora da pasta do projeto. Use os mesmos valores do seu `.env`:

```bash
docker compose up -d db
dotnet user-secrets --project src/Oficina.Api set "ConnectionStrings:Oficina" "Host=localhost;Port=5432;Database=oficina;Username=oficina;Password=SUA_SENHA_DO_POSTGRES"
dotnet user-secrets --project src/Oficina.Api set "Jwt:Chave" "SUA_CHAVE_COM_32_CARACTERES_OU_MAIS"
dotnet user-secrets --project src/Oficina.Api set "Admin:Senha" "SUA_SENHA_DO_ADMIN"
dotnet run --project src/Oficina.Api
```

## Swagger

Abra **http://localhost:8080/swagger** (ou a porta mostrada pelo `dotnet run`). O documento OpenAPI fica em `/openapi/v1.json`, e uma cópia exportada está em [`docs/api/openapi.json`](docs/api/openapi.json) (abre em https://editor.swagger.io sem rodar o projeto).

## Autenticação

O usuário administrativo é `admin`. A senha é a que você definiu em `ADMIN_SENHA` no `.env` (no modelo `.env.example`, `troque-esta-senha`).

1. `POST /api/auth/login` com `{"usuario": "admin", "senha": "<ADMIN_SENHA>"}`.
2. Copie o `token` da resposta. Ele vale por 8 horas.
3. No Swagger, clique em **Authorize** e cole só o token (sem a palavra `Bearer`).

As rotas `/api/acompanhamento/*` são públicas (usadas pelo cliente, que informa o número da OS e o CPF/CNPJ). Todas as outras exigem o token.

A chave de assinatura do JWT vem de `JWT_CHAVE` no `.env` (mínimo de 32 caracteres). Sem a chave ou sem a senha, a API não sobe.

> Se você já tinha subido o projeto antes desta mudança, rode `docker compose down -v` uma vez. O volume antigo do PostgreSQL guarda a senha anterior.

### Roteiro rápido pelo Swagger

1. `POST /api/servicos`: `{"nome": "Troca de óleo", "preco": 80, "tempoEstimadoMinutos": 30}`
2. `POST /api/pecas-insumos`: `{"codigo": "FLT-001", "nome": "Filtro de óleo", "unidade": "un", "preco": 45}`
3. `POST /api/pecas-insumos/{id}/entrada`: `{"quantidade": 5}`
4. `POST /api/ordens-servico` com o cliente (CPF de teste válido: `529.982.247-25`) e o veículo → **Recebida**
5. `POST /api/ordens-servico/{id}/iniciar-diagnostico` → **EmDiagnostico**
6. `PUT /api/ordens-servico/{id}/diagnostico`, `POST .../servicos` e `POST .../pecas-insumos`
7. `POST /api/ordens-servico/{id}/enviar-orcamento` → **AguardandoAprovacao**, com o total calculado
8. `POST /api/acompanhamento/{id}/aprovar?documento=52998224725` (sem token) → **EmExecucao**
9. `POST /api/ordens-servico/{id}/finalizar` → **Finalizada** (a peça sai do estoque)
10. `POST /api/ordens-servico/{id}/entregar` → **Entregue**
11. `GET /api/ordens-servico/tempo-medio-execucao` e `GET /api/acompanhamento/{id}?documento=52998224725`

## Testes

```bash
dotnet test
```

- `tests/Oficina.UnitTests`: domínio e camada de serviço (com banco em memória do EF).
- `tests/Oficina.IntegrationTests`: a API inteira em memória (`WebApplicationFactory`), incluindo login, fluxo completo da OS e respostas 401/400.


### Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html;TextSummary" "-assemblyfilters:+Oficina.Domain;+Oficina.Application"
```

Abra `coverage-report/index.html`. O filtro mantém só o domínio e a aplicação, que são os domínios críticos.

Resultado atual ([`coverage-report/Summary.txt`](coverage-report/Summary.txt)):
Atalho para imagem caso nessessario([`coverage-report/CoverageImage.png`](coverage-report/CoverageImage.png)):


| Métrica | Cobertura |
|---|---|
| Linhas | 93,9% |
| Branches | 81,8% |
| Métodos | 96,4% |

## Análise de vulnerabilidades

Relatórios em [`docs/seguranca/`](docs/seguranca):

- [`pacotes-nuget.txt`](docs/seguranca/pacotes-nuget.txt): `dotnet list package --vulnerable --include-transitive`, nenhum pacote vulnerável.
comandos usados: 
mkdir -p docs/seguranca
dotnet list package --vulnerable --include-transitive > docs/seguranca/pacotes-nuget.txt
- [`imagem-docker.txt`](docs/seguranca/imagem-docker.txt): Trivy na imagem da API (HIGH/CRITICAL), 0 vulnerabilidades.
comandos usados: 
docker run --rm -v //var/run/docker.sock:/var/run/docker.sock aquasec/trivy image --severity HIGH,CRITICAL oficina-api > docs/seguranca/imagem-docker.txt
- [`Vulnerabilidades.png`](docs/seguranca/Vulnerabilidades.png): painel do SonarQube.
imagem com os 2
Para repetir:

```bash
dotnet list package --vulnerable --include-transitive > docs/seguranca/pacotes-nuget.txt
docker run --rm -v //var/run/docker.sock:/var/run/docker.sock aquasec/trivy image --severity HIGH,CRITICAL projetofiap_oficina-api > docs/seguranca/imagem-docker.txt
```

## Estrutura

```
├── src/
│   ├── Oficina.Api/              # Controllers, JWT, Swagger, Program.cs
│   ├── Oficina.Application/      # Serviços (casos de uso) e abstrações
│   ├── Oficina.Domain/           # Entidades, objetos de valor e regras
│   └── Oficina.Infrastructure/   # DbContext, repositório, migrations
├── tests/
│   ├── Oficina.UnitTests/
│   └── Oficina.IntegrationTests/
├── docs/
│   ├── api/                      # OpenAPI exportado
│   ├── arquitetura/              # C4 (Structurizr + imagem)
│   ├── ddd/                      # Event Storming, modelo de domínio, linguagem ubíqua
│   ├── entrega/                  # Identificação da entrega
│   └── seguranca/                # Relatórios de vulnerabilidades
├── coverage-report/              # Relatório de cobertura
├── .env.example                  # Modelo das variáveis (senhas e chave do JWT)
├── Dockerfile
└── docker-compose.yml
```

## Documentação DDD

Quadro no Miro: https://miro.com/app/board/uXjVEXtrgrQ=/?share_link_id=578425858571

Tudo está em [`docs/ddd/`](docs/ddd/README.md):

- [Event Storming](docs/ddd/event-storming.md): criação da OS, acompanhamento da OS e peças/insumos, com atores, comandos, eventos e políticas. Exportação do Miro em [`event-storming.png`](docs/ddd/event-storming.png), e a versão comparada com o código em [`event-storming-mvp.png`](docs/ddd/event-storming-mvp.png) (vermelho: fora do MVP; laranja: simplificado).
- [Modelo de domínio](docs/ddd/modelo-de-dominio.md): bounded contexts, agregados, entidades, objetos de valor, a relação com o código e as decisões do MVP.
- [Linguagem ubíqua](docs/ddd/linguagem-ubiqua.md): os termos do MVP e o nome de cada um no código.
- Domain Storytelling: [`domain-storytelling.png`](docs/ddd/domain-storytelling.png) e a versão comparada com o código em [`domain-storytelling-mvp.png`](docs/ddd/domain-storytelling-mvp.png)
- Glossário completo: [`glossario.pdf`](docs/ddd/glossario.pdf)
- Diagramas C4: [`c4.png`](docs/arquitetura/c4.png) e [`workspace.dsl`](docs/arquitetura/workspace.dsl)

## Dicionário de linguagem ubíqua (completo)
# Atendimento

| Termo | Definição | Evitar |
| --- | --- | --- |
| Cliente | Quem autoriza o serviço e responde pelo pagamento. Pode ser pessoa física, empresa, frota ou seguradora. | “proprietário” como sinônimo |
| Proprietário | Dono legal do veículo, conforme o documento. | “cliente” |
| Condutor | Quem leva ou retira o veículo na oficina. | “cliente” |
| Veículo | O bem atendido. Identificado pelo chassi; a placa é um atributo que pode mudar. | “automóvel”, “carro” |
| Agendamento | Horário marcado para a entrada do veículo. | “reserva” (colide com reserva de peça) |
| Recepção | Chegada do veículo e abertura da OS. | “check-in”, “triagem” |
| Vistoria de entrada | Registro de quilometragem, combustível, avarias, objetos no veículo e fotos, feito na recepção. | “checklist” (o checklist é só o formulário) |
| Relato do cliente | Descrição do problema nas palavras do cliente, sem interpretação técnica. | “defeito”, “problema” (pressupõem a causa) |
| Consultor técnico | Quem atende o cliente, apresenta o orçamento e acompanha a OS. | “atendente”, “recepcionista” |
| Histórico do veículo | Todas as OS anteriores daquele veículo. | “histórico do cliente” (o veículo pode trocar de dono) |

# Diagnóstico e orçamento

| Termo | Definição | Evitar |
| --- | --- | --- |
| Diagnóstico | Investigação técnica que identifica a causa do relato do cliente. | — |
| Laudo técnico | Registro documentado do resultado do diagnóstico. | usar como sinônimo de diagnóstico |
| Orçamento | Proposta de serviços e peças com valores e prazo. Pode ser recusado. | “proposta”, “cotação”, “OS” |
| Item do orçamento | Uma linha do orçamento: um serviço ou uma peça. | — |
| Aprovação | Autorização do cliente para executar o orçamento. Pode ser total ou parcial. | — |
| Item recusado | Item que o cliente não autorizou. Fica registrado na OS. | “item excluído” |
| Orçamento complementar | Novos itens descobertos durante a execução, que exigem nova aprovação. | “adicional”, “extra” |
| Mão de obra | Valor cobrado pelo trabalho, separado das peças. | — |
| Hora técnica | Preço de uma hora de mão de obra. | — |
| Tempário | Tabela de referência com o tempo de cada serviço. | — |
| Tempo padrão | O tempo de um serviço específico segundo o tempário. | “tempário” |

# Execução

| Termo | Definição | Evitar |
| --- | --- | --- |
| Ordem de Serviço (OS) | Registro central do atendimento, da recepção à entrega. | “atendimento”, “pedido” |
| Serviço | Trabalho executado (troca de óleo, alinhamento, retífica). | — |
| Mecânico | Profissional que executa os serviços. | “reparador”, “técnico” |
| Mecânico responsável | O mecânico designado para a OS. | — |
| Chefe de oficina | Quem distribui o trabalho e confere a qualidade. | — |
| Box | Posto de trabalho onde o veículo fica. O elevador é um equipamento do box. | usar “elevador” como sinônimo de box |
| Tempo apontado | Tempo efetivamente gasto no serviço, comparado ao tempo padrão. | — |
| Serviço terceirizado | Parte do trabalho enviada a terceiros (retífica, funilaria). | — |
| Peça aplicada | Peça efetivamente instalada no veículo naquela OS. | “peça usada” |
| Peça substituída | A peça retirada do veículo, que costuma ser devolvida ou mostrada ao cliente. | “peça usada”, “sucata” |
| Insumo | Material de consumo geral (estopa, desengraxante, abraçadeiras), normalmente não itemizado por OS. | usar como sinônimo de peça |
| Controle de qualidade | Conferência final antes de liberar o veículo. | — |
| Teste de rodagem | Volta com o veículo para validar o serviço. É uma etapa do controle de qualidade. | “test-drive” como sinônimo de controle de qualidade |
| Retorno | Veículo que volta com o mesmo problema após a entrega. | “reclamação” |

# Estoque de peças

| Termo | Definição | Evitar |
| --- | --- | --- |
| Peça | O produto do catálogo: código, descrição, marca e aplicação. | confundir com a quantidade em estoque |
| Aplicação | Veículos para os quais a peça serve. | “compatibilidade” |
| Genuína / Original / Paralela | Genuína: embalagem da montadora. Original: mesmo fabricante que fornece à montadora, com marca própria. Paralela: outro fabricante. | usar “original” para tudo que não é paralelo |
| Kit | Conjunto vendido como unidade (kit embreagem, kit correia). | — |
| Saldo físico | Quantidade que está fisicamente na prateleira. | “estoque” sem qualificação |
| Saldo reservado | Parte do saldo físico vinculada a OS aprovadas. | — |
| Saldo disponível | Saldo físico menos saldo reservado. É o que vale para orçar e vender. | — |
| Reserva de peça | Vínculo de uma quantidade a uma OS aprovada. | — |
| Liberação de reserva | Desfazer uma reserva que não será usada. | “cancelamento de reserva” |
| Requisição de peça | Pedido do mecânico ao estoque para uma OS. | usar para o ato de retirar |
| Retirada | Entrega física da peça ao mecânico. | — |
| Baixa de estoque | Registro da saída da peça do saldo físico. | — |
| Devolução ao estoque | Peça requisitada e não aplicada que volta à prateleira. | — |
| Ponto de pedido | Saldo mínimo que dispara a reposição. | “ponto de ressuprimento” (escolher um só) |
| Cotação | Consulta de preço a fornecedores. | “orçamento” |
| Pedido de compra | Compra efetivada com um fornecedor. | “solicitação de compra”, se forem a mesma coisa |
| Entrada de nota | Recebimento da mercadoria e lançamento da nota do fornecedor no estoque. | — |
| Venda de balcão | Venda de peça sem OS, direto ao cliente. | — |

# Faturamento e pós-serviço

| Termo | Definição | Evitar |
| --- | --- | --- |
| Fechamento da OS | Consolidação dos serviços executados e das peças aplicadas. | — |
| Faturamento | Emissão das notas e geração do valor a receber. | “pagamento” |
| Pagamento | Quitação do valor, que pode ser parcial ou parcelada. | “faturamento” |
| NFS-e / NF-e | Nota de serviço (mão de obra) e nota de produto (peças). A separação depende do município. | — |
| Entrega | Devolução do veículo ao cliente, com explicação do que foi feito. | “liberação” |
| Garantia | Prazo ou quilometragem de cobertura de um serviço ou peça. | — |
