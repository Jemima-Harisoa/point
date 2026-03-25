using point;
using point.Database;
using point.Models;

namespace point.Examples;

/// <summary>
/// Exemple d'intégration du système de sauvegarde hybride dans Window.cs
/// Ce fichier montre comment utiliser GameSaveManager pour persister les données
/// </summary>
public class WindowIntegrationExample
{
    // Champs privés pour la gestion de sauvegarde
    private GameSaveManager? _saveManager;
    private List<Point> clickedPoints = new List<Point>();
    private int tour = 0;
    private Player player1;
    private Player player2;

    /// <summary>
    /// Initialiser le système de sauvegarde au démarrage de l'application
    /// À appeler dans le constructeur de Window ou au début de InitializeComponent
    /// </summary>
    private async Task InitializeSaveSystemAsync()
    {
        _saveManager = new GameSaveManager(clickedPoints);

        // Test de connexion PostgreSQL
        var config = DatabaseConfig.GetDefaultConfig();
        bool canConnect = await _saveManager.TestDatabaseConnectionAsync(config);

        if (canConnect)
        {
            Console.WriteLine("✓ PostgreSQL disponible");
        }
        else
        {
            Console.WriteLine("⚠ PostgreSQL non disponible - Mode local uniquement");
        }
    }

    /// <summary>
    /// Demander à l'utilisateur le mode de sauvegarde qu'il souhaite
    /// À appeler au début d'une nouvelle partie
    /// </summary>
    private async Task<bool> PromptForSaveModeAsync()
    {
        var result = MessageBox.Show(
            "Voulez-vous utiliser la sauvegarde PostgreSQL avec historisation complète ?\n\n" +
            "✓ OUI : Base de données PostgreSQL (historique complet, replay, statistiques)\n" +
            "✗ NON : Fichier local uniquement (mode classique)",
            "Mode de sauvegarde",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            var config = DatabaseConfig.GetDefaultConfig();
            bool connected = await _saveManager!.EnableDatabasePersistenceAsync(config);

            if (connected)
            {
                MessageBox.Show(
                    "✓ Connexion PostgreSQL établie\n\n" +
                    "Votre partie sera sauvegardée en base de données avec historisation complète.",
                    "Sauvegarde activée",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return true;
            }
            else
            {
                MessageBox.Show(
                    "✗ Connexion PostgreSQL échouée\n\n" +
                    "Le jeu utilisera la sauvegarde locale classique.",
                    "Mode local",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Démarrer une nouvelle partie
    /// À appeler dans le bouton "Start" ou équivalent
    /// </summary>
    private async void StartNewGame()
    {
        // Demander le mode de sauvegarde
        bool usesDatabase = await PromptForSaveModeAsync();

        // Si PostgreSQL est activé, créer la partie en base
        if (usesDatabase && _saveManager != null)
        {
            await _saveManager.StartNewGameAsync(
                player1.nom,
                player2.nom,
                GameConfig.GridRows,
                GameConfig.GridColumns
            );
        }

        // Reste de la logique de démarrage...
        tour = 0;
        clickedPoints.Clear();
    }

    /// <summary>
    /// Gérer le placement d'un point par un joueur
    /// À appeler dans space_MouseClick ou équivalent
    /// </summary>
    private async void OnPointPlaced(Point snappedPoint, Player currentPlayer)
    {
        // Ajouter le point à la liste locale
        clickedPoints.Add(snappedPoint);

        // Déterminer l'ordre du joueur (0 ou 1)
        int playerOrder = currentPlayer.Order;

        // Enregistrer en base de données si activé
        if (_saveManager != null && _saveManager.IsDatabaseEnabled)
        {
            await _saveManager.SavePointAsync(snappedPoint, playerOrder, tour);
        }

        // Incrémenter le tour
        tour++;

        // Reste de la logique...
    }

    /// <summary>
    /// Gérer le lancement d'un missile
    /// À appeler après Missile.Launch()
    /// </summary>
    private async void OnMissileLaunched(Missile missile, Player launchingPlayer, Player opponentPlayer)
    {
        // Le missile a été lancé et sa trajectoire calculée
        Point launchPoint = missile.Position;
        Point impactPoint = missile.GetCurrentPosition();

        // Vérifier si le missile touche une cible
        bool hitTarget = false;
        Point? targetPoint = null;

        // Chercher un point ennemi sur la trajectoire
        foreach (var trajectoryPoint in missile.GetTrajectory())
        {
            foreach (var enemyPoint in opponentPlayer.line.getSameColor())
            {
                if (trajectoryPoint.X == enemyPoint.X && trajectoryPoint.Y == enemyPoint.Y)
                {
                    hitTarget = true;
                    targetPoint = enemyPoint;
                    missile.HitTarget = true;

                    // Retirer le point de la liste locale
                    clickedPoints.Remove(enemyPoint);
                    Line.ClickedPoints.Remove(enemyPoint);

                    break;
                }
            }
            if (hitTarget) break;
        }

        // Enregistrer l'action missile en base de données
        if (_saveManager != null && _saveManager.IsDatabaseEnabled)
        {
            await _saveManager.SaveMissileActionAsync(
                launchPoint,
                impactPoint,
                missile.Power,
                missile.Direction,
                launchingPlayer.Order,
                tour,
                hitTarget,
                targetPoint
            );
        }

        // Incrémenter le tour
        tour++;

        // Reste de la logique d'animation...
    }

    /// <summary>
    /// Terminer une partie avec un gagnant
    /// À appeler quand un joueur gagne
    /// </summary>
    private async void EndGame(Player winner)
    {
        // Enregistrer la fin de partie en base de données
        if (_saveManager != null && _saveManager.IsDatabaseEnabled)
        {
            await _saveManager.EndGameAsync(winner.Order);
        }

        // Sauvegarder localement aussi
        if (_saveManager != null)
        {
            _saveManager.WriteLocalSave(player1.nom, player2.nom, clickedPoints);
        }

        MessageBox.Show(
            $"🎉 {winner.nom} a gagné la partie !",
            "Victoire",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    /// <summary>
    /// Sauvegarder manuellement la partie en cours
    /// À appeler dans le bouton "Save"
    /// </summary>
    private async void SaveCurrentGame()
    {
        if (_saveManager != null)
        {
            // Sauvegarde locale (toujours effectuée)
            _saveManager.WriteLocalSave(player1.nom, player2.nom, clickedPoints);

            // Sauvegarder en base si activé (déjà fait automatiquement à chaque action)
            MessageBox.Show(
                "✓ Partie sauvegardée",
                "Sauvegarde",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }

    /// <summary>
    /// Charger une partie depuis la sauvegarde locale
    /// À appeler dans le bouton "Load"
    /// </summary>
    private void LoadGame()
    {
        if (_saveManager != null)
        {
            // Charger depuis le fichier local
            var loadedPoints = _saveManager.LoadLocalSave();
            var playerNames = GameSaveManager.LoadLocalPlayerList();

            if (playerNames.Count > 0)
            {
                // Restaurer les joueurs et points
                clickedPoints.Clear();
                clickedPoints.AddRange(loadedPoints);

                MessageBox.Show(
                    $"✓ Partie chargée : {playerNames[0][0]} vs {playerNames[0][1]}",
                    "Chargement",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Redessiner
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Afficher l'historique complet de la partie actuelle
    /// Bouton optionnel pour voir tous les points et missiles (même supprimés)
    /// </summary>
    private async void ShowGameHistory()
    {
        if (_saveManager == null || !_saveManager.IsDatabaseEnabled)
        {
            MessageBox.Show(
                "L'historique complet est uniquement disponible avec PostgreSQL.",
                "Historique",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        // Récupérer l'historique complet
        var allPoints = await _saveManager.GetGameHistoryAsync();
        var missiles = await _saveManager.GetMissileHistoryAsync();

        // Afficher dans une nouvelle fenêtre ou console
        Console.WriteLine("=== HISTORIQUE DE LA PARTIE ===");
        Console.WriteLine($"\nPoints totaux : {allPoints.Count}");
        Console.WriteLine($"Points actifs : {allPoints.Count(p => !p.IsDeleted)}");
        Console.WriteLine($"Points supprimés : {allPoints.Count(p => p.IsDeleted)}");
        Console.WriteLine($"Missiles lancés : {missiles.Count}");

        Console.WriteLine("\n--- Points supprimés ---");
        foreach (var point in allPoints.Where(p => p.IsDeleted))
        {
            Console.WriteLine($"Point ({point.XCoordinate}, {point.YCoordinate}) " +
                            $"- Supprimé le {point.DeletedAt} par missile #{point.DeletedByMissileId}");
        }

        Console.WriteLine("\n--- Missiles ---");
        foreach (var missile in missiles)
        {
            Console.WriteLine($"Missile - Impact: ({missile.ImpactX}, {missile.ImpactY}) " +
                            $"- Puissance: {missile.Power} - Touché: {missile.HitTarget}");
        }

        MessageBox.Show(
            $"Historique affiché dans la console.\n\n" +
            $"Points : {allPoints.Count(p => !p.IsDeleted)}/{allPoints.Count}\n" +
            $"Missiles : {missiles.Count}",
            "Historique",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    /// <summary>
    /// Nettoyer les ressources
    /// À appeler dans Dispose() ou à la fermeture de la fenêtre
    /// </summary>
    private void CleanupSaveSystem()
    {
        _saveManager?.Dispose();
    }
}
