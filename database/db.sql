---  Database postgres : gamerecords - sauvegarde du jeu 

CREATE DATABASE gamerecords;

\c gamerecords;

-- Table pour stocker les informations des joueurs
CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table pour stocker les informations des jeux - partie
CREATE TABLE IF NOT EXISTS games (
    id SERIAL PRIMARY KEY,
    point_list jsonb NOT NULL, -- Stockage de la liste des points en format JSON (adapter pour la fonction save qui utilise des fichier)  
    player1_id INT REFERENCES players(id) ON DELETE CASCADE,
    player2_id INT REFERENCES players(id) ON DELETE CASCADE,
    first_player_id INT REFERENCES players(id) ON DELETE SET NULL, -- pour savoir qui a commencé la partie
    gameType VaRCHAR(50) IN ('NEW', 'LOAD') NOT NULL DEFAULT 'NEW',
    gamereferences INT REFERENCES games(id) ON DELETE SET NULL, -- in case of a loaded game, reference to the original game
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--- Table pour stocker les enregistrements de jeu
CREATE TABLE IF NOT EXISTS gamerecords (
    id SERIAL PRIMARY KEY,
    id_player INT REFERENCES players(id) ON DELETE CASCADE,
    is_winner BOOLEAN NOT NULL DEFAULT FALSE,
    id_game INT NOT NULL REFERENCES games(id) ON DELETE CASCADE,
    date_played TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

