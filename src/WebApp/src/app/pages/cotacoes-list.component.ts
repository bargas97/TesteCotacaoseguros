import { Component, inject, signal, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CotacoesService } from '../services/cotacoes.service';
import { CotacaoResponseDto, StatusCotacao } from '../models/dtos';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-cotacoes-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatTableModule, MatButtonModule, MatProgressBarModule, MatSnackBarModule],
  template: `
    <div class="container" data-testid="cotacoes-list">
      <h2 data-testid="titulo-cotacoes">Cotações</h2>
      <mat-progress-bar *ngIf="loading()" mode="indeterminate"></mat-progress-bar>

      <div class="filters">
        <label>
          CPF:
          <input [(ngModel)]="cpf" placeholder="Somente números" />
        </label>
        <label>
          Status:
          <select [(ngModel)]="status">
            <option [ngValue]="undefined">Todos</option>
            <option *ngFor="let s of statusOptions" [ngValue]="s.value">{{ s.label }}</option>
          </select>
        </label>
        <button mat-raised-button color="primary" (click)="buscar()" data-testid="btn-buscar">Buscar</button>
        <a mat-stroked-button color="accent" routerLink="/cotacoes/nova" data-testid="link-nova-cotacao">+ Nova Cotação</a>
      </div>

      <table mat-table [dataSource]="items()" class="mat-elevation-z1" *ngIf="items().length; else vazio" data-testid="tabela-cotacoes">
        <ng-container matColumnDef="id">
          <th mat-header-cell *matHeaderCellDef>#</th>
          <td mat-cell *matCellDef="let c">{{ c.id }}</td>
        </ng-container>
        <ng-container matColumnDef="numero">
          <th mat-header-cell *matHeaderCellDef>Número</th>
          <td mat-cell *matCellDef="let c">{{ c.numero }}</td>
        </ng-container>
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let c">{{ c.status }}</td>
        </ng-container>
        <ng-container matColumnDef="produtoId">
          <th mat-header-cell *matHeaderCellDef>Produto</th>
          <td mat-cell *matCellDef="let c">{{ c.produtoId }}</td>
        </ng-container>
        <ng-container matColumnDef="premioTotal">
          <th mat-header-cell *matHeaderCellDef>Prêmio Total</th>
          <td mat-cell *matCellDef="let c">{{ c.premioTotal | number:'1.2-2' }}</td>
        </ng-container>
        <ng-container matColumnDef="acoes">
          <th mat-header-cell *matHeaderCellDef>Ações</th>
          <td mat-cell *matCellDef="let c">
            <a mat-button color="primary" [routerLink]="['/cotacoes', c.id]">Abrir</a>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <ng-template #vazio>
        <p>Nenhum resultado.</p>
      </ng-template>
    </div>
  `,
  styles: [
    `
      .container { padding: 1rem; }
      .filters { display: flex; gap: .5rem; align-items: center; margin: .75rem 0; }
      table { width: 100%; }
      input, select, button, a { padding: .25rem .5rem; }
    `,
  ],
})
export class CotacoesListComponent {
  private readonly api = inject(CotacoesService);
  private readonly snack = inject(MatSnackBar);
  items = signal<CotacaoResponseDto[]>([]);
  cpf: string | undefined;
  status: number | undefined;
  loading = signal(false);
  displayedColumns = ['id','numero','status','produtoId','premioTotal','acoes'];

  statusOptions = Object.entries(StatusCotacao)
    .filter(([k]) => isNaN(Number(k)))
    .map(([k, v]) => ({ label: k, value: Number(v) }));

  constructor() {
    const platformId = inject(PLATFORM_ID);
    if (isPlatformBrowser(platformId)) {
      this.buscar();
    }
  }

  buscar() {
    this.loading.set(true);
    this.api.buscar(this.cpf, this.status).subscribe({
      next: (res) => this.items.set(res),
      error: (err) => this.snack.open('Erro ao buscar cotações','Fechar',{duration:3000}),
      complete: () => this.loading.set(false)
    });
  }
}
