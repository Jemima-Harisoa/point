using Npgsql;
using point.Models;
using System.Data;

namespace point.Database;

/// <summary>
/// Gestionnaire de base de données PostgreSQL pour le jeu de point
/// Gère toutes les interactions avec la base de données avec historisation complète
/// </summary>
public class DatabaseManager : IDisposable
{
    private readonly string _connectionString;
    private NpgsqlConnection? _connection;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Ouvre une connexion à la base de données
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            _connection = new NpgsqlConnection(_connectionString);
            await _connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion à la base de données: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Teste la connexion à la base de données
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Player Methods

    /// <summary>
    /// Crée ou récupère un joueur par son nom
    /// </summary>
    public async Task<PlayerModel?> GetOrCreatePlayerAsync(string playerName)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return null;
        }

        // Chercher d'abord si le joueur existe
        var selectQuery = "SELECT id, name, created_at FROM players WHERE name = @name LIMIT 1";
        using (var cmd = new NpgsqlCommand(selectQuery, _connection))
        {
            cmd.Parameters.AddWithValue("@name", playerName);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PlayerModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                };
            }
        }

        // Sinon, créer le joueur
        var insertQuery = "INSERT INTO players (name) VALUES (@name) RETURNING id, created_at";
        using (var cmd = new NpgsqlCommand(insertQuery, _connection))
        {
            cmd.Parameters.AddWithValue("@name", playerName);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PlayerModel
                {
                    Id = reader.GetInt32(0),
                    Name = playerName,
                    CreatedAt = reader.GetDateTime(1)
                };
            }
        }

        return null;
    }

    #endregion

    #region Game Methods

    /// <summary>
    /// Crée une nouvelle partie
    /// </summary>
    public async Task<GameModel?> CreateGameAsync(int player1Id, int player2Id, int gridRows, int gridColumns)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return null;
        }

        var query = @"INSERT INTO games (player1_id, player2_id, grid_rows, grid_columns, status)
                     VALUES (@p1, @p2, @rows, @cols, 'in_progress')
                     RETURNING id, started_at";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@p1", player1Id);
        cmd.Parameters.AddWithValue("@p2", player2Id);
        cmd.Parameters.AddWithValue("@rows", gridRows);
        cmd.Parameters.AddWithValue("@cols", gridColumns);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new GameModel
            {
                Id = reader.GetInt32(0),
                Player1Id = player1Id,
                Player2Id = player2Id,
                GridRows = gridRows,
                GridColumns = gridColumns,
                StartedAt = reader.GetDateTime(1),
                Status = "in_progress"
            };
        }

        return null;
    }

    /// <summary>
    /// Met à jour le statut d'une partie et définit le gagnant
    /// </summary>
    public async Task<bool> UpdateGameStatusAsync(int gameId, string status, int? winnerId = null)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return false;
        }

        var query = @"UPDATE games
                     SET status = @status, winner_id = @winnerId, ended_at = @endedAt
                     WHERE id = @gameId";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@winnerId", winnerId.HasValue ? (object)winnerId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@endedAt", status == "completed" ? DateTime.Now : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@gameId", gameId);

        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    #endregion

    #region GamePoint Methods

    /// <summary>
    /// Ajoute un point à la partie
    /// </summary>
    public async Task<GamePointModel?> AddPointAsync(int gameId, int playerId, int x, int y, int turnNumber)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return null;
        }

        var query = @"INSERT INTO game_points (game_id, player_id, x_coordinate, y_coordinate, turn_number)
                     VALUES (@gameId, @playerId, @x, @y, @turn)
                     RETURNING id, created_at";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);
        cmd.Parameters.AddWithValue("@playerId", playerId);
        cmd.Parameters.AddWithValue("@x", x);
        cmd.Parameters.AddWithValue("@y", y);
        cmd.Parameters.AddWithValue("@turn", turnNumber);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new GamePointModel
            {
                Id = reader.GetInt32(0),
                GameId = gameId,
                PlayerId = playerId,
                XCoordinate = x,
                YCoordinate = y,
                TurnNumber = turnNumber,
                IsDeleted = false,
                CreatedAt = reader.GetDateTime(1)
            };
        }

        return null;
    }

    /// <summary>
    /// Marque un point comme supprimé (soft delete) - historisation
    /// </summary>
    public async Task<bool> SoftDeletePointAsync(int pointId, int? missileId = null)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return false;
        }

        var query = @"UPDATE game_points
                     SET is_deleted = TRUE, deleted_at = @deletedAt, deleted_by_missile_id = @missileId
                     WHERE id = @pointId AND is_deleted = FALSE";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@deletedAt", DateTime.Now);
        cmd.Parameters.AddWithValue("@missileId", missileId.HasValue ? (object)missileId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@pointId", pointId);

        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    /// <summary>
    /// Récupère tous les points actifs (non supprimés) d'une partie
    /// </summary>
    public async Task<List<GamePointModel>> GetActivePointsAsync(int gameId)
    {
        var points = new List<GamePointModel>();

        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return points;
        }

        var query = @"SELECT id, game_id, player_id, x_coordinate, y_coordinate, turn_number,
                            is_deleted, deleted_at, deleted_by_missile_id, created_at
                     FROM game_points
                     WHERE game_id = @gameId AND is_deleted = FALSE
                     ORDER BY turn_number";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            points.Add(new GamePointModel
            {
                Id = reader.GetInt32(0),
                GameId = reader.GetInt32(1),
                PlayerId = reader.GetInt32(2),
                XCoordinate = reader.GetInt32(3),
                YCoordinate = reader.GetInt32(4),
                TurnNumber = reader.GetInt32(5),
                IsDeleted = reader.GetBoolean(6),
                DeletedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                DeletedByMissileId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }

        return points;
    }

    /// <summary>
    /// Récupère TOUS les points d'une partie, y compris les supprimés (historique complet)
    /// </summary>
    public async Task<List<GamePointModel>> GetAllPointsWithHistoryAsync(int gameId)
    {
        var points = new List<GamePointModel>();

        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return points;
        }

        var query = @"SELECT id, game_id, player_id, x_coordinate, y_coordinate, turn_number,
                            is_deleted, deleted_at, deleted_by_missile_id, created_at
                     FROM game_points
                     WHERE game_id = @gameId
                     ORDER BY turn_number";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            points.Add(new GamePointModel
            {
                Id = reader.GetInt32(0),
                GameId = reader.GetInt32(1),
                PlayerId = reader.GetInt32(2),
                XCoordinate = reader.GetInt32(3),
                YCoordinate = reader.GetInt32(4),
                TurnNumber = reader.GetInt32(5),
                IsDeleted = reader.GetBoolean(6),
                DeletedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                DeletedByMissileId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }

        return points;
    }

    /// <summary>
    /// Trouve un point actif aux coordonnées données
    /// </summary>
    public async Task<GamePointModel?> FindPointAtAsync(int gameId, int x, int y)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return null;
        }

        var query = @"SELECT id, game_id, player_id, x_coordinate, y_coordinate, turn_number,
                            is_deleted, deleted_at, deleted_by_missile_id, created_at
                     FROM game_points
                     WHERE game_id = @gameId AND x_coordinate = @x AND y_coordinate = @y
                           AND is_deleted = FALSE
                     LIMIT 1";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);
        cmd.Parameters.AddWithValue("@x", x);
        cmd.Parameters.AddWithValue("@y", y);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new GamePointModel
            {
                Id = reader.GetInt32(0),
                GameId = reader.GetInt32(1),
                PlayerId = reader.GetInt32(2),
                XCoordinate = reader.GetInt32(3),
                YCoordinate = reader.GetInt32(4),
                TurnNumber = reader.GetInt32(5),
                IsDeleted = reader.GetBoolean(6),
                DeletedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                DeletedByMissileId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CreatedAt = reader.GetDateTime(9)
            };
        }

        return null;
    }

    #endregion

    #region MissileAction Methods

    /// <summary>
    /// Enregistre une action missile avec son point d'impact
    /// </summary>
    public async Task<MissileActionModel?> AddMissileActionAsync(int gameId, int playerId, int launchX, int launchY,
                                                                  int impactX, int impactY, int power, int direction,
                                                                  int turnNumber, bool hitTarget = false, int? targetPointId = null)
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return null;
        }

        var query = @"INSERT INTO missile_actions
                     (game_id, player_id, launch_x, launch_y, impact_x, impact_y, power, direction,
                      turn_number, hit_target, target_point_id)
                     VALUES (@gameId, @playerId, @lx, @ly, @ix, @iy, @power, @dir, @turn, @hit, @targetId)
                     RETURNING id, created_at";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);
        cmd.Parameters.AddWithValue("@playerId", playerId);
        cmd.Parameters.AddWithValue("@lx", launchX);
        cmd.Parameters.AddWithValue("@ly", launchY);
        cmd.Parameters.AddWithValue("@ix", impactX);
        cmd.Parameters.AddWithValue("@iy", impactY);
        cmd.Parameters.AddWithValue("@power", power);
        cmd.Parameters.AddWithValue("@dir", direction);
        cmd.Parameters.AddWithValue("@turn", turnNumber);
        cmd.Parameters.AddWithValue("@hit", hitTarget);
        cmd.Parameters.AddWithValue("@targetId", targetPointId.HasValue ? (object)targetPointId.Value : DBNull.Value);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MissileActionModel
            {
                Id = reader.GetInt32(0),
                GameId = gameId,
                PlayerId = playerId,
                LaunchX = launchX,
                LaunchY = launchY,
                ImpactX = impactX,
                ImpactY = impactY,
                Power = power,
                Direction = direction,
                TurnNumber = turnNumber,
                HitTarget = hitTarget,
                TargetPointId = targetPointId,
                CreatedAt = reader.GetDateTime(1)
            };
        }

        return null;
    }

    /// <summary>
    /// Récupère toutes les actions missiles d'une partie
    /// </summary>
    public async Task<List<MissileActionModel>> GetMissileActionsAsync(int gameId)
    {
        var missiles = new List<MissileActionModel>();

        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            if (!await ConnectAsync())
                return missiles;
        }

        var query = @"SELECT id, game_id, player_id, launch_x, launch_y, impact_x, impact_y,
                            power, direction, turn_number, hit_target, target_point_id, created_at
                     FROM missile_actions
                     WHERE game_id = @gameId
                     ORDER BY turn_number";

        using var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.AddWithValue("@gameId", gameId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            missiles.Add(new MissileActionModel
            {
                Id = reader.GetInt32(0),
                GameId = reader.GetInt32(1),
                PlayerId = reader.GetInt32(2),
                LaunchX = reader.GetInt32(3),
                LaunchY = reader.GetInt32(4),
                ImpactX = reader.GetInt32(5),
                ImpactY = reader.GetInt32(6),
                Power = reader.GetInt32(7),
                Direction = reader.GetInt32(8),
                TurnNumber = reader.GetInt32(9),
                HitTarget = reader.GetBoolean(10),
                TargetPointId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                CreatedAt = reader.GetDateTime(12)
            });
        }

        return missiles;
    }

    #endregion

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
