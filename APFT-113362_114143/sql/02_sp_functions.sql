DROP PROCEDURE IF EXISTS projeto.sp_SearchGames;
DROP PROCEDURE IF EXISTS projeto.sp_SearchLists;
DROP PROCEDURE IF EXISTS projeto.sp_ManageReaction;
DROP PROCEDURE IF EXISTS projeto.sp_GetReaction;
DROP PROCEDURE IF EXISTS projeto.sp_CreateList;
DROP PROCEDURE IF EXISTS projeto.sp_UpdateUserProfile;
DROP PROCEDURE IF EXISTS projeto.sp_SearchUsers;
DROP PROCEDURE IF EXISTS projeto.sp_GetGameStats;
DROP PROCEDURE IF EXISTS projeto.sp_AddGameToList;
DROP PROCEDURE IF EXISTS projeto.sp_GetListOwner;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserStatistics;
DROP PROCEDURE IF EXISTS projeto.sp_GetGameDetails;
DROP PROCEDURE IF EXISTS projeto.sp_CheckUserGameReview;
DROP PROCEDURE IF EXISTS projeto.sp_GetGameWithRelatedData;
DROP PROCEDURE IF EXISTS projeto.sp_GetFilteredGameReviews;
DROP PROCEDURE IF EXISTS projeto.sp_CheckUserExists;
DROP PROCEDURE IF EXISTS projeto.sp_RegisterUser;
DROP PROCEDURE IF EXISTS projeto.sp_GetReviewDetails;
DROP PROCEDURE IF EXISTS projeto.sp_DeleteReview;
DROP PROCEDURE IF EXISTS projeto.sp_GetReviewReactions;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserProfile;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserReviews;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserLists;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserFollowing;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserReviewReactions;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserGameStats;
DROP PROCEDURE IF EXISTS projeto.sp_CreateReview;
DROP PROCEDURE IF EXISTS projeto.sp_UpdateReview;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserFriends;
DROP PROCEDURE IF EXISTS projeto.sp_SearchUserFriends;
DROP PROCEDURE IF EXISTS projeto.sp_GetListDetails;
DROP PROCEDURE IF EXISTS projeto.sp_GetUserBasicInfo;
DROP PROCEDURE IF EXISTS projeto.sp_GetListEntries;

-- UDF
DROP FUNCTION IF EXISTS projeto.fn_CalculateGameRating;
DROP FUNCTION IF EXISTS projeto.fn_IsFriend;
DROP FUNCTION IF EXISTS projeto.fn_GetUserStats;
DROP FUNCTION IF EXISTS projeto.fn_IsListOwner;
DROP FUNCTION IF EXISTS projeto.fn_GenerateEntryId;
DROP FUNCTION IF EXISTS projeto.fn_GenerateListId;
DROP FUNCTION IF EXISTS projeto.fn_CanUserReviewGame;
DROP FUNCTION IF EXISTS projeto.fn_GenerateUserId;
DROP FUNCTION IF EXISTS projeto.fn_IsReviewOwner;
DROP FUNCTION IF EXISTS projeto.fn_GenerateReviewId;
GO


-- SP para Pesquisa de Jogos com Filtros
CREATE PROCEDURE projeto.sp_SearchGames
    @searchText NVARCHAR(100) = NULL,
    @genre NVARCHAR(50) = NULL,
    @platform NVARCHAR(50) = NULL,
    @minRating INT = 0
AS
BEGIN
    SELECT DISTINCT j.id_jogo, j.titulo, j.capa, j.rating_medio 
    FROM projeto.jogo j
    LEFT JOIN projeto.genero g ON j.id_jogo = g.id_jogo
    LEFT JOIN projeto.plataforma p ON j.id_jogo = p.id_jogo
    WHERE (@searchText IS NULL OR j.titulo LIKE '%' + @searchText + '%')
    AND (@genre IS NULL OR EXISTS (
        SELECT 1 FROM projeto.genero g2 
        WHERE g2.id_jogo = j.id_jogo AND g2.nome = @genre
    ))
    AND (@platform IS NULL OR EXISTS (
        SELECT 1 FROM projeto.plataforma p2 
        WHERE p2.id_jogo = j.id_jogo AND p2.sigla = @platform
    ))
    AND j.rating_medio >= @minRating
    ORDER BY j.titulo;
END
GO

-- SP para Pesquisa de Listas com Filtros Combinados

CREATE PROCEDURE projeto.sp_SearchLists
    @currentUserId VARCHAR(20),
    @searchText NVARCHAR(100) = NULL,
    @filterType NVARCHAR(20) = 'All' -- All, Friends, Mine, MadeByMods
AS
BEGIN
    -- Friends filter: Only public lists from users I follow
    IF @filterType = 'Friends'
    BEGIN
        SELECT DISTINCT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
        WHERE s.id_utilizador_seguidor = @currentUserId 
        AND l.visibilidade_lista = 'Publica'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        ORDER BY l.titulo_lista;
    END
    -- MadeByMods filter: All lists from admin 
    ELSE IF @filterType = 'MadeByMods'
    BEGIN
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.nome = 'adminmod'
		AND l.visibilidade_lista = 'Publica'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        ORDER BY l.titulo_lista;
    END
    -- Mine filter: Only my lists (both public and private)
    ELSE IF @filterType = 'Mine'
    BEGIN
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.id_utilizador = @currentUserId
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        ORDER BY l.titulo_lista;
    END
    -- Default All filter: All public lists + my private lists
    ELSE -- All
    BEGIN
        -- Public lists from all users
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE l.visibilidade_lista = 'Publica'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        
        UNION
        
        -- My private lists
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.id_utilizador = @currentUserId
        AND l.visibilidade_lista = 'Privada'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        
        ORDER BY titulo_lista;
    END
END
GO

-- SP para dar manage as reacoes
CREATE PROCEDURE projeto.sp_ManageReaction
    @userId VARCHAR(20),
    @reviewId VARCHAR(20),
    @reactionText VARCHAR(200)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM projeto.reage_a 
              WHERE id_utilizador = @userId AND id_review = @reviewId)
    BEGIN
        UPDATE projeto.reage_a 
        SET reacao_texto = @reactionText, reacao_data = GETDATE()
        WHERE id_utilizador = @userId AND id_review = @reviewId;
    END
    ELSE
    BEGIN
        INSERT INTO projeto.reage_a 
        (id_utilizador, id_review, reacao_texto, reacao_data)
        VALUES (@userId, @reviewId, @reactionText, GETDATE());
    END
END
GO

--SP para recolher reacoes
CREATE PROCEDURE projeto.sp_GetReaction
    @userId VARCHAR(20),
    @reviewId VARCHAR(20)
AS
BEGIN
    SELECT reacao_texto 
    FROM projeto.reage_a
    WHERE id_utilizador = @userId AND id_review = @reviewId;
END
GO

--SP para criar listas
CREATE PROCEDURE projeto.sp_CreateList
    @listId VARCHAR(20),
    @title VARCHAR(30),
    @description VARCHAR(200) = NULL,
    @visibility VARCHAR(7) = 'Publica',
    @userId VARCHAR(20),
    @usePositions BIT = 1
AS
BEGIN
    INSERT INTO projeto.lista 
    (id_lista, titulo_lista, descricao_lista, visibilidade_lista, id_utilizador, usa_posicoes)
    VALUES 
    (@listId, @title, @description, @visibility, @userId, @usePositions);
    
    RETURN SCOPE_IDENTITY();
END
GO

--SP para atualizar o profile
CREATE PROCEDURE projeto.sp_UpdateUserProfile
    @userId VARCHAR(20),
    @name VARCHAR(50),
    @bio VARCHAR(200) = NULL
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Update user name
        UPDATE projeto.utilizador 
        SET nome = @name 
        WHERE id_utilizador = @userId;
        
        -- Update or insert bio
        IF EXISTS (SELECT 1 FROM projeto.perfil WHERE utilizador = @userId)
            UPDATE projeto.perfil SET bio = @bio WHERE utilizador = @userId;
        ELSE
            INSERT INTO projeto.perfil (bio, utilizador) VALUES (@bio, @userId);
            
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP para pesquisar pelo utilizador
CREATE PROCEDURE projeto.sp_SearchUsers
    @searchText NVARCHAR(100) = NULL,
    @currentUserId VARCHAR(20),
    @excludeFollowed BIT = 0
AS
BEGIN
    SELECT u.id_utilizador, u.nome, p.foto,
           CASE WHEN s.id_utilizador_seguido IS NOT NULL THEN 1 ELSE 0 END AS is_following
    FROM projeto.utilizador u
    LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
    LEFT JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido 
                              AND s.id_utilizador_seguidor = @currentUserId
    WHERE u.id_utilizador != @currentUserId
    AND (@searchText IS NULL OR u.nome LIKE '%' + @searchText + '%')
    AND (@excludeFollowed = 0 OR s.id_utilizador_seguido IS NULL)
    ORDER BY u.nome;
END
GO

-- SP para estatisticas do jogo
CREATE PROCEDURE projeto.sp_GetGameStats
    @gameId VARCHAR(20)
AS
BEGIN
    SELECT 
        COUNT(r.id_review) as total_reviews,
        AVG(CAST(r.rating AS FLOAT)) as avg_rating,
        COUNT(DISTINCT r.id_utilizador) as unique_reviewers,
        AVG(r.horas_jogadas) as avg_hours_played
    FROM projeto.review r
    WHERE r.id_jogo = @gameId;
END
GO


---- LIST PAGE ----

-- SP para adicionar jogo a uma lista
CREATE PROCEDURE projeto.sp_AddGameToList
    @listId VARCHAR(20),
    @gameId VARCHAR(20),
    @userId VARCHAR(20),
    @status VARCHAR(15),
    @notes VARCHAR(100) = NULL,
    @position INT = NULL
AS
BEGIN
    IF projeto.fn_IsListOwner(@userId, @listId) = 0
    BEGIN
        RAISERROR('Only the list owner can add games to this list', 16, 1);
        RETURN;
    END
    
    IF EXISTS (SELECT 1 FROM projeto.entrada_lista WHERE id_lista = @listId AND id_jogo = @gameId)
    BEGIN
        RAISERROR('Game already exists in this list', 16, 1);
        RETURN;
    END
    
    DECLARE @entryId VARCHAR(20) = projeto.fn_GenerateEntryId(@listId);
    DECLARE @listUsesPositions BIT;
    
    SELECT @listUsesPositions = usa_posicoes FROM projeto.lista WHERE id_lista = @listId;

    IF @listUsesPositions = 1 AND @position IS NULL
    BEGIN
        SELECT @position = ISNULL(MAX(posicao), 0) + 1 
        FROM projeto.entrada_lista 
        WHERE id_lista = @listId;
    END
    
    INSERT INTO projeto.entrada_lista 
    (id_item, estado, posicao, notas_adicionais, id_jogo, id_lista)
    VALUES 
    (@entryId, @status, @position, @notes, @gameId, @listId);
END
GO

-- SP para obter informa��o sobre o dono da lista
CREATE PROCEDURE projeto.sp_GetListOwner
    @listId VARCHAR(20)
AS
BEGIN
    SELECT u.id_utilizador, u.nome
    FROM projeto.lista l
    JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
    WHERE l.id_lista = @listId;
END
GO


---- MAIN PAGE ----

-- SP para obter estatisticas do utilizador
CREATE PROCEDURE projeto.sp_GetUserStatistics
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        u.nome,
        u.email,
        p.bio,
        p.foto,
        stats.games_reviewed,
        stats.lists_created,
        stats.following_count,
        stats.followers_count
    FROM projeto.utilizador u
    LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
    CROSS APPLY projeto.fn_GetUserStats(@userId) stats
    WHERE u.id_utilizador = @userId;
END
GO

-- SP para obter detalhes do jogo com rating
CREATE PROCEDURE projeto.sp_GetGameDetails
    @gameId VARCHAR(20)
AS
BEGIN
    SELECT 
        j.id_jogo,
        j.titulo,
        j.data_lancamento,
        j.sinopse,
        j.capa,
        j.rating_medio,
        j.tempo_medio_gameplay,
        j.preco,
        stats.total_reviews,
        stats.avg_rating,
        stats.unique_reviewers,
        stats.avg_hours_played
    FROM projeto.jogo j
    CROSS APPLY (
        SELECT 
            COUNT(r.id_review) as total_reviews,
            AVG(CAST(r.rating AS FLOAT)) as avg_rating,
            COUNT(DISTINCT r.id_utilizador) as unique_reviewers,
            AVG(r.horas_jogadas) as avg_hours_played
        FROM projeto.review r
        WHERE r.id_jogo = @gameId
    ) stats
    WHERE j.id_jogo = @gameId;
END
GO

-- SP para verificar se o utilizador j� deu review ao jogo
CREATE PROCEDURE projeto.sp_CheckUserGameReview
    @userId VARCHAR(20),
    @gameId VARCHAR(20)
AS
BEGIN
    SELECT 
        r.id_review,
        r.rating,
        r.descricao_review,
        r.horas_jogadas,
        r.data_review
    FROM projeto.review r
    WHERE r.id_utilizador = @userId AND r.id_jogo = @gameId;
END
GO

-- SP para obter os utilizadores seguidos (amigos)
CREATE PROCEDURE projeto.sp_GetUserFriends
    @currentUserId VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.id_utilizador, 
        u.nome 
    FROM 
        projeto.utilizador u
    JOIN 
        projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
    WHERE 
        s.id_utilizador_seguidor = @currentUserId;
END
GO


-- SP para pesquisar por amigos
CREATE PROCEDURE projeto.sp_SearchUserFriends
    @currentUserId VARCHAR(20),
    @searchText NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.id_utilizador, 
        u.nome 
    FROM 
        projeto.utilizador u
    JOIN 
        projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
    WHERE 
        s.id_utilizador_seguidor = @currentUserId
        AND u.nome LIKE '%' + @searchText + '%';
END
GO


---- LIST ----
CREATE PROCEDURE projeto.sp_GetListDetails
    @listId VARCHAR(20),
    @currentUserId VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        l.descricao_lista, 
        l.visibilidade_lista, 
        l.usa_posicoes,
        CASE WHEN l.id_utilizador = @currentUserId THEN 1 ELSE 0 END AS is_owner
    FROM 
        projeto.lista l
    WHERE 
        l.id_lista = @listId;
END
GO

-- SP para informa��es basicas do User
CREATE PROCEDURE projeto.sp_GetUserBasicInfo
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        u.nome,
        p.foto
    FROM projeto.utilizador u
    LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
    WHERE u.id_utilizador = @userId;
END
GO

-- SP para obter entradas da lista
CREATE PROCEDURE projeto.sp_GetListEntries
    @listId VARCHAR(20),
    @usesPositions BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @usesPositions = 1
    BEGIN
        SELECT 
            el.id_item, 
            j.titulo as game_title, 
            j.id_jogo, 
            el.estado, 
            el.posicao, 
            el.notas_adicionais, 
            j.capa
        FROM 
            projeto.entrada_lista el
        JOIN 
            projeto.jogo j ON el.id_jogo = j.id_jogo
        WHERE 
            el.id_lista = @listId
        ORDER BY 
            el.posicao;
    END
    ELSE
    BEGIN
        SELECT 
            el.id_item, 
            j.titulo as game_title, 
            j.id_jogo, 
            el.estado, 
            el.posicao, 
            el.notas_adicionais, 
            j.capa
        FROM 
            projeto.entrada_lista el
        JOIN 
            projeto.jogo j ON el.id_jogo = j.id_jogo
        WHERE 
            el.id_lista = @listId
        ORDER BY 
            j.titulo;
    END
END
GO

---- GAME PAGE ----

CREATE PROCEDURE projeto.sp_GetGameWithRelatedData
    @gameId VARCHAR(20)
AS
BEGIN
    -- Game details
    SELECT 
        j.id_jogo,
        j.titulo,
        j.data_lancamento,
        j.sinopse,
        j.capa,
        j.rating_medio,
        j.tempo_medio_gameplay,
        j.preco
    FROM projeto.jogo j
    WHERE j.id_jogo = @gameId;
    
    -- Genres
    SELECT g.nome 
    FROM projeto.genero g 
    WHERE g.id_jogo = @gameId;
    
    -- Developers
    SELECT d.nome 
    FROM projeto.desenvolvedor d 
    WHERE d.id_jogo = @gameId;
    
    -- Platforms
    SELECT p.sigla 
    FROM projeto.plataforma p 
    WHERE p.id_jogo = @gameId;
END
GO


CREATE PROCEDURE projeto.sp_GetFilteredGameReviews
    @gameId VARCHAR(20),
    @currentUserId VARCHAR(20),
    @filter VARCHAR(20) = 'All' -- All, Friends, MadeByMods
AS
BEGIN
    IF @filter = 'Friends'
    BEGIN
        SELECT r.id_review, r.descricao_review, u.nome, r.rating, r.data_review
        FROM projeto.review r
        JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
        WHERE r.id_jogo = @gameId
        AND projeto.fn_IsFriend(@currentUserId, r.id_utilizador) = 1
        ORDER BY r.data_review DESC;
    END
    ELSE IF @filter = 'MadeByMods'
    BEGIN
        SELECT r.id_review, r.descricao_review, u.nome, r.rating, r.data_review
        FROM projeto.review r
        JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
        WHERE r.id_jogo = @gameId
        AND u.nome = 'adminmod'
        ORDER BY r.data_review DESC;
    END
    ELSE -- All
    BEGIN
        SELECT r.id_review, r.descricao_review, u.nome, r.rating, r.data_review
        FROM projeto.review r
        JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
        WHERE r.id_jogo = @gameId
        ORDER BY r.data_review DESC;
    END
END
GO

--RegisterForm
--SP para verficar se um user ja existe
CREATE PROCEDURE projeto.sp_CheckUserExists
    @Name VARCHAR(50),
    @Email VARCHAR(100),
    @Exists BIT OUTPUT
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM projeto.utilizador 
        WHERE nome = @Name OR email = @Email
    )
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO

--SP para registar um novo user
CREATE PROCEDURE projeto.sp_RegisterUser
    @Id VARCHAR(20),
    @Name VARCHAR(50),
    @Email VARCHAR(100),
    @Password VARCHAR(255)
AS
BEGIN
    INSERT INTO projeto.utilizador (id_utilizador, nome, email, password)
    VALUES (@Id, @Name, @Email, @Password);
END
GO

--ReviewDetails
--SP para recolher review details
CREATE PROCEDURE projeto.sp_GetReviewDetails
    @reviewId VARCHAR(20)
AS
BEGIN
    SELECT j.titulo AS GameTitle, u.nome AS UserName, 
           r.rating, r.horas_jogadas AS HoursPlayed, 
           r.descricao_review AS ReviewText, r.data_review AS ReviewDate
    FROM projeto.review r
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
    WHERE r.id_review = @reviewId;
END
GO

--SP para elimiar uma review
CREATE PROCEDURE projeto.sp_DeleteReview
    @reviewId VARCHAR(20),
    @userId VARCHAR(20)
AS
BEGIN
    DECLARE @gameId VARCHAR(20);
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Get game ID first
        SELECT @gameId = id_jogo 
        FROM projeto.review 
        WHERE id_review = @reviewId AND id_utilizador = @userId;
        
        IF @gameId IS NULL
        BEGIN
            RAISERROR('Review not found or not owned by user', 16, 1);
            ROLLBACK;
            RETURN;
        END
        
        -- Delete reactions first (triggers will handle the rest)
        DELETE FROM projeto.reage_a WHERE id_review = @reviewId;
        
        -- Delete the review (this will trigger tr_UpdateGameRating)
        DELETE FROM projeto.review WHERE id_review = @reviewId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

--SP para recolher reacoes a reviews
CREATE PROCEDURE projeto.sp_GetReviewReactions
    @reviewId VARCHAR(20)
AS
BEGIN
    SELECT 
        u.nome AS UserName, 
        r.reacao_texto AS ReactionText,
        r.reacao_data AS ReactionDate
    FROM projeto.reage_a r
    JOIN projeto.utilizador u ON r.id_utilizador = u.id_utilizador
    WHERE r.id_review = @reviewId
    ORDER BY r.reacao_data DESC;
END
GO

--UserPage
--SP para recolher info do user
CREATE PROCEDURE projeto.sp_GetUserProfile
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        u.id_utilizador,
        u.nome,
        p.bio,
        p.foto,
        (SELECT COUNT(*) FROM projeto.segue WHERE id_utilizador_seguidor = @userId) AS following_count,
        (SELECT COUNT(*) FROM projeto.segue WHERE id_utilizador_seguido = @userId) AS followers_count,
        (SELECT COUNT(*) FROM projeto.review WHERE id_utilizador = @userId) AS reviews_count,
        (SELECT COUNT(*) FROM projeto.lista WHERE id_utilizador = @userId) AS lists_count
    FROM projeto.utilizador u
    LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
    WHERE u.id_utilizador = @userId;
END
GO

--SP para recolher user reviews
CREATE PROCEDURE projeto.sp_GetUserReviews
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        r.id_review, 
        j.titulo AS game_title, 
        r.rating, 
        r.descricao_review AS review_text,
        r.data_review AS review_date,
        r.horas_jogadas AS hours_played
    FROM projeto.review r
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    WHERE r.id_utilizador = @userId
    ORDER BY r.data_review DESC;
END
GO

-- SP para recolher listas com info basica
CREATE PROCEDURE projeto.sp_GetUserLists
    @userId VARCHAR(20),
    @currentUserId VARCHAR(20) = NULL
AS
BEGIN
    -- If current user is viewing their own profile, show all lists
    IF @currentUserId = @userId
    BEGIN
        SELECT 
            id_lista, 
            titulo_lista, 
            descricao_lista,
            visibilidade_lista,
            usa_posicoes
        FROM projeto.lista
        WHERE id_utilizador = @userId
        ORDER BY titulo_lista;
    END
    ELSE -- Otherwise only show public lists
    BEGIN
        SELECT 
            id_lista, 
            titulo_lista, 
            descricao_lista,
            visibilidade_lista,
            usa_posicoes
        FROM projeto.lista
        WHERE id_utilizador = @userId
        AND visibilidade_lista = 'Publica'
        ORDER BY titulo_lista;
    END
END
GO

-- Recolher amigos do user
CREATE PROCEDURE projeto.sp_GetUserFollowing
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        u.id_utilizador,
        u.nome,
        p.foto
    FROM projeto.segue s
    JOIN projeto.utilizador u ON s.id_utilizador_seguido = u.id_utilizador
    LEFT JOIN projeto.perfil p ON u.id_utilizador = p.utilizador
    WHERE s.id_utilizador_seguidor = @userId
    ORDER BY u.nome;
END
GO

-- Recolher reacoes a reviews do user
CREATE PROCEDURE projeto.sp_GetUserReviewReactions
    @userId VARCHAR(20)
AS
BEGIN
    SELECT 
        r.id_review,
        j.titulo AS game_title,
        u.nome AS reactor_name,
        ra.reacao_texto AS reaction_text,
        ra.reacao_data AS reaction_date
    FROM projeto.review r
    JOIN projeto.reage_a ra ON r.id_review = ra.id_review
    JOIN projeto.utilizador u ON ra.id_utilizador = u.id_utilizador
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    WHERE r.id_utilizador = @userId
    ORDER BY ra.reacao_data DESC;
END
GO

-- Recolher statisticas do user

CREATE PROCEDURE projeto.sp_GetUserGameStats
    @userId VARCHAR(20)
AS
BEGIN
    -- Best reviewed game (with fallback for no reviews)
    SELECT TOP 1 
        ISNULL(j.titulo, 'No games reviewed') AS best_reviewed_game
    FROM projeto.review r
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    WHERE r.id_utilizador = @userId
    ORDER BY r.rating DESC;
    
    -- Most played game (with fallback for no playtime data)
    SELECT TOP 1 
        ISNULL(j.titulo, 'No playtime data') AS most_played_game
    FROM projeto.review r
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    WHERE r.id_utilizador = @userId AND r.horas_jogadas > 0
    ORDER BY r.horas_jogadas DESC;
    
    -- Most reviewed genre (with fallback for no genres)
    SELECT TOP 1 
        ISNULL(g.nome, 'No genres reviewed') AS most_reviewed_genre
    FROM projeto.review r
    JOIN projeto.jogo j ON r.id_jogo = j.id_jogo
    JOIN projeto.genero g ON j.id_jogo = g.id_jogo
    WHERE r.id_utilizador = @userId
    GROUP BY g.nome
    ORDER BY COUNT(*) DESC;
    
    -- Average review score (with NULL handling)
    SELECT 
        CASE 
            WHEN COUNT(*) > 0 THEN AVG(CAST(rating AS FLOAT))
            ELSE NULL 
        END AS avg_rating
    FROM projeto.review
    WHERE id_utilizador = @userId;
END
GO

--ReviewPage
--SP para criar reviews
CREATE PROCEDURE projeto.sp_CreateReview
    @reviewId VARCHAR(20),
    @hours DECIMAL(6,2),
    @rating INT,
    @review VARCHAR(200),
    @userId VARCHAR(20),
    @gameId VARCHAR(20)
AS
BEGIN
    INSERT INTO projeto.review 
    (id_review, horas_jogadas, rating, descricao_review, 
     data_review, id_utilizador, id_jogo)
    VALUES 
    (@reviewId, @hours, @rating, @review, GETDATE(), @userId, @gameId);
END
GO

--SP para dar update a reviews
CREATE PROCEDURE projeto.sp_UpdateReview
    @reviewId VARCHAR(20),
    @hours DECIMAL(6,2),
    @rating INT,
    @review VARCHAR(200),
    @userId VARCHAR(20),
    @gameId VARCHAR(20)
AS
BEGIN
    UPDATE projeto.review 
    SET horas_jogadas = @hours, 
        rating = @rating, 
        descricao_review = @review, 
        data_review = GETDATE()
    WHERE id_review = @reviewId 
    AND id_utilizador = @userId 
    AND id_jogo = @gameId;
END
GO


------------------- UDF -----------------------
-- UDF para Calcular Rating M�dio de um Jogo
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

-- UDF para Verificar se Usu�rio � Amigo
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

--RegisterForm
--UDF para criar um id exclusivo para um user
CREATE FUNCTION projeto.fn_GenerateUserId()
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @count INT;
    DECLARE @newId VARCHAR(20);
    
    SELECT @count = COUNT(*) + 1 FROM projeto.utilizador;
    SET @newId = 'U' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    
    WHILE EXISTS (SELECT 1 FROM projeto.utilizador WHERE id_utilizador = @newId)
    BEGIN
        SET @count = @count + 1;
        SET @newId = 'U' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    END
    
    RETURN @newId;
END
GO

--ReviewDetails
--UDF para verificar se um user e o owner de uma review
CREATE FUNCTION projeto.fn_IsReviewOwner
(
    @userId VARCHAR(20),
    @reviewId VARCHAR(20)
)
RETURNS BIT
AS
BEGIN
    DECLARE @isOwner BIT = 0;
    
    IF EXISTS (
        SELECT 1 
        FROM projeto.review 
        WHERE id_review = @reviewId 
        AND id_utilizador = @userId
    )
    BEGIN
        SET @isOwner = 1;
    END
    
    RETURN @isOwner;
END
GO

--ReviewPage
--UDF para gerar um id para uma review
CREATE FUNCTION projeto.fn_GenerateReviewId()
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @count INT;
    DECLARE @newId VARCHAR(20);
    
    SELECT @count = COUNT(*) + 1 FROM projeto.review;
    SET @newId = 'R' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    
    WHILE EXISTS (SELECT 1 FROM projeto.review WHERE id_review = @newId)
    BEGIN
        SET @count = @count + 1;
        SET @newId = 'R' + RIGHT('000' + CAST(@count AS VARCHAR(3)), 3);
    END
    
    RETURN @newId;
END
GO