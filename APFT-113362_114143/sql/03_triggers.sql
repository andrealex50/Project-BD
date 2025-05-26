DROP TRIGGER IF EXISTS projeto.tr_UpdateGameRating;
DROP TRIGGER IF EXISTS projeto.tr_PreventSelfFollow;
GO

---- Prevent Self-Following Trigger ----
CREATE TRIGGER tr_PreventSelfFollow
ON projeto.segue
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO projeto.segue (id_utilizador_seguidor, id_utilizador_seguido, data_seguir)
    SELECT id_utilizador_seguidor, id_utilizador_seguido, data_seguir
    FROM inserted
    WHERE id_utilizador_seguidor != id_utilizador_seguido;
    
    IF @@ROWCOUNT = 0
        RAISERROR('Users cannot follow themselves', 16, 1);
END
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