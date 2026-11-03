namespace Oficina.Application.Abstracoes;

// A API transforma esta exceção em resposta 404.
public class NaoEncontradoException(string mensagem) : Exception(mensagem);
