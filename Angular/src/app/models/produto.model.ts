export interface Produto {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface ProdutoRequest {
  codigo: string;
  descricao: string;
  saldoInicial: number;
}

export interface AlterarSaldoRequest {
  quantidade: number;
}
