using System.Net;

namespace point;

public class Player
{
    public string nom;
    private int _order;
    public bool isPlaying;
    public int Order{
        get{
            return _order;
        }
        set{
            _order = value % 2;
            ///if(line != null)line.Type = _order; 
        }
    }

    public Line line ;

    public List<List<Point>> Points{
        get{
            //return line.getLineList();
            return line.LShapeLine();
        }
    }
    public Color color;

    public Player(string Nom, int order, Color cl){
        nom = Nom;
        Order = order;
        color = cl;
        line = new Line(order); 
    }
    /// <summary>
    /// Dessine les lignes gagnantes du joueur sur le terrain.
    /// La logique simplifie le rendu en trois catégories:
    ///
    /// 1. LIGNE DROITE (vertical/horizontal/diagonal):
    ///    Dessine une seule ligne du premier au dernier point
    ///
    /// 2. DIAGONALE:
    ///    Dessine une seule ligne du premier au dernier point
    ///
    /// 3. FORMATION L:
    ///    Dessine deux lignes perpendiculaires passant par l'intersection
    ///
    /// OPTIMISÉ: Bénéficie du cache de Line.Liste() et du double buffering.
    /// </summary>
    public void paint(object sender, PaintEventArgs paint){
        Graphics graph = paint.Graphics;
        graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Récupérer la ligne gagnante (GameConfig.PointsToWin = 5 par défaut)
        List<Point> ligne = line.Liste(GameConfig.PointsToWin);
        if(ligne.Count == 0) return;

        Pen Pen = new Pen(color, 5);

        // Cas 1: Ligne droite (vertical/horizontal)
        // Cas 2: Diagonale (géré de la même façon que la ligne droite)
        if(Line.VerticalOrHorizontal(ligne) || Line.isDiagonal(ligne))
        {
            // Tracer une seule ligne du premier au dernier point
            Point premier = ligne[0];
            Point dernier = ligne[ligne.Count - 1];
            graph.DrawLine(Pen, premier.X, premier.Y, dernier.X, dernier.Y);
        }
        else
        {
            // Cas 3: Formation L
            // Identifier le point d'intersection et tracer deux lignes perpendiculaires
            Point intersection = line.Intersection(ligne);
            Point extremityVertical = line.Extremity(ligne, intersection, true);
            Point extremityHorizontal = line.Extremity(ligne, intersection, false);

            // Tracer la ligne verticale
            graph.DrawLine(Pen, intersection.X, intersection.Y,
                          extremityVertical.X, extremityVertical.Y);

            // Tracer la ligne horizontale
            graph.DrawLine(Pen, intersection.X, intersection.Y,
                          extremityHorizontal.X, extremityHorizontal.Y);
        }
    }
    /// <summary>
    /// Suggère le meilleur point à jouer pour un joueur selon une stratégie intelligente.
    /// La stratégie se décompose en deux phases:
    ///
    /// PHASE 1 - Formations en L (nombre=4):
    /// Cette phase cherche à compléter une formation L-shape en 4 points.
    /// Pour éviter que le point n'aplatisse la formation en ligne droite,
    /// on vérifie que l'intersection reste unique et inchangée.
    ///
    /// PHASE 2 - Lignes droites (nombre <= 4):
    /// Cette phase cherche à étendre une ligne droite (vertical/horizontal/diagonale).
    /// Tout point qui complète la ligne est accepté.
    /// </summary>
    /// <param name="player">Le joueur pour lequel suggérer un coup</param>
    /// <param name="nombre">Le nombre de points nécessaires: 3 (trois points),
    ///                       4 (formation gagnante), ou 5 (victoire)</param>
    /// <returns>Le point optimal à jouer, ou Point() vide si aucun coup ne peut être trouvé</returns>
    public static Point Suggest(Player player, int nombre){
        // Récupérer tous les points de suggestion candidats
        // Combine les suggestions des deux types de formations (L-shape et droites)
        Line l = player.line;
        List<Point> suggestion = l.Combine(l.Suggestion(nombre), l.SuggestionL(nombre));

        // DEBUG: Afficher la liste des suggestions (à décommenter pour debug)
        // Console.WriteLine($"[SUGGEST] Recherche pour {nombre} points - {suggestion.Count} suggestions trouvées");
        // foreach (var item in suggestion)
        // {
        //     Console.Write($"({item.X}, {item.Y}) ");
        // }
        // Console.WriteLine();

        // PHASE 1: Valider les suggestions pour formations L-shape (nombre=4)
        // Cherche les points qui complètent une formation L sans la transformer en ligne droite
        List<List<Point>> lshapePoints = player.line.LShapeLine();
        foreach (var index in player.line.line(nombre, lshapePoints))
        {
            List<Point> currentLine = lshapePoints[index];
            Point originalIntersection = player.line.Intersection(currentLine);
            int originalIntersectionCount = player.line.Intersect(currentLine).Count;

            // Tester chaque point de suggestion
            foreach (var suggestedPoint in suggestion)
            {
                // Ajouter temporairement le point suggéré
                if(!currentLine.Contains(suggestedPoint)) currentLine.Add(suggestedPoint);

                // Vérifier si le point est déjà placé
                bool pointAlreadyPlaced = Line.ClickedPoints.Contains(suggestedPoint);

                // Analyser la nouvelle formation après l'ajout du point
                int newIntersectionCount = player.line.Intersect(currentLine).Count;
                Point newIntersection = player.line.Intersection(currentLine);
                bool stillLShape = !Line.VerticalOrHorizontal(currentLine);

                // CRITÈRE POUR NOMBRE=4:
                // Le point est bon si:
                // 1. C'est une formation L (pas une ligne droite)
                // 2. L'intersection reste la même position
                // 3. Le nombre d'intersections ne change pas
                // 4. Le point n'est pas déjà placé
                if(nombre == 4 && stillLShape &&
                   newIntersection.X == originalIntersection.X &&
                   newIntersection.Y == originalIntersection.Y &&
                   newIntersectionCount == originalIntersectionCount &&
                   !pointAlreadyPlaced)
                {
                    Console.WriteLine($"[SUGGEST] Point L-shape suggéré: ({suggestedPoint.X}, {suggestedPoint.Y})");
                    if(currentLine.Contains(suggestedPoint)) currentLine.Remove(suggestedPoint);
                    return suggestedPoint;
                }

                // Enlever le point pour tester le suivant
                if(currentLine.Contains(suggestedPoint)) currentLine.Remove(suggestedPoint);
            }
        }

        // PHASE 2: Valider les suggestions pour lignes droites
        // Cherche les points qui complètent une ligne droite (verticale/horizontale/diagonale)
        List<List<Point>> straightPoints = player.line.getLineList();
        foreach (var index in player.line.line(nombre, straightPoints))
        {
            List<Point> currentLine = straightPoints[index];
            bool isLinearFormation = Line.VerticalOrHorizontal(currentLine);

            // Tester chaque point de suggestion
            foreach (var suggestedPoint in suggestion)
            {
                // Ajouter temporairement le point suggéré
                if(!currentLine.Contains(suggestedPoint)) currentLine.Add(suggestedPoint);

                // CRITÈRE POUR LIGNES DROITES (nombre <= 4):
                // Le point est bon s'il n'est pas déjà placé
                // (La formation peut être droite OU L-shape, on accepte les deux)
                if(!Line.ClickedPoints.Contains(suggestedPoint))
                {
                    Console.WriteLine($"[SUGGEST] Point ligne suggéré: ({suggestedPoint.X}, {suggestedPoint.Y})");
                    if(currentLine.Contains(suggestedPoint)) currentLine.Remove(suggestedPoint);
                    return suggestedPoint;
                }

                // Enlever le point pour tester le suivant
                if(currentLine.Contains(suggestedPoint)) currentLine.Remove(suggestedPoint);
            }
        }

        // Aucun point optimal trouvé
        return new Point();
    }

    /// <summary>
    /// Vérifie si le joueur peut remporter la partie au prochain coup.
    /// Retourne vrai si le joueur possède une formation complète avec
    /// (GameConfig.PointsForCanWin) points alignés.
    /// Par défaut: 4 points (un coup avant la victoire avec 5 points)
    /// </summary>
    /// <returns>true si le joueur peut gagner au prochain coup</returns>
    public bool CanWin(){
        return has(GameConfig.PointsForCanWin);
    }

    /// <summary>
    /// Vérifie si le joueur possède une formation avec un nombre donné de points alignés.
    /// Recherche dans deux types de configurations :
    /// - Les lignes en L (LShapeLine) : deux lignes perpendiculaires se croisant
    /// - Les lignes droites (getLineList) : points alignés verticalement ou horizontalement
    /// </summary>
    /// <param name="nombre">Le nombre de points à vérifier (3, 4, ou 5)</param>
    /// <returns>true si le joueur possède une formation avec au moins 'nombre' points</returns>
    public bool has(int nombre){
        if(line.line(nombre, line.LShapeLine()).Count > 0 ) return true;
        if(line.line(nombre, line.getLineList()).Count > 0 ) return true;
        else return false;
    }

}
