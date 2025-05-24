-- Primary index for checking/loading existing reactions
CREATE INDEX IX_reage_a_user_review ON projeto.reage_a (id_utilizador, id_review)
INCLUDE (reacao_texto);

-- Reverse index for review-centric queries
CREATE INDEX IX_reage_a_review_user ON projeto.reage_a (id_review, id_utilizador);

-- Index for the review table (used in FK relationship)
CREATE INDEX IX_review_id ON projeto.review (id_review);

-- For quickly finding all lists by a user (used in sp_SearchLists)
CREATE INDEX IX_lista_user ON projeto.lista (id_utilizador);

-- For title searches (used in sp_SearchLists with LIKE)
CREATE INDEX IX_lista_title ON projeto.lista (titulo_lista);

-- For finding all entries in a specific list
CREATE INDEX IX_entrada_lista_list ON projeto.entrada_lista (id_lista);

-- For finding specific game entries across lists
CREATE INDEX IX_entrada_lista_game ON projeto.entrada_lista (id_jogo);

-- Composite index for common access patterns
CREATE INDEX IX_entrada_lista_list_game ON projeto.entrada_lista (id_lista, id_jogo);

-- For game searches and filters
CREATE INDEX IX_jogo_titulo ON projeto.jogo (titulo);
CREATE INDEX IX_jogo_rating ON projeto.jogo (rating_medio DESC);
CREATE INDEX IX_jogo_data_lancamento ON projeto.jogo (data_lancamento);

-- For user searches
CREATE INDEX IX_utilizador_nome ON projeto.utilizador (nome);
CREATE INDEX IX_utilizador_email ON projeto.utilizador (email);

-- For reviews and ratings
CREATE INDEX IX_review_jogo_rating ON projeto.review (id_jogo, rating);
CREATE INDEX IX_review_utilizador ON projeto.review (id_utilizador);
CREATE INDEX IX_review_data ON projeto.review (data_review DESC);

-- For following relationships
CREATE INDEX IX_segue_seguidor ON projeto.segue (id_utilizador_seguidor);
CREATE INDEX IX_segue_seguido ON projeto.segue (id_utilizador_seguido);

-- For game metadata
CREATE INDEX IX_genero_nome ON projeto.genero (nome, id_jogo);
CREATE INDEX IX_plataforma_sigla ON projeto.plataforma (sigla, id_jogo);
CREATE INDEX IX_desenvolvedor_nome ON projeto.desenvolvedor (nome, id_jogo);