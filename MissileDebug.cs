using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace point;

/// <summary>
/// Classe utilitaire pour déboguer le système de sauvegarde des missiles
/// </summary>
public static class MissileDebug
{
    private static string debugPath = "C:/Users/ACER/Documents/L2/C#/point/debug_missiles.txt";

    /// <summary>
    /// Affiche l'état des missiles avant sauvegarde
    /// </summary>
    public static void LogMissilesBeforeSave(Player player1, Player player2)
    {
        using (StreamWriter writer = new StreamWriter(debugPath, false))
        {
            writer.WriteLine("=== MISSILES AVANT SAUVEGARDE ===");
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine();

            writer.WriteLine($"Joueur 1 ({player1.nom}) - Order: {player1.Order}");
            writer.WriteLine($"Nombre de missiles: {player1.Missiles.Count}");
            foreach (var missile in player1.Missiles)
            {
                Point impact = missile.GetCurrentPosition();
                writer.WriteLine($"  - State: {missile.State}, HitTarget: {missile.HitTarget}");
                writer.WriteLine($"    Launch: ({missile.Position.X}, {missile.Position.Y})");
                writer.WriteLine($"    Impact: ({impact.X}, {impact.Y})");
                writer.WriteLine($"    Power: {missile.Power}, Direction: {missile.Direction}");
            }
            writer.WriteLine();

            writer.WriteLine($"Joueur 2 ({player2.nom}) - Order: {player2.Order}");
            writer.WriteLine($"Nombre de missiles: {player2.Missiles.Count}");
            foreach (var missile in player2.Missiles)
            {
                Point impact = missile.GetCurrentPosition();
                writer.WriteLine($"  - State: {missile.State}, HitTarget: {missile.HitTarget}");
                writer.WriteLine($"    Launch: ({missile.Position.X}, {missile.Position.Y})");
                writer.WriteLine($"    Impact: ({impact.X}, {impact.Y})");
                writer.WriteLine($"    Power: {missile.Power}, Direction: {missile.Direction}");
            }
        }
    }

    /// <summary>
    /// Affiche l'état des missiles après chargement
    /// </summary>
    public static void LogMissilesAfterLoad(Player player1, Player player2, List<SavedMissile> savedMissiles)
    {
        using (StreamWriter writer = new StreamWriter(debugPath, true))
        {
            writer.WriteLine();
            writer.WriteLine("=== MISSILES APRÈS CHARGEMENT ===");
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine();

            writer.WriteLine($"Missiles sauvegardés lus: {savedMissiles.Count}");
            foreach (var saved in savedMissiles)
            {
                writer.WriteLine($"  - OwnerOrder: {saved.OwnerOrder}, HitTarget: {saved.HitTarget}");
                writer.WriteLine($"    Launch: ({saved.LaunchX}, {saved.LaunchY})");
                writer.WriteLine($"    Impact: ({saved.ImpactX}, {saved.ImpactY})");
                writer.WriteLine($"    Power: {saved.Power}, Direction: {saved.Direction}");
            }
            writer.WriteLine();

            writer.WriteLine($"Joueur 1 ({player1.nom}) - Order: {player1.Order}");
            writer.WriteLine($"Nombre de missiles restaurés: {player1.Missiles.Count}");
            foreach (var missile in player1.Missiles)
            {
                Point impact = missile.GetCurrentPosition();
                writer.WriteLine($"  - State: {missile.State}, HitTarget: {missile.HitTarget}");
                writer.WriteLine($"    Launch: ({missile.Position.X}, {missile.Position.Y})");
                writer.WriteLine($"    Impact: ({impact.X}, {impact.Y})");
                writer.WriteLine($"    Color: {missile.OwnerColor}");
            }
            writer.WriteLine();

            writer.WriteLine($"Joueur 2 ({player2.nom}) - Order: {player2.Order}");
            writer.WriteLine($"Nombre de missiles restaurés: {player2.Missiles.Count}");
            foreach (var missile in player2.Missiles)
            {
                Point impact = missile.GetCurrentPosition();
                writer.WriteLine($"  - State: {missile.State}, HitTarget: {missile.HitTarget}");
                writer.WriteLine($"    Launch: ({missile.Position.X}, {missile.Position.Y})");
                writer.WriteLine($"    Impact: ({impact.X}, {impact.Y})");
                writer.WriteLine($"    Color: {missile.OwnerColor}");
            }
        }
    }
}
