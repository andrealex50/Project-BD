-- Primary index for checking/loading existing reactions
CREATE INDEX IX_reage_a_user_review ON projeto.reage_a (id_utilizador, id_review)
INCLUDE (reacao_texto);

-- Reverse index for review-centric queries
CREATE INDEX IX_reage_a_review_user ON projeto.reage_a (id_review, id_utilizador);

-- Index for the review table (used in FK relationship)
CREATE INDEX IX_review_id ON projeto.review (id_review);