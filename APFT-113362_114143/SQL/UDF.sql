-- UDF para Calcular Rating Médio de um Jogo
CREATE FUNCTION projeto.fn_CalculateGameRating
(
    @gameId VARCHAR(20)
)
RETURNS DECIMAL(3,2)
AS
BEGIN
    DECLARE @avgRating DECIMAL(3,2);
    
    SELECT @avgRating = AVG(CAST(rating AS DECIMAL(3,2)))
    FROM projeto.review
    WHERE id_jogo = @gameId;
    
    RETURN ISNULL(@avgRating, 0);
END
GO

-- UDF para Verificar se Usuário é Amigo
CREATE FUNCTION projeto.fn_IsFriend
(
    @currentUserId VARCHAR(20),
    @targetUserId VARCHAR(20)
)
RETURNS BIT
AS
BEGIN
    DECLARE @isFriend BIT = 0;
    
    IF EXISTS (
        SELECT 1 
        FROM projeto.segue 
        WHERE id_utilizador_seguidor = @currentUserId 
        AND id_utilizador_seguido = @targetUserId
    )
    BEGIN
        SET @isFriend = 1;
    END
    
    RETURN @isFriend;
END
GO

