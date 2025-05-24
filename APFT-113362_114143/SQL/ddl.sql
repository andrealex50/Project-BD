
-- Eliminamos as tabelas na ordem inversa da sua criação
-- (primeiro as tabelas que dependem de outras, depois as independentes)
DROP TABLE IF EXISTS projeto.reage_a;
DROP TABLE IF EXISTS projeto.segue;
DROP TABLE IF EXISTS projeto.avaliado_por;
DROP TABLE IF EXISTS projeto.genero;
DROP TABLE IF EXISTS projeto.plataforma;
DROP TABLE IF EXISTS projeto.desenvolvedor;
DROP TABLE IF EXISTS projeto.entrada_lista;
DROP TABLE IF EXISTS projeto.lista;
DROP TABLE IF EXISTS projeto.review;
DROP TABLE IF EXISTS projeto.perfil;
DROP TABLE IF EXISTS projeto.utilizador;
DROP TABLE IF EXISTS projeto.jogo;

-- Finalmente, eliminamos o schema
DROP SCHEMA IF EXISTS projeto;



--Aqui começa o código DDL --------------------------------------------------------------------------------------------------------------

create schema projeto;

create table projeto.jogo(
	id_jogo varchar(20)			NOT NULL,
	titulo varchar(50)			NOT NULL,
	data_lancamento date,
	sinopse varchar(100),
	capa varchar(255),  
	rating_medio int			NOT NULL DEFAULT 0   CHECK(rating_medio >= 0 and rating_medio <=5),
	tempo_medio_gameplay int	NOT NULL CHECK(tempo_medio_gameplay >= 0),
	preco numeric(6,2)			NOT NULL DEFAULT 0.00,	
	CONSTRAINT PKJogo PRIMARY KEY (id_jogo),
	CONSTRAINT CKJogo UNIQUE (titulo)
);

create table projeto.utilizador(
	id_utilizador varchar(20)		NOT NULL,
	nome varchar(50)				NOT NULL,
	email varchar(100)				NOT NULL,
	[password] varchar(255)			NOT NULL,
	CONSTRAINT PKUtilizador PRIMARY KEY (id_utilizador),
	CONSTRAINT CKUtilizador1 UNIQUE (email),
	CONSTRAINT CKUtilizador2 UNIQUE (nome)
);

create table projeto.perfil(
	bio varchar(200),
	foto varchar(255), 
	utilizador varchar(20) NOT NULL
);

create table projeto.review(
	id_review varchar(20)				NOT NULL,
	horas_jogadas numeric(6,2),
	rating int							NOT NULL DEFAULT 0   CHECK(rating >= 0 and rating <= 5),
	descricao_review varchar(200),
	data_review date,
	id_utilizador varchar(20)			NOT NULL,
	id_jogo varchar(20)					NOT NULL,
	CONSTRAINT PKReview PRIMARY KEY (id_review)
);

create table projeto.lista(
	id_lista varchar(20)				NOT NULL,
	titulo_lista varchar(30)			NOT NULL,
	descricao_lista varchar(200),
	visibilidade_lista varchar(7)		NOT NULL DEFAULT 'Publica'   CHECK (visibilidade_lista in ('Publica', 'Privada')),
	id_utilizador varchar(20)			NOT NULL,
	CONSTRAINT PKLista PRIMARY KEY (id_lista),
	CONSTRAINT CKLista UNIQUE (titulo_lista)
);
-- Adicionar o atriubto posicoes para uma lista ter elementos com posicoes ou nao
ALTER TABLE projeto.lista
ADD usa_posicoes BIT NOT NULL DEFAULT 1; -- 1 = usa posicoes, 0 = nao usa posicoes

create table projeto.entrada_lista(
	id_item varchar(20)					NOT NULL,
	estado varchar(15)					NOT NULL CHECK (estado in ('Jogado', 'Não Jogado', 'Planeia Jogar', 'Desistiu')),
	posicao int,						
	notas_adicionais varchar(100),
	id_jogo varchar(20)					NOT NULL,
	id_lista varchar(20)				NOT NULL
);

create table projeto.desenvolvedor(
	id_dev varchar(20)					NOT NULL,
	nome varchar(50),					
	id_jogo varchar(20)					NOT NULL,
	CONSTRAINT PKDesenvolvedor PRIMARY KEY (id_dev)
);

create table projeto.plataforma(
	id_plataforma varchar(20)			NOT NULL,
	sigla varchar(10),
	id_jogo varchar(20)					NOT NULL,
	CONSTRAINT PKPlataforma PRIMARY KEY (id_plataforma)
);

create table projeto.genero(
	id_genero varchar(20)				NOT NULL,
	nome varchar(30),
	id_jogo varchar(20)					NOT NULL,
	CONSTRAINT PKGenero PRIMARY KEY (id_genero)
);

--Depois de estarem as entidades, fracas e fortes criadas vamos adicionar as FK e PK que dependem de chaves de outras tabelas

ALTER TABLE projeto.perfil ADD CONSTRAINT PKPerfil PRIMARY KEY (utilizador);
ALTER TABLE projeto.perfil ADD CONSTRAINT FKPerfil FOREIGN KEY (utilizador) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.review ADD CONSTRAINT FKReview1 FOREIGN KEY (id_utilizador) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.review ADD CONSTRAINT FKReview2 FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.lista ADD CONSTRAINT FKLista FOREIGN KEY (id_utilizador) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.entrada_lista ADD CONSTRAINT PKEntradaLista PRIMARY KEY (id_item, id_lista);
ALTER TABLE projeto.entrada_lista ADD CONSTRAINT FKEntradaLista1 FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.entrada_lista ADD CONSTRAINT FKEntradaLista2 FOREIGN KEY (id_lista) REFERENCES projeto.lista (id_lista) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.desenvolvedor ADD CONSTRAINT FKDesenvolvedor FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.plataforma ADD CONSTRAINT FKPlataforma FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE projeto.genero ADD CONSTRAINT FKGenero FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE;

--Agora podemos criar as relações em paz e adicionar logo as keys todas

create table projeto.avaliado_por(
	id_jogo varchar(20)				NOT NULL,
	id_utilizador varchar(20)		NOT NULL,
	CONSTRAINT PKAvaliadoPor PRIMARY KEY (id_jogo, id_utilizador),
	CONSTRAINT FKAvaliadoPor1 FOREIGN KEY (id_jogo) REFERENCES projeto.jogo (id_jogo) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT FKAvaliadoPor2 FOREIGN KEY (id_utilizador) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE
);

create table projeto.segue(
	id_utilizador_seguidor varchar(20)			NOT NULL,
	id_utilizador_seguido varchar(20)			NOT NULL,
	data_seguir date,
	CONSTRAINT PKSegue PRIMARY KEY (id_utilizador_seguidor, id_utilizador_seguido),
	CONSTRAINT FKSegue1 FOREIGN KEY (id_utilizador_seguidor) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT FKSegue2 FOREIGN KEY (id_utilizador_seguido) REFERENCES projeto.utilizador (id_utilizador) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CHECK (id_utilizador_seguidor <> id_utilizador_seguido)
);

create table projeto.reage_a(
	id_utilizador varchar(20)		NOT NULL,
	id_review varchar(20)			NOT NULL,
	id_reacao varchar(20),
	reacao_texto varchar(200),
	reacao_data date				NOT NULL,
	CONSTRAINT PKReageA PRIMARY KEY (id_utilizador, id_review),
	CONSTRAINT FKReageA1 FOREIGN KEY (id_utilizador) REFERENCES projeto.utilizador (id_utilizador) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT FKReageA2 FOREIGN KEY (id_review) REFERENCES projeto.review (id_review) ON DELETE NO ACTION ON UPDATE NO ACTION
);