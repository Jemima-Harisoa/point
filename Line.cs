using System;
using System.Collections.Generic;
using System.Linq;

namespace point
{
    /// <summary>
    /// Classe gérant la détection des alignements de points et les calculs géométriques.
    /// Responsable de :
    /// - Détecter les points d'intersection (croisements) entre lignes
    /// - Former des configurations L-shape et verticales/horizontales
    /// - Suggérer les meilleurs coups stratégiques pour l'IA
    /// - Gérer les limites du terrain de jeu
    /// </summary>
    public class Line
    {
        ///Classe de gestion de tracés des lignes
        private  static List<Point> _clickedPoints = new List<Point>();
        private int step = 50;
        private int _type;
        private static List<Point> _limit = new List<Point>();
         // Constructeur
        public Line(int type)
        {
            Type = type;
        }

        public Line() { }
        public int Type
        {
            get { return _type; }
            set { _type = value % 2; }
        }
                public static List<Point> ClickedPoints
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



        /// <summary>
        /// Vérifie quels points d'une liste donnée sont des points d'intersection.
        /// Un point d'intersection est un endroit où deux ou plusieurs lignes se croisent.
        /// </summary>
        /// <param name="ligne">Liste de points à analyser</param>
        /// <returns>Liste des points d'intersection trouvés dans la liste fournie</returns>
        public List<Point> Intersect (List<Point> ligne){
            List<Point> Intersection = new List<Point>();
            foreach (var item in Intersect())
            {
                if(ligne.Contains(item)) Intersection.Add(item);
            }
            return Intersection;
        }
        /// <summary>
        /// Génère des points de suggestion pour les configurations en L-shape.
        /// Formules pour chaque point extrême vertical et horizontal :
        /// - Explore 2 orientations (±1 selon l'axe X/Y)
        /// - Applique l'Equation pour obtenir les points suivants parallèles
        /// - Ajoute tous les points candidats qui ne sont pas déjà utilisés
        /// </summary>
        /// <param name="nombre">Nombre de points dans la configuration L (3, 4, ou 5)</param>
        /// <returns>Liste de points de suggestion pour des formations en L</returns>
        public List<Point> SuggestionL(int nombre){
        List<Point> Suggestion = new List<Point>();
        List<List<Point>> LineList = LShapeLine();
        List<int> indice  = line(nombre, LineList);
        foreach (var index in indice)
        {
            List<Point> points = LineList[index];
            Point intersection = Intersection(points);
            bool isVertical = true; 
            do {  /// pour obtenir la valeur du point dans une orientation   
                Point externe =  Extremity(LineList[index],intersection,isVertical);
                for (int i = 1; i < 3; i++) // on ne considere configuration 1 ou 2  
                {
                    int orientation  = i;
                    Point ajout  = Equation(orientation, externe); // on recupère les points externe suivant l'orientation de la ligne
                    if (!Suggestion.Contains(ajout) && !points.Contains(ajout) && ajout.X > 0 && ajout.Y > 0  ) Suggestion.Add(ajout); 
                    orientation = -orientation; // Changer l'orientation
                    ajout = Equation(orientation, externe);
                    if (!Suggestion.Contains(ajout) && !points.Contains(ajout) && ajout.X > 0 && ajout.Y > 0) Suggestion.Add(ajout);  
                }
                isVertical = !isVertical;
               
                if(isVertical) break;   // quand l'orientation redevient true on arrête la boucle 
            }while (true);     
        }
        return  Suggestion;
    }

        /// <summary>
        /// Génère des points de suggestion pour les lignes verticales/horizontales.
        /// Pour chaque ligne droite trouvée :
        /// - Identifie l'orientation (verticale ou horizontale)
        /// - Cherche les points extrêmes et les points adjacents
        /// - Suggère des points qui poursuivent la ligne dans les deux directions
        /// La logique évite les points déjà occupés.
        /// </summary>
        /// <param name="nombre">Nombre de points dans la ligne (3, 4, ou 5)</param>
        /// <returns>Liste de points de suggestion pour des lignes droites</returns>
        public List<Point> Suggestion(int nombre){
        List<Point> Suggestion = new List<Point>();
        List<List<Point>> LineList = getLineList();
        List<int> indice  = line(nombre, LineList);
        foreach (var index in indice)
        {
            int count = 0 ; 
            if(!VerticalOrHorizontal(LineList[index])){
                continue; // ignorer le reste et passer a la suite => cause en ne prend que vertical / horizontal pour les lignes en L  
            }
            List<Point> points = LineList[index];
         
            do {  /// pour obtenir la valeur du point dans la même orientation   
                bool isVertical = isHorizontal(points) ? false : true; 
                Point externe =  Extremity(LineList[index],LineList[index][count],isVertical);
                for (int i = 1; i < 3; i++) // on ne considere configuration 1 ou 2  
                {
                    int orientation  = i;
                    Point ajout  = Equation(orientation, externe); // on recupère les points externe suivant l'orientation de la ligne
                    if (!Suggestion.Contains(ajout) && !points.Contains(ajout)) Suggestion.Add(ajout); 
                        orientation = -orientation; // Changer l'orientation
                    ajout = Equation(orientation, externe);
                    if (!Suggestion.Contains(ajout) && !points.Contains(ajout)) Suggestion.Add(ajout);  
                }
                count = count + points.Count -  1;  // passer directement au deriner element apres le premier 
            }while (count < points.Count);     

        }
        return  Suggestion;
    }

        /// <summary>
        /// Identifie le point d'intersection d'une liste de points donnée.
        /// Un point d'intersection est celui par lequel passent des points dans au moins 2 directions différentes.
        /// Vérifie pour chaque point d'intersection candidat :
        /// - Compte tous les points atteignables dans les 4 directions (vertical/horizontal)
        /// - Si le compte égale le nombre total, c'est le vrai point d'intersection
        /// </summary>
        /// <param name="points">Liste de points formant une configuration (potentiellement en L)</param>
        /// <returns>Le point d'intersection, ou Point() vide si aucun trouvé</returns>
        public Point Intersection(List<Point> points){
        List<Point> pListe = new List<Point>();
        foreach (var item in Intersect())
        {
            if(points.Contains(item)) pListe.Add(item); 
        }
        foreach( var point in pListe){
                List<List<Point>> part =  new List<List<Point>> ();
                int count = 1;
            for( int i = 1;  i < 3 ; i++){
                Point Extremity = new Point();
                List<Point> PointList = new List<Point>();

                int orientation = i;
                Extremity = Equation(orientation , point);
                if (!points.Contains(Extremity))
                {
                    orientation = -orientation; // Changer de direction
                }
                Extremity = Equation(orientation , point);

                while (true)
                {
                    if(!PointList.Contains(Extremity)) PointList.Add(Extremity);

                    if (points.Contains(Equation(orientation, Extremity)))
                    {
                        Extremity = Equation(orientation, Extremity);
                    }
                    else break;
                }         
                part.Add(PointList);
            }

            foreach (var item in part)
            {
                count = count + item.Count;
            }
            if(count == points.Count) return point;
        }
        return new Point();
    }
        /// <summary>
        /// Récupère la liste de points formant une configuration gagnante (exactement 'nombre' points alignés).
        /// Parcourt tous les points d'intersection et cherche une configuation où :
        /// - Le nombre de points égale le nombre recherché
        /// - Il existe un vrai point d'intersection (non nul)
        /// </summary>
        /// <param name="nombre">Le nombre exact de points à trouver (typiquement 5 pour la victoire)</param>
        /// <returns>Liste de points formant la configuration, ou liste vide si non trouvée</returns>
        public List<Point> Liste(int nombre){
        List<List<Point>> Liste = new List<List<Point>>();

        foreach (var item in Intersect())
        {
            List<List<Point>> list =  Intersection(item);
            List<int> points = line(nombre, list);
            foreach (var val in  list)
            {
                if(val.Count == nombre && Intersection(val).X != 0 && Intersection(val).Y != 0 ) return val;
            }

        }
        return new List<Point>();   
    }

        /// <summary>
        /// Filtre une liste de listes de points et retourne les indices des listes ayant exactement 'nombre' éléments.
        /// Utile pour identifier rapidement les formations d'une taille spécifique.
        /// </summary>
        /// <param name="nombre">Le nombre de points recherchés dans chaque sous-liste</param>
        /// <param name="linelist">Liste de listes de points à filtrer</param>
        /// <returns>Liste des indices (dans linelist) des listes ayant exactement 'nombre' éléments</returns>
        public List<int> line(int nombre, List<List<Point>> linelist){/// voir les tables qui contiennent nombre élément dans un tableau de liste de point pour adapter avec les line en l ou non
            List<int> line = new List<int>();
            int i = 0;
            foreach (var item in  linelist)
            {
                if(item.Count == nombre){
                    line.Add(i);
                }
                i++; 
            }
            return line;
        }
        /// <summary>
        /// Trouve toutes les lignes en L-shape (formations perpendiculaires) qui passent par un point d'intersection spécifique.
        /// Une ligne en L-shape est une combinaison de deux directions perpendiculaires se croisant à un angle droit.
        /// </summary>
        /// <param name="points">Le point d'intersection à analyser</param>
        /// <returns>Liste de toutes les lignes en L-shape passant par ce point</returns>
        public List<List<Point>> Intersection(Point points){
        List<List<Point>> Interction = new List<List<Point>>();

        foreach (var item in LShapeLine())
        {
            if(item.Contains(points) && !Interction.Contains(item)) Interction.Add(item); 
        }
        return  Interction;
    }
        /// <summary>
        /// Trouve le point extrême d'une ligne dans une orientation donnée (vertical ou horizontal).
        /// Trace une direction depuis le point d'intersection jusqu'au dernier point présent dans la liste.
        /// Essaie d'abord une direction positive, puis l'inverse si aucun point ne correspond.
        /// </summary>
        /// <param name="points">Liste de points de la ligne</param>
        /// <param name="intersection">Le point d'intersection d'où partir</param>
        /// <param name="isVertical">true pour direction verticale (Y), false pour horizontale (X)</param>
        /// <returns>Le point extrême de la ligne dans cette direction</returns>
        public Point Extremity(List<Point> points, Point intersection, bool isVertical){ /// fonction sugéré point en L pour une ligne donne 
        int orientation = isVertical ? 1 : 2;
        Point Extremity = new Point();
        Extremity = Equation(orientation, intersection);
        if (!points.Contains(Extremity))
        {
            orientation = -orientation; // Changer de direction
        }
        Extremity = Equation(orientation, intersection);
         while (true)
        {
             if (points.Contains(Equation(orientation, Extremity)))
            {
                Extremity = Equation(orientation, Extremity);
            }
            else break;
        }         
        return Extremity;
    }

        /// <summary>
        /// Génère toutes les lignes en L-shape (formations perpendiculaires) actuelles sur le plateau.
        /// Processus :
        /// 1. Identifie tous les points d'intersection du plateau
        /// 2. Pour chaque intersection, trouve les lignes verticales/horizontales qui y passent
        /// 3. Combine ces lignes perpendiculaires pour créer des formations L-shape
        /// </summary>
        /// <returns>Liste de toutes les configurations L-shape possibles</returns>
        public List<List<Point>> LShapeLine(){ //liste des lignes en L  
        List<List<Point>> LineL = new List<List<Point>>();
        List<List<Point>> Line = new List<List<Point>>();
        List<Point> Interction = Intersect();
        foreach (var intersect in Interction) // voir chaque point d'intersection
        {
            
            foreach (var item in LineListByIntersection(intersect)) // recherche les lignes qui se croise en ces points
            {
                if(VerticalOrHorizontal(item) && !Line.Contains(item)) Line.Add(item); // si les lignes sont vertical ou horizontal on les prends
            }
            
            foreach (var LineWithIntersection in Construct(intersect, Line)) // on construit les intersections sur les angles droit 
            {
                if(!VerticalOrHorizontal(LineWithIntersection) && !LineL.Contains(LineWithIntersection)){
                    LineL.Add(LineWithIntersection);
                }
            }
        }   
        return LineL;
    }
        /// <summary>
        /// Détermine si une liste de points forme une ligne verticale ou horizontale purem (pas de diagonale).
        /// </summary>
        /// <param name="points">Liste de points à vérifier</param>
        /// <returns>true si tous les points partagent le même X (vertical) ou Y (horizontal)</returns>
        public static bool VerticalOrHorizontal(List<Point> points){
            if(isHorizontal(points) || isVertical(points)) return true;
            return false;
        }
        /// <summary>
        /// Vérifie si une liste de points forme une ligne horizontale purem (tous même Y).
        /// </summary>
        /// <param name="points">Liste de points à tester</param>
        /// <returns>true si tous les points ont la même coordonnée Y</returns>
        public static bool isHorizontal(List<Point> points){
            Point point1 = points[0];
            int count = 0;
            foreach (var point in points)
            {
                if(point1.Y == point.Y ) count++;
            }
            if(count == points.Count ) return true;
            return false;
        }
        /// <summary>
        /// Vérifie si une liste de points forme une ligne verticale purem (tous même X).
        /// </summary>
        /// <param name="points">Liste de points à tester</param>
        /// <returns>true si tous les points ont la même coordonnée X</returns>
        public static bool isVertical(List<Point> points){
            Point point1 = points[0];
            int count = 0;
            foreach (var point in points)
            {
                if(point1.X == point.X ) count++;
            }
            if(count == points.Count ) return true;
            return false;
        }
        /// <summary>
        /// Construit toutes les formes L-shape possibles en croisant les lignes perpendiculaires.
        /// Processus :
        /// 1. Divise chaque ligne droite au point d'intersection
        /// 2. Combine tous les segments deux par deux pour former des L-shapes
        /// 3. Retourne uniquement les L-shapes véritables (non-verticales et non-horizontales)
        /// </summary>
        /// <param name="intersect">Le point d'intersection central</param>
        /// <param name="listLine">Les lignes verticales/horizontales à croiser</param>
        /// <returns>Liste de toutes les L-shapes créées par intersection</returns>
        public List<List<Point>> Construct(Point intersect, List<List<Point>> listLine){ // reconstitué les lignes
            List<List<Point>> Construct = new List<List<Point>>();
            List<List<Point>> list = new List<List<Point>>();
            List<List<Point>> LineList = listLine;
            foreach (var Line in  LineList){ //on sépare chaque ligne en deux au niveau du point d'intersection   
                foreach (var item in divide(Line, intersect)){
                    list.Add(item);
                }                      
            }
            for (int i = 0; i < list.Count; i++)
            {
                List<Point> line = new List<Point>( list[i]);
                for (int j = i + 1;  j < list.Count; j++)
                {
                    if(!Construct.Contains(Combine(line, list[j]))) Construct.Add(new List<Point>(Combine(line, list[j])));
                }    
              
            }
           
            return Construct;
        }
        /// <summary>
        /// Fusionne deux listes de points en une seule, évitant les doublons.
        /// </summary>
        /// <param name="list1">Première liste de points</param>
        /// <param name="list2">Deuxième liste de points</param>
        /// <returns>Nouvelle liste contenant l'union de list1 et list2</returns>
        public List<Point> Combine(List<Point> list1, List<Point> list2){ /// prendre 2 liste et les reunir en une seul
            List<Point> Combine  = new List<Point>(list1);
            foreach (Point point in list2){
                if(!Combine .Contains(point)) Combine.Add(point);
            }
            return Combine ;
        }
        /// <summary>
        /// Divise une ligne en segments au point d'intersection spécifié.
        /// Crée plusieurs petites listes : chaque segment va du début jusqu'au point d'intersection, 
        /// puis à nouveau depuis le point d'intersection jusqu'au prochain segment.
        /// </summary>
        /// <param name="points">La liste de points formant la ligne à diviser</param>
        /// <param name="intersect">Le point d'intersection où diviser la ligne</param>
        /// <returns>Liste de segments divisés au point d'intersection</returns>
        public List<List<Point>> divide(List<Point> points, Point intersect){
            Point item; int i = 0;
            List<Point> Line1 = new List<Point>();
            List<List<Point>> Part = new List<List<Point>>();
            do{
                item =  points[i];
                Line1.Add(item);
                if(item == intersect){ 
                    if(Line1.Count > 1) Part.Add(new List<Point>(Line1));
                    Line1.Clear();
                    Line1.Add(item);
                }
                i++;
            }while (points.Count > i);
            if(Line1.Count > 1) Part.Add(new List<Point>(Line1));
            return Part;  
        }
        /// <summary>
        /// Trouve toutes les lignes verticales/horizontales passant par un point d'intersection donné.
        /// </summary>
        /// <param name="intersect">Le point d'intersection à analyser</param>
        /// <returns>Liste de toutes les lignes droites contenant ce point</returns>
        public List<List<Point>> LineListByIntersection(Point intersect){ // pour un point d'intersection qui sont les ligne qui se croise
            List<List<Point>> LineList = new List<List<Point>>();
            List<List<Point>> SimpleLineList = getLineList();
            foreach (var item in  SimpleLineList)
            {
                if(item.Contains(intersect)) LineList.Add(item); 
            }
            return LineList;
        } 
        /// <summary>
        /// Identifie tous les points d'intersection sur le plateau de jeu.
        /// Un point d'intersection est un point où deux ou plusieurs lignes (parallélogrammes différentes) se croisent.
        /// Algorithme :
        /// - Compare chaque paire de lignes droites
        /// - Cherche les points qui apparaissent dans les deux lignes
        /// </summary>
        /// <returns>Liste de tous les points d'intersection du plateau</returns>
        public List<Point> Intersect (){
            List<Point> Intersect = new List<Point>();
            List<Point> Visited = new List<Point>();
            List<List<Point>> LineList = getLineList();   
            for (int i = 0; i < LineList.Count; i++)
            {
                List<Point> line = LineList[i];
                for (int j = i + 1;  j < LineList.Count; j++)
                {
                   foreach (var item in line)
                   {
                        if(LineList[j].Contains(item) && !Intersect.Contains(item)) Intersect.Add(item); 
                        if(!Visited.Contains(item)) Visited.Add(item); 
                   } 
                }    
            }
            return Intersect;
        }
        /// <summary>
        /// Vérifie si un point se trouve sur une limite du terrain de jeu.
        /// Les limites sont définies par les coins de la fenêtre (haut-gauche et bas-droite).
        /// Empêche les joueurs de placer des points sur les bords.
        /// </summary>
        /// <param name="p">Le point à vérifier</param>
        /// <returns>true si le point est sur une limite (frontière du terrain)</returns>
        public static bool isLimit(Point p ){
            if (_limit[0].X == p.X || _limit[0].Y == p.Y 
                ||  _limit[1].X == p.X || _limit[1].Y == p.Y)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Définit les limites du terrain de jeu à partir des dimensions du panneau.
        /// Les deux points de limite sont :
        /// - Coin supérieur gauche (0, 0)
        /// - Coin inférieur droit (Width, Height) du panneau
        /// </summary>
        /// <param name="cours">Le panneau dont les dimensions définissent les limites</param>
        /// <returns>Liste avec les deux points corners définissant la zone de jeu</returns>
        public static List<Point> Limit (Panel cours){
            _limit = new List<Point>(){
                new Point(0, 0), ///coin supérieur gauche 
                new Point(cours.Width, cours.Height)///coin inférieur droite
            };
            return _limit;
        }

    
        /// <summary>
        /// Récupère toutes les listes de points pouvant potentiellement former une ligne pour ce joueur.
        /// Utilise un algorithme de traçage :
        /// 1. Récupère tous les points de ce joueur (même couleur)
        /// 2. Pour chaque point, cherche les points les plus proches dans 4 directions différentes
        /// 3. Construit des listes de points alignés dans chaque direction
        /// 4. Remonte les listes en ordre (extrémités d'abord)
        /// Cette fonction est centrale pour détecter les formations gagnantes.
        /// </summary>
        /// <returns>Liste de toutes les configurations de lignes possibles pour ce joueur</returns>
        public List<List<Point>> getLineList()
        {
            List<List<Point>> getLineList = new List<List<Point>>();
            List<Point> PointsList = new List<Point>();
            List<Point> Points = getSameColor();
            if(Points.Count == 0) return getLineList;
            for (int i = 1; i <= 4; i++)
            {
                HashSet<int> Visited = new HashSet<int>(); // Utilisation d'un HashSet pour suivre les indices des points visités
                Point point = Points.ElementAt(0);
                PointsList.Add(point);
                Visited.Add(0);
                for (int j = 0; j < Points.Count; j++)
                {
                    if (Visited.Contains(j)) continue; // Passer au suivant si déjà visité

                    point = Points.ElementAt(j);
                    PointsList.Clear();
                    PointsList.Add(point);
                    Visited.Add(j);

                    int count = 0;
                    int currentDirection = i; // Garder la direction actuelle pour le traitement
                    int index;
                  

                    do
                    {
                        index = estProche(Equation(currentDirection, point));
                        if (index != -1)
                        {
                            point = Points.ElementAt(index);
                            if(!PointsList.Contains(point))PointsList.Add(point);
                            if(!Visited.Contains(index)) {
                                Visited.Add(index);
                            }
                        }
                        else
                        {
                            currentDirection = -currentDirection; // Changer de direction
                            count++;
                        }
                    } while (count < 2);
                    // Ordonner les points afin que les extrémités soient en premier et dernier

                    if(PointsList.Count >  1) {
                        PointsList = PointsList.OrderBy(p => p.X).ThenBy(p => p.Y).ToList(); 
                        getLineList.Add(new List<Point>(PointsList));
                    }
                }
            }
            
            return getLineList;
        }

        /// <summary>
        /// Identifie le prochain point non exploré pour la construction de lignes.
        /// Utilisé dans l'algorithme de traçage pour éviter de re-traiter les mêmes points.
        /// </summary>
        /// <param name="Visited">Liste contenant les indices des points déjà visités</param>
        /// <returns>L'indice du prochain point, ou -1 si tous ont été visités</returns>
        public int nextPoint(List<int> Visited)
        {
            int i = 0;
            foreach (Point item in getSameColor())
            {
                if (!isIn(i, Visited))
                {
                    return i;
                }
                i++;
            }
            return -1; // Retourner -1 si aucun point suivant n'est trouvé
        }

        /// <summary>
        /// Vérifie si un élément appartient à une liste donnée (helper générique).
        /// </summary>
        /// <param name="obj">L'élément à chercher</param>
        /// <param name="Objects">La liste dans laquelle chercher</param>
        /// <returns>true si l'élément est présent dans la liste</returns>
        private bool isIn<T>(T obj, List<T> Objects)
        {
            return Objects.Contains(obj);
        }

        /// <summary>
        /// Trouve l'indice du point le plus proche (dans l'une des 4 directions) d'un point donné.
        /// Les 4 directions sont : vertical (up/down), horizontal (left/right), et diagonales.
        /// Utilisé pour construire les lignes de points alignés.
        /// </summary>
        /// <param name="p">Le point de référence</param>
        /// <returns>L'indice du point le plus proche, ou -1 s'il n'existe pas</returns>
        private int estProche(Point p)
        {
            int i = 0;
            foreach (Point item in getSameColor())
            {
                if (p.X == item.X && p.Y == item.Y)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Récupère tous les points du joueur actuel, filtrés par couleur/identité.
        /// Les points appartenant au joueur possèdent un ordre pair ou impair selon le type du joueur.
        /// Exemple : Type 0 = tous les points à index pair (0, 2, 4, ...)
        ///          Type 1 = tous les points à index impair (1, 3, 5, ...)
        /// </summary>
        /// <returns>Liste des points appartenant uniquement à ce joueur</returns>
        private List<Point> getSameColor()
        {
            List<Point> getSameColor = new List<Point>();
            int rest = _type, i = 0;
            foreach (Point item in ClickedPoints)
            {
                if (rest == i % 2)
                {
                    getSameColor.Add(item);
                }
                i++;
            }
            return getSameColor;
        }

        /// <summary>
        /// Calcule déplacements géométriques basiques pour former les lignes.
        /// Les 4 directions possibles sont :
        /// - Num 1 : Vertical (vers le haut/bas) : Y +/- step
        /// - Num 2 : Horizontal (gauche/droite) : X +/- step
        /// - Num 3 : Diagonale croissante (haut-gauche à bas-droite) : X +/- step, Y +/- step
        /// - Num 4 : Diagonale décroissante (bas-gauche à haut-droite) : X +/- step, Y -/+ step
        /// Le signe du num détermine la direction (positif/négatif).
        /// </summary>
        /// <param name="num">Le numéro de direction (-4 à -1 ou 1 à 4)</param>
        /// <param name="p">Le point de départ</param>
        /// <returns>Le point calculé dans la direction spécifiée</returns>
        private Point Equation(int num, Point p)
        {
            if (num == 0)
            {
                throw new ArgumentException("num cannot be zero");
            }
            Point proche = new Point();
            int s = step * num / Math.Abs(num);
            switch (Math.Abs(num))
            {
                case 1: // Aligné suivant x = droite verticale
                    proche.X = p.X;
                    proche.Y = p.Y + s;
                    break;
                case 2: // Aligné suivant y = droite horizontale
                    proche.X = p.X + s;
                    proche.Y = p.Y;
                    break;
                case 3: // Aligné suivant +x +y droite croissante
                    proche.X = p.X + s;
                    proche.Y = p.Y + s;
                    break;
                case 4: // Aligné suivant +x -y droite décroissante
                    proche.X = p.X + s;
                    proche.Y = p.Y - s;
                    break;
            }
            return proche;
        }
    }
}
