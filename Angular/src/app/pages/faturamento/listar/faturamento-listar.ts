import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FaturamentoService } from '../../../services/faturamento.service';
import { NotaFiscal } from '../../../models/nota-fiscal.model';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { MessageModule } from 'primeng/message';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-faturamento-listar',
  imports: [RouterLink, CommonModule, TableModule, ButtonModule, CardModule, TagModule, MessageModule],
  templateUrl: './faturamento-listar.html'
})
export class FaturamentoListarComponent implements OnInit, OnDestroy {
  private readonly faturamentoService = inject(FaturamentoService);
  private readonly destroy$ = new Subject<void>();

  notas = signal<NotaFiscal[]>([]);
  carregando = signal(true);
  erroConexao = signal(false);

  ngOnInit(): void {
    this.carregarNotas();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarNotas(): void {
    this.carregando.set(true);
    this.erroConexao.set(false);

    this.faturamentoService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dados: NotaFiscal[]) => {
          this.notas.set(dados);
          this.carregando.set(false);
        },
        error: () => {
          this.erroConexao.set(true);
          this.carregando.set(false);
        }
      });
  }

  getStatusSeverity(status: string): 'info' | 'success' {
    return status === 'Aberta' ? 'info' : 'success';
  }
}
