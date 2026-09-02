import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent) 
  },
  { 
    path: 'estoque', 
    loadComponent: () => import('./pages/estoque/listar/estoque-listar').then(m => m.EstoqueListarComponent) 
  },
  { 
    path: 'estoque/cadastrar', 
    loadComponent: () => import('./pages/estoque/cadastrar/estoque-cadastrar').then(m => m.EstoqueCadastrarComponent) 
  },
  { 
    path: 'faturamento', 
    loadComponent: () => import('./pages/faturamento/listar/faturamento-listar').then(m => m.FaturamentoListarComponent) 
  },
  { 
    path: 'faturamento/criar', 
    loadComponent: () => import('./pages/faturamento/criar/faturamento-criar').then(m => m.FaturamentoCriarComponent) 
  },
  { 
    path: 'faturamento/:numero', 
    loadComponent: () => import('./pages/faturamento/detalhes/faturamento-detalhes').then(m => m.FaturamentoDetalhesComponent) 
  },
  { 
    path: '**', 
    redirectTo: '' 
  }
];
