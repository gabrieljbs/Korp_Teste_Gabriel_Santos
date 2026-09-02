import { Component, inject, OnDestroy } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EstoqueService } from '../../../services/estoque.service';
import { HttpErrorResponse } from '@angular/common/http';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationService } from '../../../core/services/notification.service';
import { FormErrorPipe } from '../../../core/pipes/form-error.pipe';

@Component({
  selector: 'app-estoque-cadastrar',
  imports: [ReactiveFormsModule, RouterLink, InputTextModule, InputNumber, ButtonModule, CardModule, FormErrorPipe],
  templateUrl: './estoque-cadastrar.html'
})
export class EstoqueCadastrarComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly estoqueService = inject(EstoqueService);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);
  private readonly destroy$ = new Subject<void>();

  form = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^\S+.*$/)]],
    descricao: ['', [Validators.required, Validators.maxLength(200)]],
    saldoInicial: [0 as number, [Validators.required, Validators.min(0)]]
  });

  salvando = false;

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.salvando = true;

    const { codigo, descricao, saldoInicial } = this.form.getRawValue();

    this.estoqueService.cadastrar({ codigo, descricao, saldoInicial })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notification.sucesso(`Produto "${descricao}" cadastrado com sucesso!`);
          this.router.navigate(['/estoque']);
        },
        error: (err: HttpErrorResponse) => {
          this.salvando = false;
          if (err.status !== 0 && err.status !== 503) {
            const mensagem = err.status === 409
              ? `Já existe um produto com o código "${codigo}".`
              : err.error?.mensagem ?? err.error?.message ?? 'Não foi possível cadastrar o produto.';
            this.notification.erro(mensagem, 'Falha no Cadastro');
          }
        }
      });
  }

  isInvalid(campo: string): boolean {
    const ctrl = this.form.get(campo);
    return !!(ctrl?.invalid && (ctrl.dirty || ctrl.touched));
  }
}
