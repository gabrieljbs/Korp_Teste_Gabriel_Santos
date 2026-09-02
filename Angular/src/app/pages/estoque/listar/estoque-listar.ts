import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { EstoqueService } from '../../../services/estoque.service';
import { Produto } from '../../../models/produto.model';
import { RouterLink } from '@angular/router';
import { TableModule, SortIcon } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-estoque-listar',
  imports: [RouterLink, DecimalPipe, TableModule, SortIcon, ButtonModule, CardModule, MessageModule, TagModule, SharedModule],
  templateUrl: './estoque-listar.html'
})
export class EstoqueListarComponent implements OnInit, OnDestroy {
  private readonly estoqueService = inject(EstoqueService);
  private readonly notification = inject(NotificationService);
  private readonly destroy$ = new Subject<void>();

  produtos = signal<Produto[]>([]);
  carregando = signal(true);
  erroConexao = signal(false);

  ngOnInit(): void {
    this.carregarProdutos();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarProdutos(): void {
    this.carregando.set(true);
    this.erroConexao.set(false);

    this.estoqueService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dados: Produto[]) => {
          this.produtos.set(dados);
          this.carregando.set(false);
        },
        error: () => {
          this.erroConexao.set(true);
          this.carregando.set(false);
        }
      });
  }

  getSaldoSeverity(saldo: number): 'success' | 'warn' | 'danger' {
    if (saldo <= 0) return 'danger';
    if (saldo <= 5) return 'warn';
    return 'success';
  }

  getSaldoLabel(saldo: number): string {
    if (saldo <= 0) return 'Esgotado';
    if (saldo <= 5) return 'Baixo';
    return 'Disponível';
  }
}
