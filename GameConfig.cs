namespace point;

/// <summary>
/// Configuration centralisée du jeu pour paramétrer les règles de victoire et la grille.
/// Permet de facilement modifier les conditions de victoire sans changer le code logique.
/// </summary>
public static class GameConfig
{
    // Configuration de la victoire

    /// <summary>
    /// Nombre de points alignés pour remporter la partie.
    /// Par défaut: 5 points
    /// </summary>
    public static int PointsToWin = 5;

    /// <summary>
    /// Nombre de points pour déclencher le mode "peut gagner" (CanWin).
    /// Par défaut: 4 points (un coup avant la victoire)
    /// </summary>
    public static int PointsForCanWin = 4;

    /// <summary>
    /// Nombre de points pour déclencher le mode "formation de 3".
    /// Par défaut: 3 points
    /// </summary>
    public static int PointsForThree = 3;

    // Configuration de la grille

    /// <summary>
    /// Espacement entre les points de la grille (en pixels).
    /// Par défaut: 50 pixels
    /// </summary>
    public static int GridSize = 50;

    /// <summary>
    /// Tolérance pour détecter les clics proches d'un point d'intersection (en pixels).
    /// Par défaut: 25 pixels
    /// </summary>
    public static int ClickTolerance = 25;

    // Méthodes utilitaires

    /// <summary>
    /// Réinitialise les paramètres du jeu aux valeurs par défaut.
    /// </summary>
    public static void ResetToDefaults()
    {
        PointsToWin = 5;
        PointsForCanWin = 4;
        PointsForThree = 3;
        GridSize = 50;
        ClickTolerance = 25;
    }

    /// <summary>
    /// Configure les paramètres du jeu selon un niveau de difficulté.
    /// </summary>
    /// <param name="difficulty">"easy", "normal", "hard"</param>
    public static void SetDifficulty(string difficulty)
    {
        switch(difficulty.ToLower())
        {
            case "easy":
                PointsToWin = 4;
                PointsForCanWin = 3;
                PointsForThree = 2;
                break;
            case "hard":
                PointsToWin = 6;
                PointsForCanWin = 5;
                PointsForThree = 4;
                break;
            case "normal":
            default:
                ResetToDefaults();
                break;
        }
    }
}
