using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oficina.Api.Configuracao;
using Oficina.Application.Cadastros;
using Oficina.Application.OrdensDeServico;
using Oficina.Infrastructure;
using Oficina.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Repositórios e banco. No ambiente "Testing" (testes de integração) usa banco em memória.
var usarBancoEmMemoria = builder.Environment.IsEnvironment("Testing");
builder.Services.AddInfraestrutura(builder.Configuration.GetConnectionString("Oficina") ?? "", usarBancoEmMemoria);

// Camada de serviço
builder.Services.AddScoped<OrdemServicoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<ServicoCatalogoService>();
builder.Services.AddScoped<PecaInsumoService>();

// Enums aparecem como texto no JSON ("EmDiagnostico" em vez de 2)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Chave"]!))
    });
builder.Services.AddAuthorization();

// OpenAPI + botão "Authorize" no Swagger
builder.Services.AddOpenApi(o => o.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Erros de domínio viram 400, não encontrado vira 404
builder.Services.AddExceptionHandler<TratadorDeErros>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Cria/atualiza as tabelas ao subir (simples para quem for avaliar)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();      // PostgreSQL: aplica as migrations
    else
        db.Database.EnsureCreated(); // banco em memória: só cria o modelo
}

app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Oficina API"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Permite que os testes de integração usem WebApplicationFactory<Program>
public partial class Program { }
