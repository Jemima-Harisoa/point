-- Schema PostgreSQL pour le jeu de point avec historisation
-- Ce schéma permet de conserver toutes les données, y compris les suppressions

-- Table des joueurs
CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table des parties
CREATE TABLE IF NOT EXISTS games (
    id SERIAL PRIMARY KEY,
    player1_id INTEGER REFERENCES players(id) ON DELETE CASCADE,
    player2_id INTEGER REFERENCES players(id) ON DELETE CASCADE,
    grid_rows INTEGER NOT NULL DEFAULT 18,
    grid_columns INTEGER NOT NULL DEFAULT 18,
    winner_id INTEGER REFERENCES players(id) ON DELETE SET NULL,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP,
    status VARCHAR(20) DEFAULT 'in_progress' -- 'in_progress', 'completed', 'abandoned'
);

-- Table des points avec historisation (soft delete)
-- Tous les points sont conservés, même ceux supprimés
CREATE TABLE IF NOT EXISTS game_points (
    id SERIAL PRIMARY KEY,
    game_id INTEGER REFERENCES games(id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES players(id) ON DELETE CASCADE,
    x_coordinate INTEGER NOT NULL,
    y_coordinate INTEGER NOT NULL,
    turn_number INTEGER NOT NULL, -- Numéro du tour où le point a été placé
    is_deleted BOOLEAN DEFAULT FALSE, -- TRUE si le point a été supprimé/détruit
    deleted_at TIMESTAMP, -- Date de suppression
    deleted_by_missile_id INTEGER, -- Référence vers l'action missile qui a détruit ce point
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table des actions missiles
-- Enregistre tous les missiles tirés avec leur point d'impact
CREATE TABLE IF NOT EXISTS missile_actions (
    id SERIAL PRIMARY KEY,
    game_id INTEGER REFERENCES games(id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES players(id) ON DELETE CASCADE, -- Joueur qui a lancé le missile
    launch_x INTEGER NOT NULL, -- Point de lancement X
    launch_y INTEGER NOT NULL, -- Point de lancement Y
    impact_x INTEGER NOT NULL, -- Point d'impact X
    impact_y INTEGER NOT NULL, -- Point d'impact Y
    power INTEGER NOT NULL, -- Puissance du missile (1-9)
    direction INTEGER NOT NULL, -- Direction: 1 = droite, -1 = gauche
    turn_number INTEGER NOT NULL, -- Numéro du tour
    hit_target BOOLEAN DEFAULT FALSE, -- TRUE si le missile a touché un point ennemi
    target_point_id INTEGER REFERENCES game_points(id) ON DELETE SET NULL, -- Point touché (si applicable)
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Index pour améliorer les performances
CREATE INDEX IF NOT EXISTS idx_game_points_game_id ON game_points(game_id);
CREATE INDEX IF NOT EXISTS idx_game_points_player_id ON game_points(player_id);
CREATE INDEX IF NOT EXISTS idx_game_points_coordinates ON game_points(x_coordinate, y_coordinate);
CREATE INDEX IF NOT EXISTS idx_game_points_deleted ON game_points(is_deleted);
CREATE INDEX IF NOT EXISTS idx_missile_actions_game_id ON missile_actions(game_id);
CREATE INDEX IF NOT EXISTS idx_missile_actions_player_id ON missile_actions(player_id);
CREATE INDEX IF NOT EXISTS idx_games_status ON games(status);

-- Vue pour récupérer les points actifs (non supprimés) d'une partie
CREATE OR REPLACE VIEW active_game_points AS
SELECT * FROM game_points
WHERE is_deleted = FALSE;

-- Vue pour l'historique complet d'une partie avec toutes les actions
CREATE OR REPLACE VIEW game_history AS
SELECT
    'point' as action_type,
    gp.game_id,
    gp.player_id,
    gp.turn_number,
    gp.x_coordinate as x,
    gp.y_coordinate as y,
    gp.is_deleted,
    gp.created_at
FROM game_points gp
UNION ALL
SELECT
    'missile' as action_type,
    ma.game_id,
    ma.player_id,
    ma.turn_number,
    ma.impact_x as x,
    ma.impact_y as y,
    FALSE as is_deleted,
    ma.created_at
FROM missile_actions ma
ORDER BY game_id, turn_number, created_at;

-- Fonction pour marquer un point comme supprimé (soft delete)
CREATE OR REPLACE FUNCTION soft_delete_point(
    p_point_id INTEGER,
    p_missile_id INTEGER
) RETURNS VOID AS $$
BEGIN
    UPDATE game_points
    SET
        is_deleted = TRUE,
        deleted_at = CURRENT_TIMESTAMP,
        deleted_by_missile_id = p_missile_id
    WHERE id = p_point_id AND is_deleted = FALSE;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour obtenir le score actuel d'un joueur dans une partie
CREATE OR REPLACE FUNCTION get_player_score(
    p_game_id INTEGER,
    p_player_id INTEGER
) RETURNS INTEGER AS $$
BEGIN
    RETURN (
        SELECT COUNT(*)
        FROM game_points
        WHERE game_id = p_game_id
        AND player_id = p_player_id
        AND is_deleted = FALSE
    );
END;
$$ LANGUAGE plpgsql;
