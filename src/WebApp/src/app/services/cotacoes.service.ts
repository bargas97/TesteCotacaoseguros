import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  AplicarDescontoRequestDto,
  CotacaoResponseDto,
  CriarRascunhoCotacaoRequestDto,
} from '../models/dtos';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CotacoesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/cotacoes`;

  criarRascunho(dto: CriarRascunhoCotacaoRequestDto) {
    return this.http.post<{ id: number }>(this.base, dto);
  }

  obter(id: number) {
    return this.http.get<CotacaoResponseDto>(`${this.base}/${id}`);
  }

  buscar(cpf?: string, status?: number) {
    const params: Record<string, string> = {};
    if (cpf) params['cpf'] = cpf;
    if (status !== undefined && status !== null) params['status'] = String(status);
    return this.http.get<CotacaoResponseDto[]>(this.base, { params });
  }

  calcular(id: number) {
    return this.http.put<void>(`${this.base}/${id}/calcular`, {});
  }

  aplicarDesconto(id: number, percentual: number) {
    const dto: AplicarDescontoRequestDto = { cotacaoId: id, percentual };
    return this.http.put<void>(`${this.base}/${id}/desconto-comercial`, dto);
  }

  aprovar(id: number) {
    return this.http.put<void>(`${this.base}/${id}/aprovar`, {});
  }

  cancelar(id: number) {
    return this.http.put<void>(`${this.base}/${id}/cancelar`, {});
  }
}
