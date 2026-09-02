import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

/**
 * Interceptor global de erros HTTP com diagnóstico explícito por microsserviço.
 */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notification = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const isEstoque = req.url.includes('/estoque');
        const nomeServico = isEstoque ? 'Serviço de Estoque' : 'Serviço de Faturamento';
        const porta = isEstoque ? '5001' : '5002';
        const comando = isEstoque 
          ? 'cd Core/Microsservico/ServicoEstoque && dotnet run' 
          : 'cd Core/Microsservico/ServicoFaturamento && dotnet run';

        // 1. Status 0: O serviço não está respondendo (offline / conexão recusada)
        if (error.status === 0) {
          notification.erro(
            `O ${nomeServico} (porta ${porta}) está inacessível. Inicie o serviço no terminal: "${comando}".`,
            'Conexão Recusada'
          );
        }
        // 2. Status 503: O serviço retornou indisponibilidade de dependência
        else if (error.status === 503) {
          const mensagem = error.error?.mensagem 
            ?? `O ${nomeServico} está temporariamente indisponível.`;
          notification.erro(mensagem, 'Serviço Indisponível (503)');
        }
        // 3. Status 500: Erro interno do servidor
        else if (error.status === 500) {
          notification.erro(
            `Ocorreu um erro interno no ${nomeServico}. Verifique os logs da aplicação.`,
            'Erro Interno (500)'
          );
        }
      }
      return throwError(() => error);
    })
  );
};
