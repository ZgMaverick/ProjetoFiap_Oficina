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
| **Commit da entrega** | _hash do commit final_ |

A avaliação deve considerar exclusivamente a versão acima.

## Onde encontrar cada item no repositório

| Requisito | Onde está |
|---|---|
| README completo | [`README.md`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/blob/dev/README.md) |
| Instruções de execução local | README, seções "Pré-requisitos" e "Como executar" (`docker compose up --build`) |
| Testes automatizados (mín. 80%) | `tests/` e [`coverage-report/`](https://github.com/ZgMaverick/ProjetoFiap_Oficina/tree/dev/coverage-report): branches 81,8%, linhas 93,9% (domínio e aplicação) |
| Documentação da API | Ver "Documentação da API" abaixo |
| Análise de vulnerabilidades | Ver "Análise de vulnerabilidades" abaixo |
| Credenciais | Só as de demonstração (`admin` / `admin123`), documentadas no README; em produção vêm de variáveis de ambiente |

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
