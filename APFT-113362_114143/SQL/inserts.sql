/* ========== JOGOS ========== */
INSERT INTO projeto.jogo (id_jogo, titulo, data_lancamento, sinopse, capa, rating_medio, tempo_medio_gameplay, preco) VALUES
('J001', 'The Legend of Zelda: Breath of the Wild', '2017-03-03', 'Aventura épica num vasto mundo aberto.', 'zelda_botw.jpg', 5, 50, 59.99),
('J002', 'The Witcher 3: Wild Hunt', '2015-05-19', 'Caça a monstros num mundo sombrio e mágico.', 'witcher3.jpg', 5, 70, 39.99),
('J003', 'Elden Ring', '2022-02-25', 'Exploração e combate num mundo de fantasia.', 'elden_ring.jpg', 5, 60, 59.99),
('J004', 'Red Dead Redemption 2', '2018-10-26', 'A vida no velho oeste com realismo brutal.', 'rdr2.jpg', 5, 80, 59.99),
('J005', 'God of War', '2018-04-20', 'Kratos numa nova jornada pela mitologia nórdica.', 'god_of_war.jpg', 5, 30, 49.99),
('J006', 'Hollow Knight', '2017-02-24', 'Metroidvania desafiador num mundo subterrâneo.', NULL, 4, 25, 14.99),
('J007', 'Celeste', '2018-01-25', 'Plataformas intensas e uma história emocional.', 'celeste.jpg', 4, 10, 19.99),
('J008', 'Dark Souls III', '2016-03-24', 'Desafios brutais e combates lendários.', 'dark_souls_3.jpg', 5, 45, 49.99),
('J009', 'Minecraft', '2011-11-18', 'Criação e sobrevivência em blocos.', 'minecraft.jpg', 4, 100, 26.95),
('J010', 'Stardew Valley', '2016-02-26', 'Vida na quinta com muito charme e conteúdo.', 'stardew_valley.jpg', 5, 60, 13.99),
('J011', 'Animal Crossing: New Horizons', '2020-03-20', NULL, NULL, 4, 80, 59.99),
('J012', 'Grand Theft Auto V', '2013-09-17', 'Crime, caos e liberdade em Los Santos.', 'gta_v.jpg', 5, 35, 29.99),
('J013', 'Portal 2', NULL, 'Puzzles de física com humor brilhante.', 'portal_2.jpg', 5, 8, 9.99),
('J014', 'Doom Eternal', '2020-03-20', 'Combate frenético contra demónios.', 'doom_eternal.jpg', 4, 20, 39.99),
('J015', 'Hades', '2020-09-17', 'Escape do submundo num roguelike aclamado.', NULL, 5, 25, 24.99),
('J016', 'Among Us', '2018-06-15', 'Descobre o impostor na tripulação.', 'among_us.jpg', 3, 5, 4.99),
('J017', 'Valorant', '2020-06-02', 'FPS tático com poderes especiais.', 'valorant.jpg', 4, 40, 0.00),
('J018', 'League of Legends', '2009-10-27', 'MOBA competitivo com milhões de jogadores.', 'lol.jpg', 4, 100, 0.00),
('J019', 'Counter-Strike: Global Offensive', '2012-08-21', 'FPS competitivo e clássico.', 'csgo.jpg', 4, 80, 0.00),
('J020', 'Fortnite', '2017-07-21', NULL, 'fortnite.jpg', 4, 60, 0.00),
('J021', 'Cyberpunk 2077', '2020-12-10', 'Futuro distópico e escolhas complexas.', 'cyberpunk2077.jpg', 3, 40, 59.99),
('J022', 'Overwatch', '2016-05-24', 'Heróis em batalhas rápidas e táticas.', 'overwatch.jpg', 4, 30, 39.99),
('J023', 'Super Mario Odyssey', '2017-10-27', NULL, 'mario_odyssey.jpg', 5, 20, 59.99),
('J024', 'Splatoon 3', '2022-09-09', 'Combates de tinta com estilo.', NULL, 4, 15, 59.99),
('J025', 'Monster Hunter: World', '2018-01-26', 'Caça épica a monstros colossais.', NULL, 5, 50, 49.99);

SELECT * FROM projeto.jogo;  -- testar

/* ========== UTILIZADORES ========== */
INSERT INTO projeto.utilizador (id_utilizador, nome, email, [password]) VALUES
('U001', 'joaosilva', 'joao.silva@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'joaosilva001'), 2)),	
('U002', 'mariacosta', 'maria.costa@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'mariacosta002'), 2)),
('U003', 'carlosmendes', 'carlos.mendes@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'carlosmendes003'), 2)),
('U004', 'anaribeiro', 'ana.ribeiro@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'anaribeiro004'), 2)),
('U005', 'riagofer', 'tiago.fernandes@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'tiagofernandes005'), 2)),
('U006', 'ritago', 'rita.gomes@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'ritagomes006'), 2)),
('U007', 'miguelro', 'miguel.rocha@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'miguelrocha007'), 2)),
('U008', 'beatrizli', 'beatriz.lima@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'beatrizlima008'), 2)),
('U009', 'andrematos', 'andre.matos@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'andrematos009'), 2)),
('U010', 'admin', 'admin@example.com', CONVERT(varchar(255), HASHBYTES('SHA2_256', 'admin'), 2));

SELECT * FROM projeto.utilizador;  -- testar

/* ========== PERFIS ========== */
INSERT INTO projeto.perfil (bio, foto, utilizador) VALUES
('Gamer apaixonado por aventuras épicas e mundos abertos.', NULL, 'U001'),
('Entusiasta de RPGs e experiências imersivas.', NULL, 'U002'),
('Amante de jogos desafiadores e de superação.', NULL, 'U003'),
(NULL, NULL, 'U004'),
('Fã de ação intensa e mitologia nórdica.', NULL, 'U005'),
('Jogadora casual com uma queda por jogos indie e experimentais.', 'rita_gomes.jpg', 'U006'),
(NULL, 'miguel_rocha.jpg', 'U007'),
('Curiosa por jogos inovadores e mecânicas únicas.', NULL, 'U008'),
('Apreciador de clássicos e novidades no mundo dos games.', NULL, 'U009'),
('Colecionadora de conquistas e troféus raros.', 'ines_marques.jpg', 'U010');

SELECT * FROM projeto.perfil;  -- testar

/* ========== LISTAS ========== */
INSERT INTO projeto.lista (id_lista, titulo_lista, descricao_lista, visibilidade_lista, id_utilizador) VALUES
('L001', 'Favoritos do João', 'Jogos que mais gostei.', 'Publica', 'U001'),
('L002', 'De olho nas promoções', NULL, 'Privada', 'U001'),
('L003', 'Para Jogar em 2025', 'Lista de jogos planejados.', 'Privada', 'U002'),
('L004', 'Top RPGs', 'Meus RPGs preferidos.', 'Publica', 'U003'),
('L005', 'Deixei de jogar', NULL, 'Publica', 'U006'),
('L006', 'Top da Semana', 'O jogo em Top esta semana', 'Publica', 'U010'),			-- criada pelo admin
('L007', 'Top 3 RPGs', 'A nossa equipa considera este como o top 3 do gênero RPG', 'Publica', 'U010');     -- criada pelo admin

SELECT * FROM projeto.lista;  -- testar

/* ========== ENTRADAS DE LISTA ========== */
INSERT INTO projeto.entrada_lista (id_item, estado, posicao, notas_adicionais, id_jogo, id_lista) VALUES
('E001', 'Jogado', 1, 'Jogo incrível! Mundo aberto bem construído, mecânicas de exploração sensacionais.', 'J001', 'L001'),
('E002', 'Jogado', 2, 'MUITO BOM! A história é incrível, curva de aprendizado pode ser difícil no início.', 'J002', 'L001'),
('E003', 'Jogado', 3, 'Fantástico! Visual deslumbrante e liberdade que poucos jogos conseguem oferecer.', 'J003', 'L001'),
('E004', 'Não Jogado', NULL, NULL, 'J001', 'L002'),
('E005', 'Não Jogado', NULL, NULL, 'J002', 'L002'),
('E006', 'Não Jogado', NULL, NULL, 'J003', 'L002'),
('E007', 'Planeia Jogar', NULL, 'Ouvi falar muito bem desse jogo, talvez depois de terminar outros da lista.', 'J001', 'L003'),
('E008', 'Planeia Jogar', NULL, 'Parece ser relaxante, perfeito para jogar em momentos mais tranquilos.', 'J011', 'L003'),
('E009', 'Planeia Jogar', NULL, 'Quero testar o multiplayer e ver as batalhas contra monstros no gênero.', 'J025', 'L003'),
('E010', 'Planeia Jogar', NULL, 'Já joguei em consoles antigos, mas a versão remasterizada tem gráficos e novos recursos.', 'J012', 'L003'),
('E011', 'Jogado', 1, 'Jogo completo, uma das melhores experiências de RPG. Finalizei todas as expansões.', 'J002', 'L004'),
('E012', 'Jogado', 2, 'Incrível! A história ficou ainda mais envolvente com as expansões. Valeu muito a pena.', 'J003', 'L004'),
('E013', 'Jogado', 3, 'Jogo bom, mas o desempenho no meu PC não é ideal. Alguns bugs, mas nada que tenha prejudicado.', 'J021', 'L004'),
('E014', 'Desistiu', NULL, 'Muito difícil para mim. Cheguei em um ponto onde não consegui passar.', 'J003', 'L005'),
('E015', 'Jogado', NULL, 'A espera pela DLC valeu muito a pena pois este jogo está no top desta semana!', 'J003', 'L006'),
('E016', 'Jogado', 1, 'A expansão foi muito bem-vinda, gostei bastante das novas missões e inimigos.', 'J002', 'L007'),
('E017', 'Jogado', 2, 'Um grande jogo, só não é top 1 pois o top 1 é o GOAT.', 'J021', 'L007'),
('E018', 'Jogado', 3, 'Gostei bastante, um bocado dificil para mim mas grandes ambientes', 'J003', 'L007');


SELECT * FROM projeto.entrada_lista;  -- testar

/* ========== REVIEWS ========== */
INSERT INTO projeto.review (id_review, horas_jogadas, rating, descricao_review, data_review, id_utilizador, id_jogo) VALUES
('R004', 85.5, 5, 'O melhor jogo de sempre, sem dúvida! A exploração é incrível e cada canto do mapa esconde segredos.', '2025-03-15', 'U004', 'J001'),
('R005', 120.0, 4, 'Jogo fantástico com uma história envolvente, mas alguns bugs técnicos retiraram um ponto.', '2025-02-28', 'U005', 'J002'),
('R006', 65.2, 5, 'FromSoftware superou-se outra vez. A dificuldade é recompensadora e o mundo é magnífico.', '2025-04-10', 'U006', 'J003'),
('R007', 45.0, 3, 'Boa premissa mas a execução ficou aquém. Esperava mais da CD Projekt após o sucesso de The Witcher.', '2025-01-05', 'U007', 'J021'),
('R008', 200.0, 5, 'Jogo perfeito para relaxar. Perdi-me literalmente centenas de horas na minha quinta virtual.', '2025-03-22', 'U008', 'J010'),
('R009', 15.5, 4, 'Plataforming excelente com uma narrativa emocionante. Só não dou 5 porque é demasiado curto.', '2025-04-05', 'U009', 'J007'),
('R010', 80.0, 5, 'Kratos está de volta e melhor do que nunca. A relação com o Atreus é o coração do jogo.', '2025-02-14', 'U010', 'J005'),
('R011', 30.0, 4, 'Muito divertido com amigos, mas a comunidade por vezes é tóxica.', '2025-03-08', 'U001', 'J016'),
('R012', 150.0, 5, 'O meu jogo de sempre. Nunca me canso de voltar a Los Santos.', '2025-01-30', 'U002', 'J012'),
('R013', 10.0, 3, 'Inovador mas demasiado curto. Vale pelo humor inteligente.', '2025-04-12', 'U003', 'J013'),
('R014', 55.0, 4, 'Combate frenético e satisfatório. Ideal para libertar o stress!', '2025-03-18', 'U004', 'J014'),
('R015', 40.0, 5, 'Roguelike perfeito. A progressão e a narrativa estão brilhantemente entrelaçadas.', '2025-02-25', 'U005', 'J015'),
('R016', 300.0, 5, 'Não há limite para a criatividade. Jogo há anos e nunca me aborrece.', '2025-04-01', 'U006', 'J009'),
('R017', 25.0, 4, 'Estética incrível e jogabilidade única. Um dos melhores metroidvanias.', '2025-03-05', 'U007', 'J006'),
('R018', 18.0, 3, 'Divertido mas a progressão torna-se repetitiva rapidamente.', '2025-02-10', 'U008', 'J020'),
('R019', 5.0, 2, 'Não é para mim. Muito caótico e difícil de acompanhar.', '2025-01-15', 'U009', 'J022'),
('R020', 12.0, 4, 'Surpreendentemente bom para um jogo gratuito. Ótimo para sessões curtas.', '2025-04-08', 'U010', 'J017');

/* ========== DESENVOLVEDORES ========== */
INSERT INTO projeto.desenvolvedor (id_dev, nome, id_jogo) VALUES
('D001', 'Nintendo', 'J001'),  -- The Legend of Zelda: Breath of the Wild
('D002', 'CD Projekt Red', 'J002'),  -- The Witcher 3: Wild Hunt
('D003', 'FromSoftware', 'J003'),  -- Elden Ring
('D004', 'Rockstar Games', 'J004'),  -- Red Dead Redemption 2
('D005', 'Santa Monica Studio', 'J005'),  -- God of War
('D006', 'Team Cherry', 'J006'),  -- Hollow Knight
('D007', 'Maddy Makes Games', 'J007'),  -- Celeste
('D008', 'FromSoftware', 'J008'),  -- Dark Souls III
('D009', 'Mojang Studios', 'J009'),  -- Minecraft
('D010', 'ConcernedApe', 'J010'),  -- Stardew Valley
('D011', 'Nintendo', 'J011'),  -- Animal Crossing: New Horizons
('D012', 'Rockstar Games', 'J012'),  -- Grand Theft Auto V
('D013', 'Valve', 'J013'),  -- Portal 2
('D014', 'id Software', 'J014'),  -- Doom Eternal
('D015', 'Supergiant Games', 'J015'),  -- Hades
('D016', 'InnerSloth', 'J016'),  -- Among Us
('D017', 'Riot Games', 'J017'),  -- Valorant
('D018', 'Riot Games', 'J018'),  -- League of Legends
('D019', 'Valve', 'J019'),  -- Counter-Strike: Global Offensive
('D020', 'Epic Games', 'J020'),  -- Fortnite
('D021', 'CD Projekt Red', 'J021'),  -- Cyberpunk 2077
('D022', 'Blizzard Entertainment', 'J022'),  -- Overwatch
('D023', 'Nintendo', 'J023'),  -- Super Mario Odyssey
('D024', 'Nintendo', 'J024'),  -- Splatoon 3
('D025', 'Capcom', 'J025');  -- Monster Hunter: World


/* ========== PLATAFORMAS ========== */
INSERT INTO projeto.plataforma (id_plataforma, sigla, id_jogo) VALUES
('P001', 'NS', 'J001'),  -- Nintendo Switch
('P002', 'NS', 'J011'),  -- Nintendo Switch
('P003', 'PS4', 'J001'),  -- PlayStation 4
('P004', 'PC', 'J002'),  -- PC
('P005', 'PS4', 'J002'),  -- PlayStation 4
('P006', 'PC', 'J003'),  -- PC
('P007', 'PS4', 'J003'),  -- PlayStation 4
('P008', 'PS5', 'J003'),  -- PlayStation 5
('P009', 'PS4', 'J004'),  -- PlayStation 4
('P010', 'PS5', 'J004'),  -- PlayStation 5
('P011', 'PC', 'J004'),  -- PC
('P012', 'PS4', 'J005'),  -- PlayStation 4
('P013', 'PS5', 'J005'),  -- PlayStation 5
('P014', 'PC', 'J005'),  -- PC
('P015', 'PC', 'J006'),  -- PC
('P016', 'NS', 'J006'),  -- Nintendo Switch
('P017', 'PC', 'J007'),  -- PC
('P018', 'NS', 'J007'),  -- Nintendo Switch
('P019', 'PS4', 'J008'),  -- PlayStation 4
('P020', 'PS5', 'J008'),  -- PlayStation 5
('P021', 'PC', 'J008'),  -- PC
('P022', 'PC', 'J009'),  -- PC
('P023', 'PS4', 'J009'),  -- PlayStation 4
('P024', 'NS', 'J010'),  -- Nintendo Switch
('P025', 'PC', 'J010'),  -- PC
('P026', 'PS4', 'J010'),  -- PlayStation 4
('P027', 'PS5', 'J012'),  -- PlayStation 5
('P028', 'PC', 'J012'),  -- PC
('P029', 'PS4', 'J013'),  -- PlayStation 4
('P030', 'PC', 'J013'),  -- PC
('P031', 'PS5', 'J014'),  -- PlayStation 5
('P032', 'PC', 'J014'),  -- PC
('P033', 'NS', 'J015'),  -- Nintendo Switch
('P034', 'PC', 'J015'),  -- PC
('P035', 'PS4', 'J016'),  -- PlayStation 4
('P036', 'PC', 'J016'),  -- PC
('P037', 'PC', 'J017'),  -- PC
('P038', 'PS4', 'J017'),  -- PlayStation 4
('P039', 'PS5', 'J018'),  -- PlayStation 5
('P040', 'PC', 'J018'),  -- PC
('P041', 'PC', 'J019'),  -- PC
('P042', 'PS4', 'J019'),  -- PlayStation 4
('P043', 'PS5', 'J020'),  -- PlayStation 5
('P044', 'PC', 'J020'),  -- PC
('P045', 'NS', 'J020'),  -- Nintendo Switch
('P046', 'PC', 'J021'),  -- PC
('P047', 'PS4', 'J021'),  -- PlayStation 4
('P048', 'PS5', 'J022'),  -- PlayStation 5
('P049', 'PC', 'J022'),  -- PC
('P050', 'NS', 'J023'),  -- Nintendo Switch
('P051', 'NS', 'J024'),  -- Nintendo Switch
('P052', 'PS4', 'J025'),  -- PlayStation 4
('P053', 'PC', 'J025');  -- PC

/* ========== GÉNEROS ========== */
INSERT INTO projeto.genero (id_genero, nome, id_jogo) VALUES
('G001', 'Aventura', 'J001'),  -- The Legend of Zelda: Breath of the Wild
('G002', 'Aventura', 'J011'),  -- Animal Crossing: New Horizons
('G003', 'RPG', 'J002'),  -- The Witcher 3: Wild Hunt
('G004', 'RPG', 'J003'),  -- Elden Ring
('G005', 'Ação', 'J003'),  -- Elden Ring
('G006', 'Aventura', 'J004'),  -- Red Dead Redemption 2
('G007', 'Ação', 'J004'),  -- Red Dead Redemption 2
('G008', 'Ação', 'J005'),  -- God of War
('G009', 'Aventura', 'J005'),  -- God of War
('G010', 'Metroidvania', 'J006'),  -- Hollow Knight
('G011', 'Plataforma', 'J007'),  -- Celeste
('G012', 'Ação', 'J008'),  -- Dark Souls III
('G013', 'Ação', 'J009'),  -- Minecraft
('G014', 'Simulação', 'J010'),  -- Stardew Valley
('G015', 'RPG', 'J012'),  -- Grand Theft Auto V
('G016', 'Puzzle', 'J013'),  -- Portal 2
('G017', 'Ação', 'J014'),  -- Doom Eternal
('G018', 'RPG', 'J015'),  -- Hades
('G019', 'Ação', 'J015'),  -- Hades
('G020', 'Social Deduction', 'J016'),  -- Among Us
('G021', 'Tático', 'J017'),  -- Valorant
('G022', 'MOBA', 'J018'),  -- League of Legends
('G023', 'Ação', 'J019'),  -- Counter-Strike: Global Offensive
('G024', 'Battle Royale', 'J020'),  -- Fortnite
('G025', 'RPG', 'J021'),  -- Cyberpunk 2077
('G026', 'FPS', 'J022'),  -- Overwatch
('G027', 'Ação', 'J023'),  -- Super Mario Odyssey
('G028', 'Aventura', 'J024'),  -- Splatoon 3
('G029', 'Ação', 'J025');  -- Monster Hunter: World

/* ========== AVALIAÇÕES ========== */
INSERT INTO projeto.avaliado_por (id_jogo, id_utilizador) VALUES
('J001', 'U001'),  
('J002', 'U002'),  
('J003', 'U003'),  
('J001', 'U004'),  
('J002', 'U005'),  
('J003', 'U006'),  
('J021', 'U007'),  
('J010', 'U008'),  
('J007', 'U009'), 
('J005', 'U010'),  
('J016', 'U001'), 
('J012', 'U002'),  
('J013', 'U003'),  
('J014', 'U004'), 
('J015', 'U005'), 
('J009', 'U006'), 
('J006', 'U007'),
('J020', 'U008'),  
('J022', 'U009'),  
('J017', 'U010');  

/* ========== SEGUIDORES ========== */
INSERT INTO projeto.segue (id_utilizador_seguidor, id_utilizador_seguido, data_seguir) VALUES
('U001', 'U002', '2025-04-20'),
('U002', 'U003', '2025-04-21'),
('U003', 'U001', '2025-04-22');