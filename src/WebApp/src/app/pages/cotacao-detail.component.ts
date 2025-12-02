import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, inject, signal, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CotacoesService } from '../services/cotacoes.service';
import { CotacaoResponseDto } from '../models/dtos';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-cotacao-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, MatTableModule, MatButtonModule, MatProgressBarModule, MatSnackBarModule],
  template: `
    <div class="container" *ngIf="cotacao(); else carregando" data-testid="cotacao-detail">
      <a mat-button routerLink="/cotacoes" data-testid="link-voltar">← Voltar</a>
      <h2 data-testid="titulo-cotacao">Cotação #{{ cotacao()!.id }} - {{ cotacao()!.numero }}</h2>
      <mat-progress-bar *ngIf="loading()" mode="indeterminate"></mat-progress-bar>
      <p data-testid="status">Status: <strong>{{ cotacao()!.status }}</strong></p>
      <p data-testid="premio-total">Prêmio Total: <strong>{{ cotacao()!.premioTotal | number:'1.2-2' }}</strong></p>

      <div class="actions">
        <button mat-raised-button color="primary" (click)="calcular()" data-testid="btn-calcular">Calcular</button>
        <label>
          Desconto (%):
          <input type="number" [(ngModel)]="percentual" min="0" max="100" step="0.01" />
        </label>
        <button mat-stroked-button color="accent" (click)="aplicarDesconto()" data-testid="btn-desconto">Aplicar Desconto</button>
        <button mat-raised-button color="accent" (click)="aprovar()" data-testid="btn-aprovar">Aprovar</button>
        <button mat-stroked-button color="warn" (click)="cancelar()" data-testid="btn-cancelar">Cancelar</button>
      </div>

      <h3>Coberturas</h3>
      <table mat-table [dataSource]="cotacao()!.coberturas" class="mat-elevation-z1">
        <ng-container matColumnDef="coberturaId">
          <th mat-header-cell *matHeaderCellDef>CoberturaId</th>
          <td mat-cell *matCellDef="let c">{{ c.coberturaId }}</td>
        </ng-container>
        <ng-container matColumnDef="importanciaSegurada">
          <th mat-header-cell *matHeaderCellDef>Importância</th>
          <td mat-cell *matCellDef="let c">{{ c.importanciaSegurada | number:'1.2-2' }}</td>
        </ng-container>
        <ng-container matColumnDef="premioCobertura">
          <th mat-header-cell *matHeaderCellDef>Prêmio</th>
          <td mat-cell *matCellDef="let c">{{ c.premioCobertura | number:'1.2-2' }}</td>
        </ng-container>
        <ng-container matColumnDef="franquiaSelecionada">
          <th mat-header-cell *matHeaderCellDef>Franquia</th>
          <td mat-cell *matCellDef="let c">{{ c.franquiaSelecionada || '-' }}</td>
        </ng-container>
        <ng-container matColumnDef="contratada">
          <th mat-header-cell *matHeaderCellDef>Contratada</th>
          <td mat-cell *matCellDef="let c">{{ c.contratada ? 'Sim' : 'Não' }}</td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="colunasCoberturas"></tr>
        <tr mat-row *matRowDef="let row; columns: colunasCoberturas;"></tr>
      </table>
    </div>
    <ng-template #carregando>
      <p>Carregando...</p>
    </ng-template>
  `,
  styles: [
    `
      .container { padding: 1rem; display: block; }
      table { width: 100%; margin-top: .5rem; }
      .actions { display: flex; align-items: center; gap: .5rem; margin: .75rem 0; }
      input, button { padding: .25rem .5rem; }
    `,
  ],
})
export class CotacaoDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CotacoesService);
  private readonly snack = inject(MatSnackBar);
  cotacao = signal<CotacaoResponseDto | null>(null);
  percentual = 0;
  loading = signal(false);
  colunasCoberturas = ['coberturaId','importanciaSegurada','premioCobertura','franquiaSelecionada','contratada'];

  constructor() {
    const platformId = inject(PLATFORM_ID);
    if (isPlatformBrowser(platformId)) {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      this.carregar(id);
    }
  }

  private carregar(id: number) {
    this.loading.set(true);
    this.api.obter(id).subscribe({
      next: (c) => this.cotacao.set(c),
      error: () => this.snack.open('Erro ao carregar cotação','Fechar',{duration:3000}),
      complete: () => this.loading.set(false)
    });
  }

  calcular() {
    const id = this.cotacao()?.id; if (!id) return;
    this.loading.set(true);
    this.api.calcular(id).subscribe({
      next: () => { this.snack.open('Cálculo concluído','Fechar',{duration:2000}); this.carregar(id); },
      error: () => { this.loading.set(false); this.snack.open('Erro ao calcular','Fechar',{duration:3000}); }
    });
  }

  aplicarDesconto() {
    const id = this.cotacao()?.id; if (!id) return;
    this.loading.set(true);
    this.api.aplicarDesconto(id, this.percentual).subscribe({
      next: () => { this.snack.open('Desconto aplicado','Fechar',{duration:2000}); this.carregar(id); },
      error: () => { this.loading.set(false); this.snack.open('Erro ao aplicar desconto','Fechar',{duration:3000}); }
    });
  }

  aprovar() {
    const id = this.cotacao()?.id; if (!id) return;
    this.loading.set(true);
    this.api.aprovar(id).subscribe({
      next: () => { this.snack.open('Cotação aprovada','Fechar',{duration:2000}); this.carregar(id); },
      error: () => { this.loading.set(false); this.snack.open('Erro ao aprovar','Fechar',{duration:3000}); }
    });
  }

  cancelar() {
    const id = this.cotacao()?.id; if (!id) return;
    this.loading.set(true);
    this.api.cancelar(id).subscribe({
      next: () => { this.snack.open('Cotação cancelada','Fechar',{duration:2000}); this.carregar(id); },
      error: () => { this.loading.set(false); this.snack.open('Erro ao cancelar','Fechar',{duration:3000}); }
    });
  }
}
