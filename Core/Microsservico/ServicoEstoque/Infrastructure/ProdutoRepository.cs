using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Application;
using ServicoEstoque.Domain;

namespace ServicoEstoque.Infrastructure;

public sealed class ProdutoRepository : IProdutoRepository
{
    private readonly EstoqueDbContext contexto;

    public ProdutoRepository(EstoqueDbContext contexto)
    {
        this.contexto = contexto;
    }

    public async Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await contexto.Produtos
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Produto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await contexto.Produtos
            .OrderBy(produto => produto.Descricao)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        Produto produto,
        CancellationToken cancellationToken = default)
    {
        await contexto.Produtos.AddAsync(produto, cancellationToken);
    }

    public async Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        await contexto.SaveChangesAsync(cancellationToken);
    }
}