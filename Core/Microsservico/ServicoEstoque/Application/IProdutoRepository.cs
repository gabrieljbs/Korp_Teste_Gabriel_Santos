using ServicoEstoque.Domain;

namespace ServicoEstoque.Application;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}