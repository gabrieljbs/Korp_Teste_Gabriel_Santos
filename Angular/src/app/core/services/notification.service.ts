import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

export type NotificationSeverity = 'success' | 'info' | 'warn' | 'error';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messageService = inject(MessageService);

  sucesso(mensagem: string, titulo = 'Sucesso'): void {
    this.messageService.add({
      severity: 'success',
      summary: titulo,
      detail: mensagem,
      life: 4000
    });
  }

  erro(mensagem: string, titulo = 'Erro'): void {
    this.messageService.add({
      severity: 'error',
      summary: titulo,
      detail: mensagem,
      life: 6000
    });
  }

  aviso(mensagem: string, titulo = 'Atenção'): void {
    this.messageService.add({
      severity: 'warn',
      summary: titulo,
      detail: mensagem,
      life: 5000
    });
  }

  servicoIndisponivel(servico: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Serviço Indisponível',
      detail: `O ${servico} está temporariamente indisponível. Tente novamente em alguns instantes.`,
      life: 8000
    });
  }
}
