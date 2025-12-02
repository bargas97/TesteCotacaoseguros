export enum StatusCotacao {
  Rascunho = 0,
  Calculada = 1,
  Aprovada = 2,
  Expirada = 3,
  Cancelada = 4,
}

export enum Genero {
  M = 0,
  F = 1,
}

export enum TipoUtilizacao {
  Particular = 0,
  Profissional = 1,
  Aplicativo = 2,
}

export interface ProponenteDto {
  nome: string;
  cpfCnpj: string;
  genero: Genero;
  estadoCivil: string;
  dtNascimento: string; // ISO string
  cepResidencial: string;
}

export interface VeiculoDto {
  codigoFipeOuVeiculo: string;
  anoModelo: number;
  anoFabricacao: number;
  cepPernoite: string;
  tipoUtilizacao: TipoUtilizacao;
  zeroKm: boolean;
  blindado: boolean;
  kitGas: boolean;
  placa?: string | null;
  chassi?: string | null;
}

export interface CotacaoCoberturaInputDto {
  coberturaId: number;
  importanciaSegurada: number;
  contratada: boolean;
  franquiaSelecionada?: string | null;
}

export interface CriarRascunhoCotacaoRequestDto {
  produtoId: number;
  proponente: ProponenteDto;
  veiculo: VeiculoDto;
  coberturas: CotacaoCoberturaInputDto[];
}

export interface AplicarDescontoRequestDto {
  cotacaoId: number;
  percentual: number;
}

export interface CotacaoCoberturaDto {
  coberturaId: number;
  importanciaSegurada: number;
  premioCobertura: number;
  franquiaSelecionada?: string | null;
  contratada: boolean;
}

export interface CotacaoResponseDto {
  id: number;
  numero: string;
  produtoId: number;
  status: StatusCotacao;
  dtCriacao: string;
  dtValidade?: string | null;
  premioLiquido: number;
  descontoComercial: number;
  premioComercial: number;
  comissao: number;
  premioTotal: number;
  proponente?: ProponenteDto | null;
  veiculo?: VeiculoDto | null;
  coberturas: CotacaoCoberturaDto[];
}
