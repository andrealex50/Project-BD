DROP FUNCTION IF EXISTS projeto.fn_CalculateGameRating;
DROP FUNCTION IF EXISTS projeto.fn_IsFriend;
DROP FUNCTION IF EXISTS projeto.fn_GetUserStats;
GO

-- UDF para Calcular Rating Médio de um Jogo
CREATE FUNCTION projeto.fn_CalculateGameRating
(
    @gameId VARCHAR(20)
)
RETURNS INT
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

-- UDF para estatisticas do utilizador
CREATE FUNCTION projeto.fn_GetUserStats
(
    @userId VARCHAR(20)
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        COUNT(DISTINCT r.id_jogo) as games_reviewed,
        COUNT(DISTINCT l.id_lista) as lists_created,
        COUNT(DISTINCT s1.id_utilizador_seguido) as following_count,
        COUNT(DISTINCT s2.id_utilizador_seguidor) as followers_count
    FROM projeto.utilizador u
    LEFT JOIN projeto.review r ON u.id_utilizador = r.id_utilizador
    LEFT JOIN projeto.lista l ON u.id_utilizador = l.id_utilizador
    LEFT JOIN projeto.segue s1 ON u.id_utilizador = s1.id_utilizador_seguidor
    LEFT JOIN projeto.segue s2 ON u.id_utilizador = s2.id_utilizador_seguido
    WHERE u.id_utilizador = @userId
);