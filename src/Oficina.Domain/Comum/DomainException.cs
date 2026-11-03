namespace Oficina.Domain.Comum;

// Erro de regra de negócio. A API transforma esta exceção em resposta 400.
public class DomainException(string mensagem) : Exception(mensagem);
