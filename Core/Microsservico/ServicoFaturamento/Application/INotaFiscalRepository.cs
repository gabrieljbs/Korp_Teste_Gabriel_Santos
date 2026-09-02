using ServicoFaturamento.Domain;

namespace ServicoFaturamento.Application;

public interface INotaFiscalRepository
{
    Task<NotaFiscal?> ObterPorNumeroAsync(long numero, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<NotaFiscal>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(NotaFiscal nota, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
