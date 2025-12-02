import { Routes } from '@angular/router';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'cotacoes' },
	{
		path: 'cotacoes',
		loadComponent: () => import('./pages/cotacoes-list.component').then(m => m.CotacoesListComponent),
	},
	{
		path: 'cotacoes/nova',
		loadComponent: () => import('./pages/nova-cotacao.component').then(m => m.NovaCotacaoComponent),
	},
	{
		path: 'cotacoes/:id',
		loadComponent: () => import('./pages/cotacao-detail.component').then(m => m.CotacaoDetailComponent),
	},
];
