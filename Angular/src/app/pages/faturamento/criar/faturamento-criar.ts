import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { FaturamentoService } from '../../../services/faturamento.service';
import { EstoqueService } from '../../../services/estoque.service';
import { Produto } from '../../../models/produto.model';
import { AdicionarItemRequest } from '../../../models/nota-fiscal.model';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputNumber } from 'primeng/inputnumber';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { Subject, from } from 'rxjs';
import { concatMap, last, switchMap, takeUntil } from 'rxjs/operators';
import { NotificationService } from '../../../core/services/notification.service';
import { FormErrorPipe } from '../../../core/pipes/form-error.pipe';

export interface ItemRascunho {
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
  saldoDisponivel: number;
  editando: boolean;
  quantidadeEditando: number;
}

@Component({
  selector: 'app-faturamento-criar',
  imports: [
    ReactiveFormsModule, FormsModule, RouterLink, CommonModule,
    TableModule, ButtonModule, InputNumber, CardModule, MessageModule,
    TooltipModule, FormErrorPipe
  ],
  templateUrl: './faturamento-criar.html'
})
export class FaturamentoCriarComponent implements OnInit, OnDestroy {
  private readonly faturamentoService = inject(FaturamentoService);
  private readonly estoqueService = inject(EstoqueService);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();

  produtosDisponiveis = signal<Produto[]>([]);
  itensDaNota = signal<ItemRascunho[]>([]);
  carregando = signal(true);
  erroConexao = signal(false);
  salvando = signal(false);
  erroAcao = signal('');

  form = this.fb.nonNullable.group({
    codigoProduto: ['', Validators.required],
    descricao: [{ value: '', disabled: true }],
    saldoDisponivel: [{ value: 0, disabled: true }],
    quantidade: [1 as number, [Validators.required, Validators.min(0.001)]]
  });

  codigoSelecionado = toSignal(this.form.controls.codigoProduto.valueChanges, { initialValue: '' });

  produtoSelecionado = computed(() => {
    const codigo = this.codigoSelecionado();
    return this.produtosDisponiveis().find(p => p.codigo === codigo) ?? null;
  });

  podeSalvar = computed(() => this.itensDaNota().length > 0 && !this.salvando());

  ngOnInit(): void {
    this.carregarProdutos();

    // Auto-preenche descricao e saldoDisponivel quando o produto muda e atualiza o limite de quantidade
    this.form.controls.codigoProduto.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(codigo => {
        const produto = this.produtosDisponiveis().find(p => p.codigo === codigo);
        this.form.patchValue({
          descricao: produto?.descricao ?? '',
          saldoDisponivel: produto?.saldo ?? 0,
          quantidade: 1
        });

        if (produto && produto.saldo > 0) {
          this.form.controls.quantidade.setValidators([
            Validators.required,
            Validators.min(0.001),
            Validators.max(produto.saldo)
          ]);
        } else {
          this.form.controls.quantidade.setValidators([
            Validators.required,
            Validators.min(0.001)
          ]);
        }
        this.form.controls.quantidade.updateValueAndValidity();
      });
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
        next: (produtos: Produto[]) => {
          this.produtosDisponiveis.set(produtos.filter(p => p.saldo > 0));
          this.carregando.set(false);
        },
        error: () => {
          this.erroConexao.set(true);
          this.carregando.set(false);
        }
      });
  }

  isInvalid(campo: string): boolean {
    const ctrl = this.form.get(campo);
    return !!(ctrl?.invalid && (ctrl.dirty || ctrl.touched));
  }

  adicionarItem(): void {
    const { codigoProduto, quantidade } = this.form.getRawValue();
    const produto = this.produtoSelecionado();

    if (!produto) {
      this.notification.aviso('Selecione um produto antes de adicionar.', 'Atenção');
      return;
    }

    if (quantidade <= 0) {
      this.notification.aviso('Informe uma quantidade maior que zero.', 'Quantidade Inválida');
      return;
    }

    if (quantidade > produto.saldo) {
      this.notification.aviso(
        `Saldo insuficiente! Você informou ${quantidade} unidades, mas há apenas ${produto.saldo} disponíveis no estoque.`,
        'Estoque Insuficiente'
      );
      this.form.controls.quantidade.markAsTouched();
      return;
    }

    const itensAtuais = this.itensDaNota();
    const itemExistente = itensAtuais.find(i => i.codigoProduto === codigoProduto);

    if (itemExistente) {
      const novaQtd = itemExistente.quantidade + quantidade;
      if (novaQtd > produto.saldo) {
        this.notification.aviso(
          `Quantidade total (${novaQtd}) ultrapassa o saldo disponível (${produto.saldo}). Já existem ${itemExistente.quantidade} na nota.`,
          'Estoque Insuficiente'
        );
        return;
      }
      this.itensDaNota.set(itensAtuais.map(i =>
        i.codigoProduto === codigoProduto ? { ...i, quantidade: novaQtd } : i
      ));
      this.notification.sucesso(`Quantidade de "${produto.descricao}" atualizada para ${novaQtd}.`);
    } else {
      const novoItem: ItemRascunho = {
        codigoProduto: produto.codigo,
        descricaoProduto: produto.descricao,
        quantidade,
        saldoDisponivel: produto.saldo,
        editando: false,
        quantidadeEditando: quantidade
      };
      this.itensDaNota.set([...itensAtuais, novoItem]);
      this.notification.sucesso(`"${produto.descricao}" adicionado à nota.`);
    }

    this.form.reset({ codigoProduto: '', quantidade: 1 });
  }

  removerItem(codigo: string): void {
    this.itensDaNota.set(this.itensDaNota().filter(i => i.codigoProduto !== codigo));
  }

  iniciarEdicao(item: ItemRascunho): void {
    this.itensDaNota.set(this.itensDaNota().map(i => ({
      ...i,
      editando: i.codigoProduto === item.codigoProduto,
      quantidadeEditando: i.codigoProduto === item.codigoProduto ? i.quantidade : i.quantidadeEditando
    })));
  }

  confirmarEdicao(item: ItemRascunho): void {
    if (!item.quantidadeEditando || item.quantidadeEditando <= 0 || item.quantidadeEditando > item.saldoDisponivel) return;
    this.itensDaNota.set(this.itensDaNota().map(i =>
      i.codigoProduto === item.codigoProduto
        ? { ...i, quantidade: item.quantidadeEditando, editando: false }
        : i
    ));
  }

  cancelarEdicao(item: ItemRascunho): void {
    this.itensDaNota.set(this.itensDaNota().map(i =>
      i.codigoProduto === item.codigoProduto ? { ...i, editando: false } : i
    ));
  }

  atualizarQuantidadeEditando(codigo: string, valor: number): void {
    this.itensDaNota.set(this.itensDaNota().map(i =>
      i.codigoProduto === codigo ? { ...i, quantidadeEditando: valor } : i
    ));
  }

  salvarNota(): void {
    const itens = this.itensDaNota();
    if (itens.length === 0) return;

    this.salvando.set(true);
    this.erroAcao.set('');

    let numeroCriado: number;

    this.faturamentoService.criar().pipe(
      switchMap(nota => {
        numeroCriado = nota.numero;
        const requests: AdicionarItemRequest[] = itens.map(item => ({
          codigoProduto: item.codigoProduto,
          descricaoProduto: item.descricaoProduto,
          quantidade: item.quantidade
        }));
        return from(requests).pipe(
          concatMap(req => this.faturamentoService.adicionarItem(numeroCriado, req)),
          last()
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.salvando.set(false);
        this.notification.sucesso('Nota fiscal criada com sucesso!', 'Nota Criada');
        this.router.navigate(['/faturamento', numeroCriado]);
      },
      error: (err: HttpErrorResponse) => {
        this.salvando.set(false);
        const msg = err.error?.mensagem ?? err.error?.message ?? 'Não foi possível salvar a nota fiscal.';
        this.erroAcao.set(msg);
        if (err.status !== 0 && err.status !== 503) {
          this.notification.erro(msg, 'Erro ao Salvar');
        }
      }
    });
  }
}
