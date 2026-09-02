import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produto, ProdutoRequest, AlterarSaldoRequest } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class EstoqueService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/estoque/produtos';

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl);
  }

  obterPorCodigo(codigo: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.baseUrl}/${codigo}`);
  }

  cadastrar(request: ProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, request);
  }

  alterarSaldo(codigo: string, request: AlterarSaldoRequest): Observable<Produto> {
    return this.http.patch<Produto>(`${this.baseUrl}/${codigo}/saldo`, request);
  }
}
