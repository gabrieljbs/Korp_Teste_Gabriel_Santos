export type StatusNotaFiscal = 'Aberta' | 'Fechada';

export interface ItemNotaFiscal {
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  numero: number;
  status: StatusNotaFiscal;
  itens: ItemNotaFiscal[];
}

export interface AdicionarItemRequest {
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
}
