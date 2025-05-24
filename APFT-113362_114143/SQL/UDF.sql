DROP FUNCTION IF EXISTS projeto.fn_CalculateGameRating;
DROP FUNCTION IF EXISTS projeto.fn_IsFriend;
DROP FUNCTION IF EXISTS projeto.fn_GetUserStats;
DROP FUNCTION IF EXISTS projeto.fn_IsListOwner;
DROP FUNCTION IF EXISTS projeto.fn_GenerateEntryId;
DROP FUNCTION IF EXISTS projeto.fn_GenerateListId;
DROP FUNCTION IF EXISTS projeto.fn_CanUserReviewGame;
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
GO


---- LIST PAGE ----

-- UDF para verificar dono da lista
CREATE FUNCTION projeto.fn_IsListOwner
(
    @userId VARCHAR(20),
    @listId VARCHAR(20)
)
RETURNS BIT
AS
BEGIN
    DECLARE @isOwner BIT = 0;
    
    IF EXISTS (
        SELECT 1 
        FROM projeto.lista 
        WHERE id_lista = @listId 
        AND id_utilizador = @userId
    )
    BEGIN
        SET @isOwner = 1;
    END
    
    RETURN @isOwner;
END
GO

-- UDF para gerar IDs exclusivos
CREATE FUNCTION projeto.fn_GenerateEntryId
(
    @listId VARCHAR(20)
)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @count INT;
    DECLARE @newId VARCHAR(20);
    
    SELECT @count = COUNT(*) + 1
    FROM projeto.entrada_lista 
    WHERE id_lista = @listId;
    
    SET @newId = 'E' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    
    RETURN @newId;
END
GO


---- MAIN PAGE ----
-- UDF para criar IDs
CREATE FUNCTION projeto.fn_GenerateListId()
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @count INT;
    DECLARE @newId VARCHAR(20);
    
    SELECT @count = COUNT(*) + 1 FROM projeto.lista;
    SET @newId = 'L' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    
    WHILE EXISTS (SELECT 1 FROM projeto.lista WHERE id_lista = @newId)
    BEGIN
        SET @count = @count + 1;
        SET @newId = 'L' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    END
    
    RETURN @newId;
END
GO


---- MAIN PAGE ----
CREATE FUNCTION projeto.fn_CanUserReviewGame
(
    @userId VARCHAR(20),
    @gameId VARCHAR(20)
)
RETURNS BIT
AS
BEGIN
    DECLARE @canReview BIT = 1;
    
    IF EXISTS (
        SELECT 1 
        FROM projeto.review 
        WHERE id_utilizador = @userId 
        AND id_jogo = @gameId
    )
    BEGIN
        SET @canReview = 0;
    END
    
    RETURN @canReview;
END
GO