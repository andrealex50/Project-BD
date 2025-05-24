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