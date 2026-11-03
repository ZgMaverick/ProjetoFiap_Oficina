using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Oficina.IntegrationTests;

// Sobe a API inteira em memória. O ambiente "Testing" faz o Program.cs
// usar o banco em memória do Entity Framework no lugar do PostgreSQL.
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Testing");
}
