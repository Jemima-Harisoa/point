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
    /// Récupère la configuration gagnante (5 points alignés) et dessine :
    /// - Pour une ligne droite (vertical/horizontal) : une seule ligne du premier au dernier point
    /// - Pour une forme en L : deux lignes perpendiculaires passant par le point d'intersection
    /// OPTIMISÉ : Bénéficie du cache de Line.Liste() et double buffering
    /// </summary>
    public void paint(object sender, PaintEventArgs paint){
        Graphics graph = paint.Graphics;
        graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        List<Point> ligne = line.Liste(5);
        if(ligne.Count == 0) return; // Aucune ligne gagnante trouvée

        Pen Pen = new Pen(color,5);

        // Vérifier si c'est une ligne droite (verticale ou horizontale)
        if(Line.VerticalOrHorizontal(ligne))
        {
            // Pour une ligne droite : dessiner du premier au dernier point
            Point premier = ligne[0];
            Point dernier = ligne[ligne.Count - 1];
            graph.DrawLine(Pen, premier.X, premier.Y, dernier.X, dernier.Y);
        }
        else
        {
            // Pour une forme en L : dessiner les deux lignes perpendiculaires
            Point intersection = line.Intersection(ligne);
            Point ExtremityVertical = line.Extremity( ligne, intersection, true);
            Point ExtremityHorizontal = line.Extremity( ligne, intersection, false);
            graph.DrawLine(Pen, intersection.X, intersection.Y, ExtremityVertical.X, ExtremityVertical.Y);
            graph.DrawLine(Pen, intersection.X, intersection.Y, ExtremityHorizontal.X, ExtremityHorizontal.Y);
        }
    }
    /// <summary>
    /// Suggère le meilleur point à jouer pour un joueur selon une stratégie intelligente.
    /// Cherche un point qui :
    /// - Complète une configuration L-shape (pour nombre=4) en maintenant l'intersection unique
    /// - Ou étend une ligne verticale/horizontale (pour nombre <= 4)
    /// Retourne le premier point valide trouvé, ou Point() vide si aucun coup optimal.
    /// </summary>
    /// <param name="player">Le joueur pour lequel suggérer un coup</param>
    /// <param name="nombre">Le nombre de points nécessaires dans la formation (3, 4, ou 5)</param>
    /// <returns>Le point optimal à jouer, ou Point() vide si aucun coup ne peut être trouvé</returns>
    public static Point Suggest(Player player, int nombre){
        Line l = player.line;
        List<Point> suggestion =  l.Combine(l.Suggestion(nombre),  l.SuggestionL(nombre));
        // Console.WriteLine("liste des point de suggestion :");
        // foreach (var item in suggestion)
        // {
        //     Console.Write($"({item.X}, {item.Y}) ");
        // }
        // Console.WriteLine();

        List<List<Point>> points = player.line.LShapeLine();
        foreach (var index in player.line.line(nombre, points))
        {
            List<Point> points1 = points[index];
            Point inter = player.line.Intersection(points1);
            int interNumber = player.line.Intersect(points1).Count;
            bool estVerticalOuHorizontal = Line.VerticalOrHorizontal(points1);
            foreach (var item in suggestion)
            {
                if(!points1.Contains(item)) points1.Add(item);
                int newInterNumber = player.line.Intersect(points1).Count;
                Point inter1 = player.line.Intersection(points1);
                bool result = !Line.VerticalOrHorizontal(points1);
                // Voir si le point d'intersection reste unique donc unchanged
                if(nombre == 4 &&  result  && inter1.X == inter.X && inter1.Y == inter.Y && newInterNumber == interNumber){
                    if (!Line.ClickedPoints.Contains(item)){
                        Console.WriteLine($" point suggérée ({item.X}, {item.Y}) ");
                        return item;
                    }
                }
                if(points1.Contains(item))points1.Remove(item);
            }
        
        }

         points = player.line.getLineList();
        foreach (var index in player.line.line(nombre, points))
        {
            List<Point> points1 = points[index];
          
            bool estVerticalOuHorizontal = Line.VerticalOrHorizontal(points1);
            foreach (var item in suggestion)
            {
                if(!points1.Contains(item)) points1.Add(item);
               
                // Voir si le point d'intersection reste unique donc unchanged
                if((estVerticalOuHorizontal || !estVerticalOuHorizontal) && nombre <= 4) {
                    if (!Line.ClickedPoints.Contains(item)){
                    Console.WriteLine($" point suggérée ({item.X}, {item.Y}) ");
                        return item;
                    }
                }
                if(points1.Contains(item))points1.Remove(item);
            }
        
        }
        return new Point();
    }

    /// <summary>
    /// Vérifie si le joueur peut remporter la partie au prochain coup.
    /// Retourne vrai si le joueur possède une formation de 4 points alignés
    /// (un coup de plus complèterait 5 points = victoire).
    /// </summary>
    /// <returns>true si le joueur a une configuration 4-gagnante potentielle</returns>
    public bool CanWin(){
        if(has(4)) return true;
        else return false;
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
