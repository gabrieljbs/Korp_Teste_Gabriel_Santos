import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { FaturamentoService } from '../../../services/faturamento.service';
import { EstoqueService } from '../../../services/estoque.service';
import { NotaFiscal, AdicionarItemRequest } from '../../../models/nota-fiscal.model';
import { Produto } from '../../../models/produto.model';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { InputNumber } from 'primeng/inputnumber';
import { DecimalPipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ConfirmationService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil, timeout } from 'rxjs/operators';
import { TimeoutError } from 'rxjs';
import { NotificationService } from '../../../core/services/notification.service';
import { FormErrorPipe } from '../../../core/pipes/form-error.pipe';

import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-faturamento-detalhes',
  imports: [
    ReactiveFormsModule, RouterLink, CommonModule, DecimalPipe,
    TableModule, ButtonModule, TagModule, InputNumber, CardModule, MessageModule, FormErrorPipe, TooltipModule
  ],
  templateUrl: './faturamento-detalhes.html'
})
export class FaturamentoDetalhesComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly faturamentoService = inject(FaturamentoService);
  private readonly estoqueService = inject(EstoqueService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly notification = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();

  nota = signal<NotaFiscal | null>(null);
  produtosDisponiveis = signal<Produto[]>([]);
  numero!: number;

  carregando = signal(true);
  erroConexao = signal(false);
  adicionando = signal(false);
  fechando = signal(false);
  erroAcao = signal('');

  isInvalid(campo: string): boolean {
    const ctrl = this.formItem.get(campo);
    return !!(ctrl?.invalid && (ctrl.dirty || ctrl.touched));
  }

  formItem = this.fb.nonNullable.group({
    codigoProduto: ['', Validators.required],
    quantidade: [1 as number, [Validators.required, Validators.min(0.001)]]
  });

  codigoSelecionado = toSignal(this.formItem.controls.codigoProduto.valueChanges, { initialValue: '' });
  
  produtoSelecionado = computed(() => {
    const codigo = this.codigoSelecionado();
    return this.produtosDisponiveis().find(p => p.codigo === codigo) ?? null;
  });

  ngOnInit(): void {
    this.numero = Number(this.route.snapshot.paramMap.get('numero'));
    this.carregarDados();

    this.formItem.controls.codigoProduto.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(codigo => {
        const produto = this.produtosDisponiveis().find(p => p.codigo === codigo);
        if (produto && produto.saldo > 0) {
          this.formItem.controls.quantidade.setValidators([
            Validators.required,
            Validators.min(0.001),
            Validators.max(produto.saldo)
          ]);
        } else {
          this.formItem.controls.quantidade.setValidators([
            Validators.required,
            Validators.min(0.001)
          ]);
        }
        this.formItem.controls.quantidade.updateValueAndValidity();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarDados(): void {
    this.carregando.set(true);
    this.erroConexao.set(false);

    this.faturamentoService.obter(this.numero)
      .pipe(
        timeout(10000),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (nota: NotaFiscal) => {
          this.nota.set(nota);
          if (nota.status === 'Aberta') {
            this.carregarProdutos();
          } else {
            this.carregando.set(false);
          }
        },
        error: (err) => {
          this.erroConexao.set(true);
          this.carregando.set(false);
          if (err instanceof TimeoutError) {
            this.notification.erro('O servidor demorou muito para responder. Verifique se o backend está rodando.', 'Timeout');
          }
        }
      });
  }

  private carregarProdutos(): void {
    this.estoqueService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (produtos: Produto[]) => {
          this.produtosDisponiveis.set(produtos);
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.notification.aviso('Serviço de Estoque indisponível. A seleção de produtos pode estar incompleta.');
        }
      });
  }

  adicionarItem(): void {
    const notaAtual = this.nota();
    if (this.formItem.invalid || !notaAtual) return;

    this.adicionando.set(true);
    this.erroAcao.set('');

    const { codigoProduto, quantidade } = this.formItem.getRawValue();
    const produtoSelecionado = this.produtosDisponiveis().find(p => p.codigo === codigoProduto);

    if (!produtoSelecionado) {
      const msg = 'Produto não encontrado na base.';
      this.erroAcao.set(msg);
      this.adicionando.set(false);
      this.notification.erro(msg, 'Erro');
      return;
    }

    if (produtoSelecionado.saldo < quantidade) {
      const msg = `Saldo insuficiente. Disponível: ${produtoSelecionado.saldo}`;
      this.erroAcao.set(msg);
      this.adicionando.set(false);
      this.notification.aviso(msg, 'Estoque Insuficiente');
      return;
    }

    const request: AdicionarItemRequest = {
      codigoProduto: produtoSelecionado.codigo,
      descricaoProduto: produtoSelecionado.descricao,
      quantidade
    };

    this.faturamentoService.adicionarItem(this.numero, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notaAtualizada: NotaFiscal) => {
          this.nota.set(notaAtualizada);
          this.adicionando.set(false);
          this.formItem.reset({ codigoProduto: '', quantidade: 1 });
          this.notification.sucesso(`${produtoSelecionado.descricao} adicionado à nota.`);
        },
        error: (err: HttpErrorResponse) => {
          this.adicionando.set(false);
          const msg = err.error?.mensagem ?? err.error?.message ?? 'Erro ao adicionar item.';
          this.erroAcao.set(msg);
          this.notification.erro(msg, 'Erro');
        }
      });
  }

  removendoItem = signal(false);

  removerItem(codigoProduto: string): void {
    const notaAtual = this.nota();
    if (!notaAtual || notaAtual.status !== 'Aberta') return;

    this.removendoItem.set(true);
    this.faturamentoService.removerItem(this.numero, codigoProduto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notaAtualizada: NotaFiscal) => {
          this.nota.set(notaAtualizada);
          this.removendoItem.set(false);
          this.notification.sucesso('Item removido da nota fiscal com sucesso.');
        },
        error: (err: HttpErrorResponse) => {
          this.removendoItem.set(false);
          const msg = err.error?.mensagem ?? err.error?.message ?? 'Não foi possível remover o item.';
          this.notification.erro(msg, 'Erro ao Remover');
        }
      });
  }

  confirmarFechamento(): void {
    const notaAtual = this.nota();
    if (!notaAtual || notaAtual.itens.length === 0) return;

    this.confirmationService.confirm({
      message: `Deseja realmente fechar a Nota #${this.numero}? O estoque será debitado e a ação não pode ser desfeita.`,
      header: 'Fechar Nota Fiscal',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Fechar Nota',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.fecharNota()
    });
  }

  private fecharNota(): void {
    this.fechando.set(true);
    this.erroAcao.set('');

    this.faturamentoService.fechar(this.numero)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notaFechada: NotaFiscal) => {
          this.nota.set(notaFechada);
          this.fechando.set(false);
          this.notification.sucesso(
            'Nota fiscal fechada! O saldo dos produtos foi debitado do estoque.',
            'Nota Fechada'
          );
        },
        error: (err: HttpErrorResponse) => {
          this.fechando.set(false);
          const msg = err.status === 503
            ? (err.error?.mensagem ?? 'O Serviço de Estoque está indisponível. A nota não pôde ser fechada. Tente novamente.')
            : (err.error?.mensagem ?? err.error?.message ?? 'Erro ao fechar a nota fiscal.');

          this.erroAcao.set(msg);

          if (err.status !== 503) {
            this.notification.erro(msg, 'Falha no Fechamento');
          }
        }
      });
  }

  getStatusSeverity(status: string): 'info' | 'success' {
    return status === 'Aberta' ? 'info' : 'success';
  }
}
