DROP TRIGGER IF EXISTS projeto.tr_UpdateGameRating;
DROP TRIGGER IF EXISTS projeto.tr_VerificarMaximoJogosPorLista;
GO

---- MAIN PAGE ----
-- Trigger para atualizar rating dos jogos quando a review muda
CREATE TRIGGER projeto.tr_UpdateGameRating
ON projeto.review
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @affectedGames TABLE (id_jogo VARCHAR(20));
    
    INSERT INTO @affectedGames (id_jogo)
    SELECT DISTINCT id_jogo FROM inserted
    UNION
    SELECT DISTINCT id_jogo FROM deleted;
    
    UPDATE projeto.jogo 
    SET rating_medio = (
        SELECT ISNULL(AVG(CAST(rating AS DECIMAL(3,2))), 0)
        FROM projeto.review r
        WHERE r.id_jogo = projeto.jogo.id_jogo
    )
    WHERE id_jogo IN (SELECT id_jogo FROM @affectedGames);
END
GO


-- Trigger para verificar número máximo de jogos por lista (máximo: 20)
CREATE TRIGGER projeto.tr_VerificarMaximoJogosPorLista
ON projeto.entrada_lista
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @count_jogos INT;
    DECLARE @id_lista VARCHAR(20);
    DECLARE @MAX_JOGOS_POR_LISTA INT = 20;
    
    -- Verificar cada lista afetada pela inserção
	-- CURSOR Funciona como um while com FETCH NEXT
    DECLARE lista_cursor CURSOR FOR
    SELECT DISTINCT id_lista FROM inserted;
    
    OPEN lista_cursor;
    FETCH NEXT FROM lista_cursor INTO @id_lista;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Contar quantos jogos existem na lista após a inserção
        SELECT @count_jogos = COUNT(*)
        FROM projeto.entrada_lista
        WHERE id_lista = @id_lista;
        
        -- Se exceder o limite, fazer rollback
        IF @count_jogos > @MAX_JOGOS_POR_LISTA
        BEGIN
            CLOSE lista_cursor;
            DEALLOCATE lista_cursor;
            
            RAISERROR('Erro: A lista já contém o número máximo de jogos permitidos (%d). Não é possível adicionar mais jogos.', 
                      16, 1, @MAX_JOGOS_POR_LISTA);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        FETCH NEXT FROM lista_cursor INTO @id_lista;
    END
    
    CLOSE lista_cursor;
    DEALLOCATE lista_cursor;
END;