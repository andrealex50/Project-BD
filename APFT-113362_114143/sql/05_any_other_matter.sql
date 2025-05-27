------ INDEXES ------

DROP INDEX IF EXISTS IX_jogo_titulo_rating ON projeto.jogo;
DROP INDEX IF EXISTS IX_review_jogo_usuario_data ON projeto.review;
DROP INDEX IF EXISTS IX_lista_usuario_titulo_visibilidade ON projeto.lista;
DROP INDEX IF EXISTS IX_segue_relacionamento ON projeto.segue;

-- Esta SP faz buscas por t�tulo e filtros por rating. O �ndice composto acelera ambas as opera��es.
CREATE INDEX IX_jogo_titulo_rating ON projeto.jogo(titulo, rating_medio);

-- Mostrar reviews � cr�tico na p�gina de detalhes do jogo.
CREATE INDEX IX_review_jogo_usuario_data ON projeto.review(id_jogo, id_utilizador, data_review DESC);

-- Consultado frequentemente para mostrar listas no perfil, diferenciando visibilidade.
CREATE INDEX IX_lista_usuario_titulo_visibilidade ON projeto.lista(id_utilizador, titulo_lista, visibilidade_lista);

-- Esta fun��o � usada em m�ltiplas SPs para verificar rela��es entre usu�rios.
CREATE INDEX IX_segue_relacionamento ON projeto.segue(id_utilizador_seguidor, id_utilizador_seguido);