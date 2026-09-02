using Microsoft.EntityFrameworkCore;
using ServicoFaturamento.Application;
using ServicoFaturamento.Domain;

namespace ServicoFaturamento.Infrastructure;

public sealed class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly FaturamentoDbContext _contexto;

    public NotaFiscalRepository(FaturamentoDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<NotaFiscal?> ObterPorNumeroAsync(
        long numero,
        CancellationToken cancellationToken = default)
    {
        return await _contexto.NotasFiscais
            .Include(nf => nf.Itens)
            .FirstOrDefaultAsync(nf => nf.Numero == numero, cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotaFiscal>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _contexto.NotasFiscais
            .Include(nf => nf.Itens)
            .OrderByDescending(nf => nf.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        NotaFiscal nota,
        CancellationToken cancellationToken = default)
    {
        await _contexto.NotasFiscais.AddAsync(nota, cancellationToken);
    }

    public async Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
