namespace point;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

        ApplicationConfiguration.Initialize();
        Application.Run(new Window());
        //Création d'une liste de points
        /*         
        List<Point> points = new List<Point>
            {
                new Point(250, 200),//0
                new Point(350, 150),//1
                new Point(300, 150),//0
                new Point(250, 150),//1
                new Point(300, 200),//0
                new Point(300, 100),//1
                new Point(350, 200),//0
                new Point(300, 250),//1
                new Point(200, 200),//0
                new Point(150, 200),//1
                new Point(200, 100),//0
                new Point(400, 200),//1
                new Point(200, 150)//0
            };

        // Instanciation de la classe Line avec les points
        Line.ClickedPoints = points;
        Line line = new Line();
        line.Type = 0;
        // Récupération des lignes de points alignés

        
        Console.WriteLine("Ensemeble de tout les points :");
        
                Console.Write("Ligne : ");
                foreach (var point in Line.ClickedPoints)
                {
                    Console.Write($"({point.X}, {point.Y}) ");
                }
                Console.WriteLine();
            
        

        Console.WriteLine("Liste des point du joueur :");
        foreach (var lineList in line.LShapeLine())
        {
           if(lineList.Count != 3) continue;
                Console.Write("Ligne : ");
                foreach (var point in lineList)
                {
                    Console.Write($"({point.X}, {point.Y}) ");
                }
                Console.WriteLine();
        }
        Console.WriteLine();
      

      //  List<List<Point>> lines = line.LShapeLine();

        line.SuggestionL(3);
      
        */
    }
}