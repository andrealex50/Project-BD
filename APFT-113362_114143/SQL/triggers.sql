---- Auto-update Game Rating Trigger ----
CREATE TRIGGER tr_UpdateGameRating
ON projeto.review
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    -- Update rating for affected games
    UPDATE projeto.jogo 
    SET rating_medio = projeto.fn_CalculateGameRating(id_jogo)
    WHERE id_jogo IN (
        SELECT DISTINCT id_jogo FROM inserted
        UNION
        SELECT DISTINCT id_jogo FROM deleted
    );
END
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

