import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, inject, PLATFORM_ID } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Router } from '@angular/router';
import { CotacoesService } from '../services/cotacoes.service';
import { CatalogoService, CoberturaItem, ProdutoItem } from '../services/catalogo.service';
import { CriarRascunhoCotacaoRequestDto, Genero, TipoUtilizacao } from '../models/dtos';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { NgxMaskDirective, NgxMaskPipe } from 'ngx-mask';

@Component({
  selector: 'app-nova-cotacao',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    // Angular Material
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonToggleModule,
    MatSnackBarModule,
    MatProgressBarModule,
    // ngx-mask
    NgxMaskDirective,
    NgxMaskPipe,
  ],
  template: `
    <div class="container" data-testid="nova-cotacao">
      <h2 data-testid="titulo-nova">Nova Cotação</h2>
      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>
      <form [formGroup]="form" (ngSubmit)="salvar()" class="form-grid">
        <section class="grupo">
          <h3>Produto</h3>
          <mat-form-field appearance="outline">
            <mat-label>Produto</mat-label>
            <mat-select data-testid="select-produto" formControlName="produtoId" (selectionChange)="onProdutoChange()">
              <mat-option [value]="null">Selecione...</mat-option>
              <mat-option *ngFor="let p of produtos" [value]="p.id">{{ p.nome }}</mat-option>
            </mat-select>
            <mat-error *ngIf="form.get('produtoId')?.invalid && form.get('produtoId')?.touched">Produto obrigatório</mat-error>
          </mat-form-field>
        </section>

        <section class="grupo">
          <h3>Proponente</h3>
          <mat-form-field appearance="outline" data-testid="campo-nome">
            <mat-label>Nome</mat-label>
            <input matInput formControlName="nome" />
            <mat-error *ngIf="fc('nome').invalid && fc('nome').touched">Nome obrigatório</mat-error>
          </mat-form-field>

          <mat-button-toggle-group name="docTipo" [(ngModel)]="docTipo" aria-label="Tipo Doc">
            <mat-button-toggle value="CPF">CPF</mat-button-toggle>
            <mat-button-toggle value="CNPJ">CNPJ</mat-button-toggle>
          </mat-button-toggle-group>

          <mat-form-field appearance="outline" data-testid="campo-doc">
            <mat-label>{{ docTipo }}</mat-label>
            <input matInput formControlName="cpfCnpj"
                   [mask]="docMask" [dropSpecialCharacters]="true"
                   (input)="onDocChange(fc('cpfCnpj').value)" />
            <mat-error *ngIf="fc('cpfCnpj').invalid && fc('cpfCnpj').touched">{{ docTipo }} inválido</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Gênero</mat-label>
            <mat-select formControlName="genero">
              <mat-option [value]="Genero.M">Masculino</mat-option>
              <mat-option [value]="Genero.F">Feminino</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" data-testid="campo-estado-civil">
            <mat-label>Estado Civil</mat-label>
            <input matInput formControlName="estadoCivil" />
            <mat-error *ngIf="fc('estadoCivil').invalid && fc('estadoCivil').touched">Obrigatório</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Nascimento</mat-label>
            <input matInput [matDatepicker]="picker" formControlName="dtNascimento" [min]="minDate" [max]="maxDate" />
            <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
            <mat-datepicker #picker></mat-datepicker>
            <mat-error *ngIf="fc('dtNascimento').invalid && fc('dtNascimento').touched">Idade mínima 18 anos</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>CEP Residencial</mat-label>
            <input matInput formControlName="cepResidencial" [mask]="'00000-000'" [dropSpecialCharacters]="true" />
            <mat-error *ngIf="fc('cepResidencial').invalid && fc('cepResidencial').touched">CEP inválido</mat-error>
          </mat-form-field>
        </section>

        <section class="grupo">
          <h3>Veículo</h3>
          <mat-form-field appearance="outline" data-testid="campo-fipe">
            <mat-label>Código FIPE/Veículo</mat-label>
            <input matInput formControlName="codigoFipeOuVeiculo" />
            <mat-error *ngIf="fv('codigoFipeOuVeiculo').invalid && fv('codigoFipeOuVeiculo').touched">Obrigatório</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Ano Modelo</mat-label>
            <input matInput type="number" formControlName="anoModelo" />
            <mat-error *ngIf="fv('anoModelo').invalid && fv('anoModelo').touched">Ano inválido</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Ano Fabricação</mat-label>
            <input matInput type="number" formControlName="anoFabricacao" />
            <mat-error *ngIf="fv('anoFabricacao').invalid && fv('anoFabricacao').touched">Ano inválido</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>CEP Pernoite</mat-label>
            <input matInput formControlName="cepPernoite" [mask]="'00000-000'" [dropSpecialCharacters]="true" />
            <mat-error *ngIf="fv('cepPernoite').invalid && fv('cepPernoite').touched">CEP inválido</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" data-testid="campo-placa">
            <mat-label>Placa</mat-label>
            <input matInput formControlName="placa" [mask]="plateMask" [dropSpecialCharacters]="false"
                   (input)="onPlacaInput($event)" placeholder="ABC-1234 ou ABC1D23" />
            <mat-hint>{{ plateMask === 'SSS-0000' ? 'Formato antigo' : 'Mercosul' }}</mat-hint>
            <mat-error *ngIf="fv('placa').hasError('placaInvalida') && fv('placa').touched">Placa inválida</mat-error>
            <mat-error *ngIf="fv('placa').hasError('required') && fv('placa').touched">Obrigatória</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Utilização</mat-label>
            <mat-select formControlName="tipoUtilizacao">
              <mat-option [value]="TipoUtilizacao.Particular">Particular</mat-option>
              <mat-option [value]="TipoUtilizacao.Profissional">Profissional</mat-option>
              <mat-option [value]="TipoUtilizacao.Aplicativo">Aplicativo</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-checkbox formControlName="zeroKm">Zero Km</mat-checkbox>
          <mat-checkbox formControlName="blindado">Blindado</mat-checkbox>
          <mat-checkbox formControlName="kitGas">Kit Gás</mat-checkbox>
        </section>

        <section class="grupo">
          <h3>Coberturas</h3>
          <div formArrayName="coberturas" data-testid="lista-coberturas">
            <div *ngFor="let c of coberturasArray.controls; let i = index" [formGroupName]="i" class="linha-cobertura">
            <mat-form-field appearance="outline">
              <mat-label>Cobertura</mat-label>
              <mat-select [attr.data-testid]="'select-cobertura-' + i" formControlName="coberturaId">
                <mat-option [value]="undefined">Selecione...</mat-option>
                <mat-option *ngFor="let cb of coberturas" [value]="cb.id">{{ cb.codigo }} - {{ cb.descricao }}</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Importância</mat-label>
              <input matInput type="number" step="0.01" formControlName="importanciaSegurada" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Franquia</mat-label>
              <input matInput formControlName="franquiaSelecionada" />
            </mat-form-field>

            <mat-checkbox formControlName="contratada">Contratada</mat-checkbox>
            <button mat-stroked-button color="warn" type="button" (click)="removerCobertura(i)">Remover</button>
          </div>
          <button mat-stroked-button color="primary" type="button" (click)="adicionarCobertura()">+ Cobertura</button>
          </div>
        </section>

        <div class="acoes">
          <button mat-raised-button color="primary" type="submit" data-testid="btn-salvar" [disabled]="form.invalid || !docValido || loading">Salvar</button>
        </div>
      </form>
    </div>
  `,
  styles: [
    `
      .container { padding: 1rem; }
      .form-grid { display: grid; gap: 1rem; }
      .grupo { display: grid; grid-template-columns: repeat(2, minmax(260px, 1fr)); gap: 1rem; }
      .grupo h3 { grid-column: 1 / -1; margin: .25rem 0; }
      .linha-cobertura { display: grid; grid-template-columns: repeat(5, minmax(180px, 1fr)); gap: .5rem; align-items: center; }
      .acoes { margin-top: 1rem; }
    `,
  ],
})
export class NovaCotacaoComponent {
  private readonly api = inject(CotacoesService);
  private readonly router = inject(Router);
  private readonly catalogo = inject(CatalogoService);
  private readonly snack = inject(MatSnackBar);

  Genero = Genero;
  TipoUtilizacao = TipoUtilizacao;

  produtos: ProdutoItem[] = [];
  coberturas: CoberturaItem[] = [];

  form: FormGroup;
  minDate = new Date(new Date().getFullYear() - 120, 0, 1);
  maxDate = new Date();
  docTipo: 'CPF' | 'CNPJ' = 'CPF';
  docValido = true;
  loading = false;

  get docMask() {
    return this.docTipo === 'CPF' ? '000.000.000-00' : '00.000.000/0000-00';
  }

  get coberturasArray(): FormArray { return this.form.get('coberturas') as FormArray; }

  adicionarCobertura() {
    this.coberturasArray.push(this.fb.group({
      coberturaId: [null, Validators.required],
      importanciaSegurada: [0, [Validators.required, Validators.min(1)]],
      contratada: [false],
      franquiaSelecionada: [null]
    }));
  }

  removerCobertura(i: number) {
    this.coberturasArray.removeAt(i);
  }

  salvar() {
    if (this.form.invalid || !this.docValido) return;
    const v = this.form.value;
    const payload: CriarRascunhoCotacaoRequestDto = {
      produtoId: v.produtoId,
      proponente: {
        nome: v.nome,
        cpfCnpj: v.cpfCnpj,
        genero: v.genero,
        estadoCivil: v.estadoCivil,
        dtNascimento: this.toIsoDate(v.dtNascimento),
        cepResidencial: v.cepResidencial
      },
      veiculo: {
        codigoFipeOuVeiculo: v.codigoFipeOuVeiculo,
        anoModelo: v.anoModelo,
        anoFabricacao: v.anoFabricacao,
        cepPernoite: v.cepPernoite,
        tipoUtilizacao: v.tipoUtilizacao,
        zeroKm: v.zeroKm,
        blindado: v.blindado,
        kitGas: v.kitGas,
        placa: v.placa?.toUpperCase()
      },
      coberturas: (v.coberturas || []).map((c: any) => ({
        coberturaId: c.coberturaId,
        importanciaSegurada: c.importanciaSegurada,
        contratada: c.contratada,
        franquiaSelecionada: c.franquiaSelecionada
      }))
    };
    this.loading = true;
    this.api.criarRascunho(payload).subscribe({
      next: ({ id }) => {
        this.snack.open('Cotação criada','Fechar',{duration:2000});
        this.router.navigate(['/cotacoes', id]);
      },
      error: () => {
        this.loading = false;
        this.snack.open('Erro ao criar cotação','Fechar',{duration:3000});
      }
    });
  }

  constructor() {
    const platformId = inject(PLATFORM_ID);
    this.fb = inject(FormBuilder);
    this.form = this.fb.group({
      produtoId: [null, Validators.required],
      // Proponente
      nome: ['', Validators.required],
      cpfCnpj: ['', Validators.required],
      genero: [Genero.M, Validators.required],
      estadoCivil: ['', Validators.required],
      dtNascimento: [new Date(), [Validators.required, this.maiorDeIdadeValidator(18)]],
      cepResidencial: ['', [Validators.required, Validators.minLength(8)]],
      // Veículo
      codigoFipeOuVeiculo: ['', Validators.required],
      anoModelo: [new Date().getFullYear(), [Validators.required, Validators.min(1950)]],
      anoFabricacao: [new Date().getFullYear(), [Validators.required, Validators.min(1950)]],
      cepPernoite: ['', [Validators.required, Validators.minLength(8)]],
      tipoUtilizacao: [TipoUtilizacao.Particular, Validators.required],
      zeroKm: [false],
      blindado: [false],
      kitGas: [false],
      placa: ['', [Validators.required, this.placaValidator()]],
      coberturas: this.fb.array([])
    });
    // inicia com uma cobertura
    this.adicionarCobertura();
    if (isPlatformBrowser(platformId)) {
      this.catalogo.getProdutos().subscribe(list => this.produtos = list);
    }
  }

  onProdutoChange() {
    const pid = this.form.get('produtoId')?.value;
    if (!pid) { this.coberturas = []; return; }
    this.catalogo.getCoberturas(pid).subscribe(list => this.coberturas = list);
  }

  private toIsoDate(d: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    const yyyy = d.getFullYear();
    const mm = pad(d.getMonth() + 1);
    const dd = pad(d.getDate());
    return `${yyyy}-${mm}-${dd}`;
  }

  private onlyDigits(v: string): string {
    return (v || '').replace(/\D+/g, '');
  }

  isCpf(v: string): boolean {
    const cpf = this.onlyDigits(v);
    if (!cpf || cpf.length !== 11) return false;
    if (/^(\d)\1{10}$/.test(cpf)) return false; // sequências repetidas
    const calc = (base: string, factor: number) => {
      let total = 0;
      for (let i = 0; i < base.length; i++) total += parseInt(base[i], 10) * (factor - i);
      const rest = (total * 10) % 11;
      return rest === 10 ? 0 : rest;
    };
    const d1 = calc(cpf.substring(0, 9), 10);
    const d2 = calc(cpf.substring(0, 10), 11);
    return d1 === parseInt(cpf[9], 10) && d2 === parseInt(cpf[10], 10);
  }

  isCnpj(v: string): boolean {
    const cnpj = this.onlyDigits(v);
    if (!cnpj || cnpj.length !== 14) return false;
    if (/^(\d)\1{13}$/.test(cnpj)) return false;
    const calc = (base: string, factors: number[]) => {
      let total = 0;
      for (let i = 0; i < base.length; i++) total += parseInt(base[i], 10) * factors[i];
      const rest = total % 11;
      return rest < 2 ? 0 : 11 - rest;
    };
    const f1 = [5,4,3,2,9,8,7,6,5,4,3,2];
    const f2 = [6,5,4,3,2,9,8,7,6,5,4,3,2];
    const d1 = calc(cnpj.substring(0, 12), f1);
    const d2 = calc(cnpj.substring(0, 12) + d1.toString(), f2);
    return d1 === parseInt(cnpj[12], 10) && d2 === parseInt(cnpj[13], 10);
  }

  onDocChange(value: string) {
    this.docValido = this.docTipo === 'CPF' ? this.isCpf(value) : this.isCnpj(value);
    if (!this.docValido) this.form.get('cpfCnpj')?.setErrors({ invalid: true });
    else this.form.get('cpfCnpj')?.setErrors(null);
  }

  private fb: FormBuilder;
  fc(name: string) { return this.form.get(name)!; }
  fv(name: string) { return this.form.get(name)!; }
  private maiorDeIdadeValidator(idadeMin: number) {
    return (ctrl: any) => {
      const v = ctrl.value as Date;
      if (!v) return { invalidDate: true };
      const hoje = new Date();
      let idade = hoje.getFullYear() - v.getFullYear();
      if (v > new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate())) idade--;
      return idade < idadeMin ? { menorIdade: true } : null;
    };
  }

  plateMask = 'SSS-0000';
  onPlacaInput(ev: any) {
    const inputEl = ev.target as HTMLInputElement;
    let value = (inputEl.value || '').toUpperCase();
    // remove espaços
    value = value.replace(/\s+/g,'');
    // decide máscara
    const raw = value.replace('-', '');
    const mercosulRegex = /^[A-Z]{3}[0-9][A-Z][0-9]{2}$/;
    const oldRegex = /^[A-Z]{3}[0-9]{4}$/;
    if (mercosulRegex.test(raw)) this.plateMask = 'SSS0S00'; else this.plateMask = 'SSS-0000';
    this.form.get('placa')?.setValue(value, { emitEvent: false });
    // valida manualmente
    const ctrl = this.form.get('placa');
    if (!mercosulRegex.test(raw) && !oldRegex.test(raw)) ctrl?.setErrors({ ...(ctrl.errors||{}), placaInvalida: true });
    else if (ctrl?.errors) {
      const { placaInvalida, ...rest } = ctrl.errors;
      ctrl.setErrors(Object.keys(rest).length ? rest : null);
    }
  }

  private placaValidator() {
    return (ctrl: any) => {
      const v: string = (ctrl.value || '').toUpperCase();
      const raw = v.replace('-', '');
      if (!raw) return { required: true };
      const mercosulRegex = /^[A-Z]{3}[0-9][A-Z][0-9]{2}$/;
      const oldRegex = /^[A-Z]{3}[0-9]{4}$/;
      return mercosulRegex.test(raw) || oldRegex.test(raw) ? null : { placaInvalida: true };
    };
  }
}
