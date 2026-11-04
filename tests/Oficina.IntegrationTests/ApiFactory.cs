using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Oficina.IntegrationTests;

// Sobe a API inteira em memória. O ambiente "Testing" faz o Program.cs
// usar o banco em memória do Entity Framework no lugar do PostgreSQL.
// A chave do JWT e a senha do admin são valores só de teste, já que o
// appsettings.json não traz credenciais.
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string SenhaAdmin = "senha-de-teste";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder
            .UseEnvironment("Testing")
            .UseSetting("Jwt:Chave", "chave-de-teste-com-pelo-menos-32-caracteres")
            .UseSetting("Admin:Senha", SenhaAdmin);
}
