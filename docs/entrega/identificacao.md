# Tech Challenge — Identificação da entrega

| | |
|---|---|
| **Curso** | SOAT — Software Architecture |
| **Fase** | 1 |
| **Grupo** | 95 |

## Integrantes

| Nome completo | RM | Discord |
|---|---|---|
| Gabriel Mauricio Zanette Gaidzinksi | RM378808 | zgthemaverick |

## Linguagem e principais tecnologias

- **Linguagem:** C# (.NET 10)
- **API:** ASP.NET Core Web API (REST), autenticação JWT Bearer
- **Documentação da API:** OpenAPI + Swagger UI
- **Persistência:** Entity Framework Core 10 + PostgreSQL 17 (Npgsql)
- **Testes:** xUnit, EF Core InMemory, WebApplicationFactory, Coverlet + ReportGenerator
- **Infraestrutura:** Docker e Docker Compose
- **Qualidade e segurança:** SonarQube, Trivy, `dotnet list package --vulnerable`
- **Arquitetura:** monólito em camadas com DDD; diagramas C4 no Structurizr

---

# 2. Repositório do projeto

| | |
|---|---|
| **Repositório (público)** | [https://github.com/ZgMaverick/ProjetoFiap_Oficina](https://github.com/ZgMaverick/ProjetoFiap_Oficina) |
| **Branch da entrega** | `dev` |
| **Commit da entrega** | [`dba51d01eab91436f87ed1577e5c832c2208e7e7`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/commit/dba51d01eab91436f87ed1577e5c832c2208e7e7) |

A avaliação deve considerar exclusivamente a versão acima.

## Onde encontrar cada item no repositório

| Requisito | Onde está |
|---|---|
| README completo | [`README.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/README.md) |
| Instruções de execução local | README, seções "Pré-requisitos" e "Como executar" (`docker compose up --build`) |
| Testes automatizados (mín. 80%) | `tests/` e [`coverage-report/`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/tree/dev/coverage-report): branches 81,8%, linhas 93,9% (domínio e aplicação) |
| Documentação da API | Ver "Documentação da API" abaixo |
| Análise de vulnerabilidades | Ver "Análise de vulnerabilidades" abaixo |
| Credenciais | Nenhuma no repositório: senhas e chave do JWT vêm do arquivo `.env` (fora do Git), a partir do modelo [`.env.example`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/.env.example) |

## Documentação da API (Swagger / OpenAPI)

- **Swagger UI:** com a aplicação rodando (`docker compose up --build`), em `http://localhost:8080/swagger`. O botão **Authorize** aceita o token JWT obtido em `POST /api/auth/login`.
- **Documento OpenAPI 3.1 (ao vivo):** `http://localhost:8080/openapi/v1.json`.
- **Cópia exportada no repositório:** [`docs/api/openapi.json`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/api/openapi.json). Pode ser aberta em https://editor.swagger.io sem rodar o projeto.

Grupos de endpoints documentados: `Auth`, `Clientes`, `Veiculos`, `Servicos`, `PecasInsumos`, `OrdensServico` (administrativos, exigem JWT) e `Acompanhamento` (público, usado pelo cliente).

## Análise de vulnerabilidades

Relatórios em [`docs/seguranca/`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/tree/dev/docs/seguranca):

| Análise | Ferramenta | Resultado |
|---|---|---|
| Pacotes NuGet (incluindo transitivos) | `dotnet list package --vulnerable --include-transitive` → [`pacotes-nuget.txt`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/seguranca/pacotes-nuget.txt) | Nenhum pacote vulnerável |
| Imagem Docker da API (HIGH/CRITICAL) | Trivy → [`imagem-docker.txt`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/seguranca/imagem-docker.txt) | 0 vulnerabilidades (SO Ubuntu 24.04 e runtime .NET 10.0.14) |
| Análise estática do código | SonarQube → [`Vulnerabilidades.png`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/seguranca/Vulnerabilidades.png) | Painel com Quality Gate, bugs, vulnerabilidades e cobertura |

---

# 3. Vídeo da entrega

**YouTube (público):** _link do vídeo_

# 4. Documentação DDD

- **Miro (acesso sem pedir permissão):** [https://miro.com/app/board/uXjVEXtrgrQ=/?share_link_id=578425858571](https://miro.com/app/board/uXjVEXtrgrQ=/?share_link_id=578425858571)
- **No repositório:** [`docs/ddd/`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/tree/dev/docs/ddd)

| Conteúdo pedido | Onde está |
|---|---|
| Event Storming da criação da OS, do acompanhamento da OS e da gestão de peças e insumos | [`event-storming.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/event-storming.md), seções 1 a 3, e [`event-storming.png`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/event-storming.png) |
| Eventos de domínio, comandos, políticas e atores | [`event-storming.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/event-storming.md), seção "Resumo" |
| Agregados, entidades, objetos de valor e bounded contexts | [`modelo-de-dominio.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/modelo-de-dominio.md) |
| Diagramas | Domain Storytelling ([`domain-storytelling.png`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/domain-storytelling.png)) e C4 ([`c4.png`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/arquitetura/c4.png)) |
| Linguagem ubíqua e glossário | [`linguagem-ubiqua.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/linguagem-ubiqua.md) e [`glossario.pdf`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/glossario.pdf) |
| Relação entre a documentação e o código | [`modelo-de-dominio.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/docs/ddd/modelo-de-dominio.md), seções "Relação entre documentação e código" e "Decisões do MVP" |
