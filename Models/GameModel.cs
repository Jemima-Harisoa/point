namespace point.Models;

/// <summary>
/// Modèle représentant une partie dans la base de données
/// </summary>
public class GameModel
{
    public int Id { get; set; }
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int GridRows { get; set; }
    public int GridColumns { get; set; }
    public int? WinnerId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = "in_progress"; // 'in_progress', 'completed', 'abandoned'

    public GameModel() { }

    public GameModel(int player1Id, int player2Id, int gridRows, int gridColumns)
    {
        Player1Id = player1Id;
        Player2Id = player2Id;
        GridRows = gridRows;
        GridColumns = gridColumns;
        StartedAt = DateTime.Now;
        Status = "in_progress";
    }
}
