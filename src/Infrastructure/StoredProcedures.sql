-- Script de Stored Procedures do módulo de Cotação de Seguros Auto
-- Cria ou altera a stored procedure sp_GetCotacaoCompleta que retorna 4 result sets:
-- 1) Cabeçalho da cotação
-- 2) Proponente
-- 3) Veículo
-- 4) Coberturas contratadas com dados da cobertura

IF OBJECT_ID('dbo.sp_GetCotacaoCompleta', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetCotacaoCompleta;
GO

CREATE PROCEDURE dbo.sp_GetCotacaoCompleta
    @CotacaoId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1º Result Set: Cabeçalho
    SELECT c.Id, c.Numero, c.ProdutoId, c.Status, c.DtCriacao, c.DtValidade,
           c.PremioLiquido, c.DescontoComercial, c.PremioComercial, c.Comissao, c.PremioTotal
    FROM Cotacoes c
    WHERE c.Id = @CotacaoId;

    -- 2º Result Set: Proponente
    SELECT p.Id, p.CotacaoId, p.Nome, p.CpfCnpj, p.Genero, p.EstadoCivil, p.DtNascimento, p.CepResidencial
    FROM Proponentes p
    WHERE p.CotacaoId = @CotacaoId;

    -- 3º Result Set: Veículo
    SELECT v.Id, v.CotacaoId, v.Placa, v.Chassi, v.CodigoFipeOuVeiculo, v.AnoModelo, v.AnoFabricacao,
           v.CepPernoite, v.TipoUtilizacao, v.ZeroKm, v.Blindado, v.KitGas
    FROM Veiculos v
    WHERE v.CotacaoId = @CotacaoId;

    -- 4º Result Set: Coberturas
    SELECT cc.CotacaoId, cc.CoberturaId, cb.Codigo, cb.Descricao, cc.ImportanciaSegurada,
           cc.PremioCobertura, cc.FranquiaSelecionada, cc.Contratada
    FROM CotacaoCoberturas cc
    INNER JOIN Coberturas cb ON cc.CoberturaId = cb.Id
    WHERE cc.CotacaoId = @CotacaoId;
END
GO
