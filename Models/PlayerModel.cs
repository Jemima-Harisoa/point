namespace point.Models;

/// <summary>
/// Modèle représentant un joueur dans la base de données
/// </summary>
public class PlayerModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public PlayerModel() { }

    public PlayerModel(string name)
    {
        Name = name;
        CreatedAt = DateTime.Now;
    }
}
