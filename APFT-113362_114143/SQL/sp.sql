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
    @filterType NVARCHAR(20) = 'All' -- All, Friends, MadeByMods
AS
BEGIN
    IF @filterType = 'Friends'
    BEGIN
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
        WHERE s.id_utilizador_seguidor = @currentUserId 
        AND l.visibilidade_lista = 'Publica'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        ORDER BY l.titulo_lista;
    END
    ELSE IF @filterType = 'MadeByMods'
    BEGIN
        SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.nome = 'admin'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%')
        ORDER BY l.titulo_lista;
    END
    ELSE -- All
    BEGIN
        (SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        JOIN projeto.segue s ON u.id_utilizador = s.id_utilizador_seguido
        WHERE s.id_utilizador_seguidor = @currentUserId 
        AND l.visibilidade_lista = 'Publica'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%'))
        
        UNION
        
        (SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.nome = 'admin'
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%'))
        
        UNION
        
        (SELECT l.id_lista, l.titulo_lista, u.nome AS criador
        FROM projeto.lista l
        JOIN projeto.utilizador u ON l.id_utilizador = u.id_utilizador
        WHERE u.id_utilizador = @currentUserId
        AND (@searchText IS NULL OR l.titulo_lista LIKE '%' + @searchText + '%'))
        
        ORDER BY titulo_lista;
    END
END
GO
