# ProjetoFiap_Oficina

Projeto de Software Architecture, com intuito de montar um sistema de uma Oficina Mecânica.

MVP do Tech Challenge: API em .NET 10 para gestão de ordens de serviço, clientes, veículos, catálogo de serviços e estoque de peças e insumos.

## Tecnologias

- .NET 10 (ASP.NET Core com controllers)
- Entity Framework Core com PostgreSQL
- Autenticação JWT
- OpenAPI e Swagger UI
- xUnit, com banco em memória do Entity Framework nos testes
- Docker e Docker Compose

## Estrutura

```
src/
  Oficina.Domain          Entidades, objetos de valor e regras de negócio (sem dependências)
  Oficina.Application     Serviços de aplicação e interfaces dos repositórios
  Oficina.Infrastructure  Entity Framework, repositórios e migrations
  Oficina.Api             Controllers, JWT, Swagger e tratamento de erros
tests/
  Oficina.UnitTests         Testes do domínio e da camada de serviço
  Oficina.IntegrationTests  Testes da API em memória
```

## Como rodar

Só é preciso ter o Docker instalado:

```
docker compose up --build
```

O comando sobe o PostgreSQL e a API. A API aplica as migrations ao iniciar.

- Swagger: http://localhost:8080/swagger

Para parar e apagar os dados do banco:

```
docker compose down -v
```

### Rodar sem Docker para a API

Com o SDK do .NET 10 instalado, suba só o banco e rode a API localmente:

```
docker compose up -d db
dotnet run --project src/Oficina.Api
```

## Credenciais de demonstração

| O quê | Valor |
|---|---|
| Usuário da API | `admin` |
| Senha da API | `admin123` |
| Chave do JWT | `troque-esta-chave-por-uma-com-32-caracteres-ou-mais` |
| Banco (PostgreSQL) | banco `oficina`, usuário `oficina`, senha `oficina` |

Esses valores são apenas para demonstração. No `docker-compose.yml`, a chave do JWT pode ser trocada pela variável de ambiente `JWT_CHAVE`, que precisa ter pelo menos 32 caracteres. Em produção, as credenciais devem vir de variáveis de ambiente.

## Usando a API

1. `POST /api/auth/login` com `{"usuario": "admin", "senha": "admin123"}` e copie o `token`.
2. No Swagger, clique em **Authorize** e cole só o token, sem a palavra `Bearer`.
3. Cadastre serviços (`/api/servicos`) e peças (`/api/pecas-insumos`), e dê entrada no estoque (`/api/pecas-insumos/{id}/entrada`).
4. Abra uma OS em `POST /api/ordens-servico`. O cliente e o veículo são cadastrados automaticamente.
5. Siga o fluxo: iniciar diagnóstico, registrar diagnóstico, adicionar serviços e peças, enviar orçamento, aprovar, finalizar e entregar.

O cliente acompanha, aprova ou recusa a OS sem token, informando o número da OS e o CPF/CNPJ:

- `GET /api/acompanhamento/{id}?documento=...`
- `POST /api/acompanhamento/{id}/aprovar?documento=...`
- `POST /api/acompanhamento/{id}/recusar?documento=...`

### Status da ordem de serviço

Recebida → Em diagnóstico → Aguardando aprovação → Em execução → Finalizada → Entregue

A OS pode ser cancelada enquanto está Recebida, Em diagnóstico ou Aguardando aprovação. Recusar o orçamento também cancela a OS e devolve as peças reservadas.

## Testes

```
dotnet test
```

Para gerar o relatório de cobertura:

```
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html" "-assemblyfilters:+Oficina.Domain;+Oficina.Application"
```

O relatório fica em `coverage-report/index.html`.

## Segurança

Os relatórios de análise de vulnerabilidades ficam em `docs/seguranca/`.
