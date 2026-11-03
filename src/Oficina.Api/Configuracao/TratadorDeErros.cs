using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oficina.Application.Abstracoes;
using Oficina.Domain.Comum;

namespace Oficina.Api.Configuracao;

public class TratadorDeErros(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception erro, CancellationToken ct)
    {
        var (status, titulo) = erro switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio não atendida"),
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Não encontrado"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Registro alterado por outra operação. Tente novamente."),
            _ => (0, "")
        };

        if (status == 0) return false; // erro inesperado: deixa virar 500

        http.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = { Status = status, Title = titulo, Detail = erro.Message }
        });
    }
}
