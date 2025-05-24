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
    -- MadeByMods filter: All lists from admin (regardless of visibility for admin)
    ELSE IF @filterType = 'MadeByMods'
    BEGIN
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.nome = 'admin'
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