using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Oficina.Api.Controllers;
using Oficina.Application.Cadastros;
using Oficina.Application.OrdensDeServico;

namespace Oficina.IntegrationTests;

public class FluxoOrdemServicoTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Cpf = "52998224725";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var http = factory.CreateClient();
        var login = await http.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", ApiFactory.SenhaAdmin));
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>(Json))!.Token;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    private static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
    {
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Endpoint_administrativo_sem_token_retorna_401()
    {
        var http = factory.CreateClient();
        var resposta = await http.GetAsync("/api/ordens-servico");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Fluxo_completo_da_os_ate_a_entrega()
    {
        var http = await ClienteAutenticadoAsync();

        // Cadastros
        var servico = await LerAsync<ServicoResponse>(await http.PostAsJsonAsync("/api/servicos",
            new ServicoRequest("Troca de óleo", 80m, 30)));
        var peca = await LerAsync<PecaInsumoResponse>(await http.PostAsJsonAsync("/api/pecas-insumos",
            new PecaInsumoRequest("FLT-001", "Filtro de óleo", "un", 45m)));
        await LerAsync<PecaInsumoResponse>(await http.PostAsJsonAsync($"/api/pecas-insumos/{peca.Id}/entrada",
            new EntradaEstoqueRequest(5)));

        // Abertura da OS
        var os = await LerAsync<OrdemServicoResponse>(await http.PostAsJsonAsync("/api/ordens-servico",
            new CriarOrdemServicoRequest(Cpf, "Ana Souza", null, null, "ABC1D23", "Fiat", "Uno", 2015, "Barulho no motor")));
        Assert.Equal("Recebida", os.Status);

        // Diagnóstico e orçamento
        await LerAsync<OrdemServicoResponse>(await http.PostAsync($"/api/ordens-servico/{os.Id}/iniciar-diagnostico", null));
        await LerAsync<OrdemServicoResponse>(await http.PutAsJsonAsync($"/api/ordens-servico/{os.Id}/diagnostico",
            new DiagnosticoRequest("Óleo vencido")));
        await LerAsync<OrdemServicoResponse>(await http.PostAsJsonAsync($"/api/ordens-servico/{os.Id}/servicos",
            new AdicionarServicoRequest(servico.Id)));
        await LerAsync<OrdemServicoResponse>(await http.PostAsJsonAsync($"/api/ordens-servico/{os.Id}/pecas-insumos",
            new AdicionarPecaRequest(peca.Id, 1)));
        os = await LerAsync<OrdemServicoResponse>(await http.PostAsync($"/api/ordens-servico/{os.Id}/enviar-orcamento", null));
        Assert.Equal("AguardandoAprovacao", os.Status);
        Assert.Equal(125m, os.ValorOrcamento);

        // Cliente aprova pela API pública, sem token
        var publico = factory.CreateClient();
        os = await LerAsync<OrdemServicoResponse>(await publico.PostAsync($"/api/acompanhamento/{os.Id}/aprovar?documento={Cpf}", null));
        Assert.Equal("EmExecucao", os.Status);

        // Execução e entrega
        await LerAsync<OrdemServicoResponse>(await http.PostAsync($"/api/ordens-servico/{os.Id}/finalizar", null));
        os = await LerAsync<OrdemServicoResponse>(await http.PostAsync($"/api/ordens-servico/{os.Id}/entregar", null));
        Assert.Equal("Entregue", os.Status);

        // Estoque: 5 entraram, 1 saiu
        var pecaFinal = await LerAsync<PecaInsumoResponse>(await http.GetAsync($"/api/pecas-insumos/{peca.Id}"));
        Assert.Equal(4m, pecaFinal.SaldoFisico);
        Assert.Equal(0m, pecaFinal.SaldoReservado);
    }

    [Fact]
    public async Task Cpf_invalido_retorna_400()
    {
        var http = await ClienteAutenticadoAsync();
        var resposta = await http.PostAsJsonAsync("/api/ordens-servico",
            new CriarOrdemServicoRequest("123.456.789-00", "X", null, null, "ABC1234", "VW", "Gol", 2010, "Teste"));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cliente_nao_consulta_os_de_outro_cliente()
    {
        var http = await ClienteAutenticadoAsync();
        var os = await LerAsync<OrdemServicoResponse>(await http.PostAsJsonAsync("/api/ordens-servico",
            new CriarOrdemServicoRequest("11144477735", "Bruno", null, null, "XYZ9876", "Ford", "Ka", 2018, "Freio")));

        var resposta = await factory.CreateClient().GetAsync($"/api/acompanhamento/{os.Id}?documento={Cpf}");
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
