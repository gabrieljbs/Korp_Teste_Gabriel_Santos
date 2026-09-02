import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal, AdicionarItemRequest } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class FaturamentoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/faturamento/notas';

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.baseUrl);
  }

  obter(numero: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.baseUrl}/${numero}`);
  }

  criar(): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, {});
  }

  adicionarItem(numero: number, request: AdicionarItemRequest): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.baseUrl}/${numero}/itens`, request);
  }

  removerItem(numero: number, codigoProduto: string): Observable<NotaFiscal> {
    return this.http.delete<NotaFiscal>(`${this.baseUrl}/${numero}/itens/${encodeURIComponent(codigoProduto)}`);
  }

  fechar(numero: number): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.baseUrl}/${numero}/fechar`, {});
  }
}
