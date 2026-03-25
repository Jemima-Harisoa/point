namespace point.Models;

/// <summary>
/// Modèle représentant une action de missile dans la base de données
/// Enregistre tous les missiles tirés avec leur point d'impact et leur résultat
/// </summary>
public class MissileActionModel
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public int LaunchX { get; set; }
    public int LaunchY { get; set; }
    public int ImpactX { get; set; }
    public int ImpactY { get; set; }
    public int Power { get; set; }
    public int Direction { get; set; } // 1 = droite, -1 = gauche
    public int TurnNumber { get; set; }
    public bool HitTarget { get; set; }
    public int? TargetPointId { get; set; }
    public DateTime CreatedAt { get; set; }

    public MissileActionModel() { }

    public MissileActionModel(int gameId, int playerId, int launchX, int launchY,
                             int impactX, int impactY, int power, int direction,
                             int turnNumber, bool hitTarget = false, int? targetPointId = null)
    {
        GameId = gameId;
        PlayerId = playerId;
        LaunchX = launchX;
        LaunchY = launchY;
        ImpactX = impactX;
        ImpactY = impactY;
        Power = power;
        Direction = direction;
        TurnNumber = turnNumber;
        HitTarget = hitTarget;
        TargetPointId = targetPointId;
        CreatedAt = DateTime.Now;
    }
}
