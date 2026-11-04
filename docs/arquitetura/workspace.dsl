workspace "Oficina Mecânica" "Modelo C4 (níveis 1 a 3) do sistema de gestão de Oficina Mecânica - FIAP Software Architecture." {

    !identifiers hierarchical

    model {
        # ------------------------------------------------------------------
        # Nível 1 - Pessoas
        # ------------------------------------------------------------------
        admin = person "Administrador / Atendente" "Funcionário da oficina. Faz login, cadastra clientes, veículos, serviços e peças/insumos e conduz as ordens de serviço."
        cliente = person "Cliente" "Dono do veículo. Acompanha o andamento da sua ordem de serviço e aprova ou recusa o orçamento."

        # ------------------------------------------------------------------
        # Nível 1/2 - Sistema e containers
        # ------------------------------------------------------------------
        oficina = softwareSystem "Sistema da Oficina Mecânica" "Gerencia ordens de serviço, orçamentos, cadastros e estoque de peças/insumos da oficina." {

            api = container "Oficina API" "API REST com Swagger UI. Expõe cadastros, fluxo da ordem de serviço, acompanhamento público e autenticação JWT." "ASP.NET Core 10 (C#) - Docker" {

                # --- Camada de apresentação (Oficina.Api) ---
                swagger = component "Swagger UI / OpenAPI" "Documentação interativa da API em /swagger, com botão Authorize para o token Bearer." "Swashbuckle.SwaggerUI + Microsoft.AspNetCore.OpenApi" "Api"
                authController = component "AuthController" "POST /api/auth/login - valida o usuário admin configurado e emite o token JWT." "ASP.NET Core Controller" "Api"
                jwtMiddleware = component "Autenticação JWT" "Valida emissor, audiência, validade e assinatura do token Bearer nas rotas [Authorize]." "JwtBearer Middleware" "Api"
                tratadorErros = component "TratadorDeErros" "Converte DomainException em 400 e NaoEncontradoException em 404 (ProblemDetails)." "IExceptionHandler" "Api"

                clientesController = component "ClientesController" "CRUD de clientes - /api/clientes." "ASP.NET Core Controller" "Api"
                veiculosController = component "VeiculosController" "CRUD de veículos - /api/veiculos." "ASP.NET Core Controller" "Api"
                servicosController = component "ServicosController" "CRUD do catálogo de serviços - /api/servicos." "ASP.NET Core Controller" "Api"
                pecasController = component "PecasInsumosController" "CRUD de peças/insumos e entrada de estoque - /api/pecas-insumos." "ASP.NET Core Controller" "Api"
                ordensController = component "OrdensServicoController" "Abertura, diagnóstico, inclusão de serviços/peças, orçamento, cancelamento, finalização, entrega e tempo médio - /api/ordens-servico." "ASP.NET Core Controller" "Api"
                acompanhamentoController = component "AcompanhamentoController" "Rotas públicas: consultar OS, aprovar e recusar orçamento - /api/acompanhamento." "ASP.NET Core Controller" "Api"

                # --- Camada de aplicação (Oficina.Application) ---
                clienteService = component "ClienteService" "Casos de uso de cadastro de clientes (CPF/CNPJ validados)." "C# Service" "Application"
                veiculoService = component "VeiculoService" "Casos de uso de cadastro de veículos vinculados a clientes." "C# Service" "Application"
                servicoCatalogoService = component "ServicoCatalogoService" "Casos de uso do catálogo de serviços." "C# Service" "Application"
                pecaInsumoService = component "PecaInsumoService" "Casos de uso de peças/insumos e controle de estoque." "C# Service" "Application"
                ordemServicoService = component "OrdemServicoService" "Orquestra o ciclo de vida da ordem de serviço, orçamento e baixa de estoque." "C# Service" "Application"

                # --- Camada de domínio (Oficina.Domain) ---
                dominio = component "Modelo de Domínio" "Entidades e regras: Cliente, Veiculo, Servico, PecaInsumo, OrdemDeServico (status + histórico) e Value Objects Documento e Placa." "C# Domain Model" "Domain"

                # --- Camada de infraestrutura (Oficina.Infrastructure) ---
                repositorio = component "Repositorio<T>" "Implementação genérica de IRepositorio<T> para leitura e escrita das entidades." "EF Core" "Infrastructure"
                dbContext = component "OficinaDbContext" "Mapeamento ORM, migrations e IUnidadeDeTrabalho (SaveChanges)." "EF Core DbContext + Npgsql" "Infrastructure"
            }

            database = container "Banco de Dados" "Armazena clientes, veículos, serviços, peças/insumos, ordens de serviço e histórico de status." "PostgreSQL 17 - Docker" "Database"
        }

        # ------------------------------------------------------------------
        # Relacionamentos - nível de sistema/container
        # ------------------------------------------------------------------
        admin -> oficina.api "Gerencia cadastros e ordens de serviço" "HTTPS/JSON + JWT"
        cliente -> oficina.api "Acompanha a OS e aprova/recusa o orçamento" "HTTPS/JSON"
        oficina.api -> oficina.database "Lê e grava dados" "TCP 5432 / Npgsql"

        # ------------------------------------------------------------------
        # Relacionamentos - nível de componente
        # ------------------------------------------------------------------
        admin -> oficina.api.swagger "Explora e testa a API"
        admin -> oficina.api.authController "Faz login" "POST /api/auth/login"
        admin -> oficina.api.clientesController "Usa" "HTTPS/JSON + JWT"
        admin -> oficina.api.veiculosController "Usa" "HTTPS/JSON + JWT"
        admin -> oficina.api.servicosController "Usa" "HTTPS/JSON + JWT"
        admin -> oficina.api.pecasController "Usa" "HTTPS/JSON + JWT"
        admin -> oficina.api.ordensController "Usa" "HTTPS/JSON + JWT"
        cliente -> oficina.api.acompanhamentoController "Consulta, aprova ou recusa" "HTTPS/JSON (anônimo)"

        oficina.api.jwtMiddleware -> oficina.api.clientesController "Protege"
        oficina.api.jwtMiddleware -> oficina.api.veiculosController "Protege"
        oficina.api.jwtMiddleware -> oficina.api.servicosController "Protege"
        oficina.api.jwtMiddleware -> oficina.api.pecasController "Protege"
        oficina.api.jwtMiddleware -> oficina.api.ordensController "Protege"

        oficina.api.clientesController -> oficina.api.clienteService "Chama"
        oficina.api.veiculosController -> oficina.api.veiculoService "Chama"
        oficina.api.servicosController -> oficina.api.servicoCatalogoService "Chama"
        oficina.api.pecasController -> oficina.api.pecaInsumoService "Chama"
        oficina.api.ordensController -> oficina.api.ordemServicoService "Chama"
        oficina.api.acompanhamentoController -> oficina.api.ordemServicoService "Chama"

        oficina.api.clienteService -> oficina.api.dominio "Usa"
        oficina.api.veiculoService -> oficina.api.dominio "Usa"
        oficina.api.servicoCatalogoService -> oficina.api.dominio "Usa"
        oficina.api.pecaInsumoService -> oficina.api.dominio "Usa"
        oficina.api.ordemServicoService -> oficina.api.dominio "Usa"

        oficina.api.clienteService -> oficina.api.repositorio "Usa" "IRepositorio<T>"
        oficina.api.veiculoService -> oficina.api.repositorio "Usa" "IRepositorio<T>"
        oficina.api.servicoCatalogoService -> oficina.api.repositorio "Usa" "IRepositorio<T>"
        oficina.api.pecaInsumoService -> oficina.api.repositorio "Usa" "IRepositorio<T>"
        oficina.api.ordemServicoService -> oficina.api.repositorio "Usa" "IRepositorio<T>"

        oficina.api.clienteService -> oficina.api.dbContext "Confirma transação" "IUnidadeDeTrabalho"
        oficina.api.veiculoService -> oficina.api.dbContext "Confirma transação" "IUnidadeDeTrabalho"
        oficina.api.servicoCatalogoService -> oficina.api.dbContext "Confirma transação" "IUnidadeDeTrabalho"
        oficina.api.pecaInsumoService -> oficina.api.dbContext "Confirma transação" "IUnidadeDeTrabalho"
        oficina.api.ordemServicoService -> oficina.api.dbContext "Confirma transação" "IUnidadeDeTrabalho"

        oficina.api.dominio -> oficina.api.tratadorErros "Lança DomainException, tratada como HTTP 400"
        oficina.api.repositorio -> oficina.api.dbContext "Usa"
        oficina.api.dbContext -> oficina.database "Lê e grava" "SQL / Npgsql"
    }

    views {
        systemContext oficina "N1_Contexto" "Nível 1 - Diagrama de Contexto" {
            include *
            autoLayout lr
        }

        container oficina "N2_Containers" "Nível 2 - Diagrama de Containers" {
            include *
            autoLayout lr
        }

        component oficina.api "N3_Componentes_API" "Nível 3 - Componentes da Oficina API" {
            include *
            autoLayout lr
        }

        styles {
            element "Element" {
                color #ffffff
            }
            element "Person" {
                background #08427b
                shape Person
            }
            element "Software System" {
                background #1168bd
            }
            element "Container" {
                background #438dd5
            }
            element "Database" {
                shape Cylinder
                background #2f6f9f
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            element "Api" {
                background #85bbf0
            }
            element "Application" {
                background #a7d08c
            }
            element "Domain" {
                background #f5d77a
                shape Hexagon
            }
            element "Infrastructure" {
                background #d9a6e0
            }
        }
    }

}
