-- Stored Procedure: sp_GetCotacaoCompleta
IF OBJECT_ID('dbo.sp_GetCotacaoCompleta', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetCotacaoCompleta;
GO
CREATE PROCEDURE dbo.sp_GetCotacaoCompleta
    @CotacaoId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- 1º result set: Cabeçalho
    SELECT c.Id, c.Numero, c.ProdutoId, c.Status, c.DtCriacao, c.DtValidade, c.PremioLiquido, c.DescontoComercial, c.PremioComercial, c.Comissao, c.PremioTotal
    FROM Cotacoes c WHERE c.Id = @CotacaoId;

    -- 2º: Proponente
    SELECT p.* FROM Proponentes p WHERE p.CotacaoId = @CotacaoId;

    -- 3º: Veículo
    SELECT v.* FROM Veiculos v WHERE v.CotacaoId = @CotacaoId;

    -- 4º: Coberturas
    SELECT cc.* FROM CotacaoCoberturas cc WHERE cc.CotacaoId = @CotacaoId;
END
GO