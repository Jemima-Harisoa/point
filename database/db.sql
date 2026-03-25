---  Database postgres : gamerecords - sauvegarde du jeu 

CREATE DATABASE gamerecords;

\c gamerecords;

-- Table pour stocker les informations des joueurs
CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--- Table pour stocker les enregistrements de jeu
CREATE TABLE IF NOT EXISTS gamerecords (
    id SERIAL PRIMARY KEY,
    player_name VARCHAR(255) NOT NULL,
    score INTEGER NOT NULL,
    date_played TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);