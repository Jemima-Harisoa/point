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
    /// Nombre de colonnes dans la grille.
    /// Par défaut: 15 colonnes
    /// </summary>
    public static int GridColumns = 15;

    /// <summary>
    /// Nombre de lignes dans la grille.
    /// Par défaut: 10 lignes
    /// </summary>
    public static int GridRows = 10;

    /// <summary>
    /// Espacement entre les points de la grille (en pixels).
    /// Calculé automatiquement selon la taille du panneau et le nombre de colonnes/lignes.
    /// </summary>
    public static int GridSize { get; private set; } = 50;

    /// <summary>
    /// Tolérance pour détecter les clics proches d'un point d'intersection (en pixels).
    /// Par défaut: 25 pixels (50% de GridSize)
    /// </summary>
    public static int ClickTolerance { get; private set; } = 25;

    // Méthodes utilitaires

    /// <summary>
    /// Réinitialise les paramètres du jeu aux valeurs par défaut.
    /// </summary>
    public static void ResetToDefaults()
    {
        PointsToWin = 5;
        PointsForCanWin = 4;
        PointsForThree = 3;
        GridColumns = 15;
        GridRows = 10;
        UpdateGridSize();
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
                GridColumns = 10;
                GridRows = 8;
                break;
            case "hard":
                PointsToWin = 6;
                PointsForCanWin = 5;
                PointsForThree = 4;
                GridColumns = 20;
                GridRows = 15;
                break;
            case "normal":
            default:
                ResetToDefaults();
                break;
        }
        UpdateGridSize();
    }

    /// <summary>
    /// Met à jour la taille de la grille en pixels selon les dimensions du panneau.
    /// La taille est calculée pour s'adapter parfaitement au nombre de colonnes/lignes.
    /// </summary>
    /// <param name="panelWidth">Largeur du panneau en pixels</param>
    /// <param name="panelHeight">Hauteur du panneau en pixels</param>
    public static void UpdateGridSize(int panelWidth = 700, int panelHeight = 450)
    {
        // Ajouter une "cellule" de marge autour de la grille :
        // si N lignes/colonnes, on calcule la taille sur N+1 intervalles.
        int safeColumns = Math.Max(1, GridColumns + 1);
        int safeRows = Math.Max(1, GridRows + 1);

        // Calculer la taille de grille pour que les colonnes/lignes s'adaptent avec marge
        int gridSizeX = Math.Max(1, panelWidth / safeColumns);
        int gridSizeY = Math.Max(1, panelHeight / safeRows);

        // Utiliser la taille la plus petite pour garder une grille carrée
        GridSize = Math.Min(gridSizeX, gridSizeY);

        // La tolérance de clic est 50% de la taille de la grille
        ClickTolerance = GridSize / 2;
    }

    /// <summary>
    /// Configure la grille avec un nombre spécifique de colonnes et lignes.
    /// </summary>
    /// <param name="columns">Nombre de colonnes</param>
    /// <param name="rows">Nombre de lignes</param>
    public static void SetGridDimensions(int columns, int rows)
    {
        GridColumns = Math.Max(5, columns);  // Minimum 5 colonnes
        GridRows = Math.Max(5, rows);        // Minimum 5 lignes
        UpdateGridSize();
    }
}

