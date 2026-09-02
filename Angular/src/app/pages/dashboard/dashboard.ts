import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { EstoqueService } from '../../services/estoque.service';
import { FaturamentoService } from '../../services/faturamento.service';
import { RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, finalize, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, CardModule, ButtonModule, SkeletonModule],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly estoqueService = inject(EstoqueService);
  private readonly faturamentoService = inject(FaturamentoService);
  private readonly destroy$ = new Subject<void>();

  totalProdutos = signal<number | null>(null);
  notasAbertas = signal<number | null>(null);
  notasFechadas = signal<number | null>(null);

  carregando = signal(true);
  erroEstoque = signal(false);
  erroFaturamento = signal(false);

  temErro = computed(() => this.erroEstoque() || this.erroFaturamento());

  ngOnInit(): void {
    this.carregarDados();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarDados(): void {
    this.carregando.set(true);
    this.erroEstoque.set(false);
    this.erroFaturamento.set(false);

    forkJoin({
      produtos: this.estoqueService.listar().pipe(
        catchError(() => {
          this.erroEstoque.set(true);
          return of(null);
        })
      ),
      notas: this.faturamentoService.listar().pipe(
        catchError(() => {
          this.erroFaturamento.set(true);
          return of(null);
        })
      )
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.carregando.set(false))
      )
      .subscribe(({ produtos, notas }) => {
        if (produtos !== null) {
          this.totalProdutos.set(produtos.length);
        } else {
          this.totalProdutos.set(null);
        }

        if (notas !== null) {
          this.notasAbertas.set(notas.filter(n => n.status === 'Aberta').length);
          this.notasFechadas.set(notas.filter(n => n.status === 'Fechada').length);
        } else {
          this.notasAbertas.set(null);
          this.notasFechadas.set(null);
        }
      });
  }
}
