using point.Database;
using point.Models;

namespace point;

/// <summary>
/// Gestionnaire de sauvegarde hybride : local (fichier texte) + PostgreSQL
/// Conserve la logique actuelle tout en ajoutant la persistance en base de données avec historisation
/// </summary>
public class GameSaveManager : IDisposable
{
    private Save _localSave;
    private DatabaseManager? _dbManager;
    private bool _useDatabasePersistence;
    private GameModel? _currentGame;
    private PlayerModel? _player1Model;
    private PlayerModel? _player2Model;
    private Dictionary<Point, int> _pointToDbId; // Mapping point -> DB ID pour soft delete

    public bool IsDatabaseEnabled => _useDatabasePersistence && _dbManager != null;
    public GameModel? CurrentGame => _currentGame;

    public GameSaveManager(List<Point> clickedPoints, List<int> pointOwners = null)
    {
        _localSave = new Save(clickedPoints, pointOwners ?? new List<int>());
        _useDatabasePersistence = false;
        _pointToDbId = new Dictionary<Point, int>();
    }

    /// <summary>
    /// Active la persistance PostgreSQL
    /// </summary>
    public async Task<bool> EnableDatabasePersistenceAsync(DatabaseConfig config)
    {
        try
        {
            _dbManager = new DatabaseManager(config.GetConnectionString());
            var connected = await _dbManager.ConnectAsync();
            if (connected)
            {
                _useDatabasePersistence = true;
                Console.WriteLine("✓ Connexion PostgreSQL établie");
                return true;
            }
            else
            {
                Console.WriteLine("✗ Échec de connexion à PostgreSQL");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'activation de la persistance DB: {ex.Message}");
            _useDatabasePersistence = false;
            return false;
        }
    }

    /// <summary>
    /// Désactive la persistance PostgreSQL (bascule en mode local uniquement)
    /// </summary>
    public void DisableDatabasePersistence()
    {
        _useDatabasePersistence = false;
        _dbManager?.Dispose();
        _dbManager = null;
        Console.WriteLine("Mode de sauvegarde : local uniquement");
    }

    /// <summary>
    /// Teste la connexion à la base de données
    /// </summary>
    public async Task<bool> TestDatabaseConnectionAsync(DatabaseConfig config)
    {
        try
        {
            using var testManager = new DatabaseManager(config.GetConnectionString());
            return await testManager.TestConnectionAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Initialise une nouvelle partie en base de données
    /// </summary>
    public async Task<bool> StartNewGameAsync(string player1Name, string player2Name, int gridRows, int gridColumns)
    {
        if (!_useDatabasePersistence || _dbManager == null)
            return false;

        try
        {
            // Créer ou récupérer les joueurs
            _player1Model = await _dbManager.GetOrCreatePlayerAsync(player1Name);
            _player2Model = await _dbManager.GetOrCreatePlayerAsync(player2Name);

            if (_player1Model == null || _player2Model == null)
            {
                Console.WriteLine("Erreur : impossible de créer/récupérer les joueurs");
                return false;
            }

            // Créer la partie
            _currentGame = await _dbManager.CreateGameAsync(
                _player1Model.Id,
                _player2Model.Id,
                gridRows,
                gridColumns
            );

            if (_currentGame != null)
            {
                Console.WriteLine($"✓ Partie créée en DB (Game ID: {_currentGame.Id})");
                _pointToDbId.Clear();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la création de la partie: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Enregistre un point placé par un joueur
    /// Sauvegarde locale + PostgreSQL avec historisation
    /// </summary>
    public async Task<bool> SavePointAsync(Point point, int playerOrder, int turnNumber)
    {
        bool success = true;

        // Sauvegarde PostgreSQL
        if (_useDatabasePersistence && _dbManager != null && _currentGame != null)
        {
            try
            {
                var playerId = playerOrder == 0 ? _player1Model!.Id : _player2Model!.Id;
                var pointModel = await _dbManager.AddPointAsync(
                    _currentGame.Id,
                    playerId,
                    point.X,
                    point.Y,
                    turnNumber
                );

                if (pointModel != null)
                {
                    _pointToDbId[point] = pointModel.Id;
                    Console.WriteLine($"✓ Point sauvegardé en DB: ({point.X}, {point.Y}) - Turn {turnNumber}");
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde du point en DB: {ex.Message}");
                success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// Enregistre une action missile avec point d'impact et indication visuelle
    /// </summary>
    public async Task<bool> SaveMissileActionAsync(Point launchPoint, Point impactPoint,
                                                     int power, int direction, int playerOrder,
                                                     int turnNumber, bool hitTarget, Point? targetPoint = null)
    {
        bool success = true;

        // Sauvegarde PostgreSQL
        if (_useDatabasePersistence && _dbManager != null && _currentGame != null)
        {
            try
            {
                var playerId = playerOrder == 0 ? _player1Model!.Id : _player2Model!.Id;
                int? targetPointId = null;

                // Si le missile a touché un point, récupérer son ID
                if (hitTarget && targetPoint.HasValue && _pointToDbId.ContainsKey(targetPoint.Value))
                {
                    targetPointId = _pointToDbId[targetPoint.Value];
                }

                var missileModel = await _dbManager.AddMissileActionAsync(
                    _currentGame.Id,
                    playerId,
                    launchPoint.X,
                    launchPoint.Y,
                    impactPoint.X,
                    impactPoint.Y,
                    power,
                    direction,
                    turnNumber,
                    hitTarget,
                    targetPointId
                );

                if (missileModel != null)
                {
                    Console.WriteLine($"✓ Missile sauvegardé en DB: Impact ({impactPoint.X}, {impactPoint.Y}) - Hit: {hitTarget}");

                    // Si le missile a touché un point, le marquer comme supprimé (soft delete)
                    if (hitTarget && targetPointId.HasValue)
                    {
                        await SoftDeletePointAsync(targetPoint!.Value, missileModel.Id);
                    }
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde du missile en DB: {ex.Message}");
                success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// Marque un point comme supprimé (soft delete) au lieu de le supprimer physiquement
    /// HISTORISATION : le point reste en base mais est marqué is_deleted = TRUE
    /// </summary>
    public async Task<bool> SoftDeletePointAsync(Point point, int? missileId = null)
    {
        if (!_useDatabasePersistence || _dbManager == null || !_pointToDbId.ContainsKey(point))
            return false;

        try
        {
            var pointId = _pointToDbId[point];
            var deleted = await _dbManager.SoftDeletePointAsync(pointId, missileId);

            if (deleted)
            {
                Console.WriteLine($"✓ Point marqué comme supprimé (historisé): ({point.X}, {point.Y})");
            }

            return deleted;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la suppression douce du point: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sauvegarde complète locale (conserve la logique actuelle)
    /// </summary>
    public void WriteLocalSave(string player1, string player2, List<Point> points, List<int> owners = null)
    {
        _localSave.ClickedPoints = points;
        if (owners != null)
        {
            _localSave.PointOwners = owners;
        }
        _localSave.Write(player1, player2);
        Console.WriteLine("✓ Sauvegarde locale effectuée");
    }

    /// <summary>
    /// Charge les points depuis la sauvegarde locale
    /// </summary>
    public List<Point> LoadLocalSave()
    {
        return _localSave.getPointList();
    }

    /// <summary>
    /// Charge les noms des joueurs depuis la sauvegarde locale
    /// </summary>
    public static List<string[]> LoadLocalPlayerList()
    {
        return Save.getPlayerList();
    }

    /// <summary>
    /// Termine une partie et définit le gagnant
    /// </summary>
    public async Task<bool> EndGameAsync(int? winnerOrder = null)
    {
        if (!_useDatabasePersistence || _dbManager == null || _currentGame == null)
            return false;

        try
        {
            int? winnerId = null;
            if (winnerOrder.HasValue)
            {
                winnerId = winnerOrder.Value == 0 ? _player1Model!.Id : _player2Model!.Id;
            }

            var updated = await _dbManager.UpdateGameStatusAsync(_currentGame.Id, "completed", winnerId);

            if (updated)
            {
                Console.WriteLine($"✓ Partie terminée - Gagnant ID: {winnerId}");
            }

            return updated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la fin de partie: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Récupère l'historique complet de la partie (points actifs + supprimés)
    /// </summary>
    public async Task<List<GamePointModel>> GetGameHistoryAsync()
    {
        if (!_useDatabasePersistence || _dbManager == null || _currentGame == null)
            return new List<GamePointModel>();

        return await _dbManager.GetAllPointsWithHistoryAsync(_currentGame.Id);
    }

    /// <summary>
    /// Récupère toutes les actions missiles de la partie
    /// </summary>
    public async Task<List<MissileActionModel>> GetMissileHistoryAsync()
    {
        if (!_useDatabasePersistence || _dbManager == null || _currentGame == null)
            return new List<MissileActionModel>();

        return await _dbManager.GetMissileActionsAsync(_currentGame.Id);
    }

    public void Dispose()
    {
        _dbManager?.Dispose();
    }
}
