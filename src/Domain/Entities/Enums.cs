namespace Domain.Entities;

public enum StatusCotacao
{
    Rascunho = 0,
    Calculada = 1,
    Aprovada = 2,
    Expirada = 3,
    Cancelada = 4
}

public enum TipoCobertura
{
    Basica = 0,
    Adicional = 1,
    Servico = 2
}

public enum Genero
{
    M = 0,
    F = 1
}

public enum TipoUtilizacao
{
    Particular = 0,
    Profissional = 1,
    Aplicativo = 2
}

public enum FaixaIdade
{
    Idade18_25 = 0,
    Idade26_35 = 1,
    Idade36_60 = 2,
    Idade60Mais = 3
}