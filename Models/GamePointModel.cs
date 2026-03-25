namespace point.Models;

/// <summary>
/// Modèle représentant un point de jeu avec historisation (soft delete)
/// Les points ne sont jamais supprimés physiquement, seulement marqués comme deleted
/// </summary>
public class GamePointModel
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public int XCoordinate { get; set; }
    public int YCoordinate { get; set; }
    public int TurnNumber { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByMissileId { get; set; }
    public DateTime CreatedAt { get; set; }

    public GamePointModel() { }

    public GamePointModel(int gameId, int playerId, int x, int y, int turnNumber)
    {
        GameId = gameId;
        PlayerId = playerId;
        XCoordinate = x;
        YCoordinate = y;
        TurnNumber = turnNumber;
        IsDeleted = false;
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Marque le point comme supprimé (soft delete)
    /// </summary>
    public void MarkAsDeleted(int? missileId = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        DeletedByMissileId = missileId;
    }
}
