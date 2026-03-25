using System.Text;
using System.IO;

namespace point;

/// <summary>
/// Données d'un missile sauvegardé
/// </summary>
public class SavedMissile
{
    public int LaunchX { get; set; }
    public int LaunchY { get; set; }
    public int ImpactX { get; set; }
    public int ImpactY { get; set; }
    public int Power { get; set; }
    public int Direction { get; set; }
    public bool HitTarget { get; set; }
    public int OwnerOrder { get; set; } // 0 ou 1 pour identifier le joueur
}

public class Save
{
   private List<Point> _clickedPoints = new List<Point>();
   private List<int> _pointOwners = new List<int>();
   private List<SavedMissile> _missiles = new List<SavedMissile>();
   private static string path = "C:/Users/ACER/Documents/L2/C#/point/save/save.txt";

/// Propriété pour la gestion de la sauvegarde des points
    public List<Point> ClickedPoints
    {
        get { return _clickedPoints; }
        set
        {
            if (value != null)
            {
                _clickedPoints = value;
            }
        }
    }

    /// Propriété pour la gestion des propriétaires des points
    public List<int> PointOwners
    {
        get { return _pointOwners; }
        set
        {
            if (value != null)
            {
                _pointOwners = value;
            }
        }
    }

    /// Propriété pour la gestion de la sauvegarde des missiles
    public List<SavedMissile> Missiles
    {
        get { return _missiles; }
        set
        {
            if (value != null)
            {
                _missiles = value;
            }
        }
    }

    // Constructeur
    public Save(List<Point> points, List<int> owners)
    {
        ClickedPoints = points;
        PointOwners = owners;
        _missiles = new List<SavedMissile>();
    }

    // Constructeur par défaut
    public Save()
    {
        _clickedPoints = new List<Point>();
        _pointOwners = new List<int>();
        _missiles = new List<SavedMissile>();
    }

    /// <summary>
    /// Ajoute un missile à la liste de sauvegarde
    /// </summary>
    public void AddMissile(int launchX, int launchY, int impactX, int impactY,
                          int power, int direction, bool hitTarget, int ownerOrder)
    {
        _missiles.Add(new SavedMissile
        {
            LaunchX = launchX,
            LaunchY = launchY,
            ImpactX = impactX,
            ImpactY = impactY,
            Power = power,
            Direction = direction,
            HitTarget = hitTarget,
            OwnerOrder = ownerOrder
        });
    }

    /// <summary>
    /// Ajoute tous les missiles de deux joueurs à la sauvegarde
    /// Sauvegarde TOUS les missiles (en vol et détruits) pour conserver l'état complet
    /// </summary>
    public void AddMissilesFromPlayers(Player player1, Player player2)
    {
        // Missiles du joueur 1 - TOUS les missiles
        foreach (var missile in player1.Missiles)
        {
            Point impact = missile.GetCurrentPosition();
            AddMissile(missile.Position.X, missile.Position.Y,
                      impact.X, impact.Y,
                      missile.Power, missile.Direction,
                      missile.HitTarget, player1.Order);
        }

        // Missiles du joueur 2 - TOUS les missiles
        foreach (var missile in player2.Missiles)
        {
            Point impact = missile.GetCurrentPosition();
            AddMissile(missile.Position.X, missile.Position.Y,
                      impact.X, impact.Y,
                      missile.Power, missile.Direction,
                      missile.HitTarget, player2.Order);
        }
    }

/// sauvegarde des donnés de jeux (points + missiles)
    public void Write(string player1, string player2)
    {
        StringBuilder data = new StringBuilder();

        // Ligne 1 : Noms des joueurs
        data.AppendFormat("{0} - {1}\n", player1, player2);

        // Section POINTS
        data.AppendLine("POINTS");
        for (int i = 0; i < _clickedPoints.Count; i++)
        {
            Point point = _clickedPoints[i];
            int owner = (i < _pointOwners.Count) ? _pointOwners[i] : (i % 2);
            data.AppendFormat("({0}, {1}, {2})\n", point.X, point.Y, owner);
        }

        // Section MISSILES
        data.AppendLine("MISSILES");
        foreach (var missile in _missiles)
        {
            // Format: launchX,launchY,impactX,impactY,power,direction,hitTarget,ownerOrder
            data.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}\n",
                missile.LaunchX, missile.LaunchY,
                missile.ImpactX, missile.ImpactY,
                missile.Power, missile.Direction,
                missile.HitTarget ? 1 : 0,
                missile.OwnerOrder);
        }

        string result = data.ToString();

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            byte[] bit = System.Text.Encoding.UTF8.GetBytes(result);
            fs.Write(bit, 0, bit.Length);
        }
    }

/// lecture des données du fichier de sauvegarde pour avoir la liste des points
    public List<Point> getPointList()
    {
        List<Point> obj = new List<Point>();
        _pointOwners.Clear(); // Effacer les anciens propriétaires

        if (!File.Exists(path))
            return obj;

        using (StreamReader reader = new StreamReader(path))
        {
            string ligne;
            bool inPointsSection = false;

            while ((ligne = reader.ReadLine()) != null)
            {
                // Détecter la section POINTS
                if (ligne.Trim() == "POINTS")
                {
                    inPointsSection = true;
                    continue;
                }

                // Arrêter si on arrive à la section MISSILES
                if (ligne.Trim() == "MISSILES")
                {
                    break;
                }

                // Lire uniquement dans la section POINTS
                if (inPointsSection && ligne.Contains('('))
                {
                    string cleanedInput = ligne.Trim('(', ')').Trim();
                    string[] parts = cleanedInput.Split(',');

                    if (parts.Length >= 2)
                    {
                        try
                        {
                            int x = int.Parse(parts[0].Trim());
                            int y = int.Parse(parts[1].Trim());
                            int owner = (parts.Length >= 3) ? int.Parse(parts[2].Trim()) : 0;

                            obj.Add(new Point(x, y));
                            _pointOwners.Add(owner);
                        }
                        catch
                        {
                            // Ignorer les lignes mal formées
                        }
                    }
                }
            }
        }
        return obj;
    }

    /// <summary>
    /// Retourne la liste des propriétaires chargés depuis le fichier
    /// Doit être appelé après getPointList()
    /// </summary>
    public List<int> getPointOwners()
    {
        return _pointOwners;
    }

    /// <summary>
    /// Lecture des missiles depuis le fichier de sauvegarde
    /// </summary>
    public List<SavedMissile> getMissileList()
    {
        List<SavedMissile> obj = new List<SavedMissile>();

        if (!File.Exists(path))
            return obj;

        using (StreamReader reader = new StreamReader(path))
        {
            string ligne;
            bool inMissilesSection = false;

            while ((ligne = reader.ReadLine()) != null)
            {
                // Détecter la section MISSILES
                if (ligne.Trim() == "MISSILES")
                {
                    inMissilesSection = true;
                    continue;
                }

                // Lire les missiles
                if (inMissilesSection && !string.IsNullOrWhiteSpace(ligne))
                {
                    string[] parts = ligne.Split(',');

                    if (parts.Length == 8)
                    {
                        try
                        {
                            obj.Add(new SavedMissile
                            {
                                LaunchX = int.Parse(parts[0].Trim()),
                                LaunchY = int.Parse(parts[1].Trim()),
                                ImpactX = int.Parse(parts[2].Trim()),
                                ImpactY = int.Parse(parts[3].Trim()),
                                Power = int.Parse(parts[4].Trim()),
                                Direction = int.Parse(parts[5].Trim()),
                                HitTarget = int.Parse(parts[6].Trim()) == 1,
                                OwnerOrder = int.Parse(parts[7].Trim())
                            });
                        }
                        catch
                        {
                            // Ignorer les lignes mal formées
                        }
                    }
                }
            }
        }
        return obj;
    }

/// lecture des données du fichier de sauvegarde pour avoir le nom des joueurs par partie
    public static List<string[]> getPlayerList()
    {
        List<string[]> obj = new List<string[]>();

        if (!File.Exists(path))
            return obj;

        using (StreamReader reader = new StreamReader(path))
        {
            string ligne;

            while ((ligne = reader.ReadLine()) != null)
            {
                if (!ligne.Contains('-'))
                {
                    continue;
                }

                string cleanedInput = ligne.Trim();
                string[] parts = cleanedInput.Split('-');

                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Trim();
                }

                obj.Add(parts);
            }
        }
        return obj;
    }
}
