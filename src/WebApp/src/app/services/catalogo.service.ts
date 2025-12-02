import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface ProdutoItem { id: number; nome: string; ativo: boolean; }
export interface CoberturaItem {
  id: number; codigo: string; descricao: string;
  tipo: number; valorMinimo?: number | null; valorMaximo?: number | null;
}

@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/catalogo`;

  getProdutos() {
    return this.http.get<ProdutoItem[]>(`${this.base}/produtos`);
  }

  getCoberturas(produtoId: number) {
    return this.http.get<CoberturaItem[]>(`${this.base}/produtos/${produtoId}/coberturas`);
  }
}
