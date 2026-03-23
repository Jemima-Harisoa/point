using System.Text;
using System.IO;

namespace point;

public class Save
{
   private List<Point> _clickedPoints = new List<Point>();
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
    // Constructeur
    public Save(List<Point> points)
    {
        ClickedPoints = points;
    }

/// sauvegarde des donner de jeux;
    public void Write(string player1, string player2) 
    {
        StringBuilder data = new StringBuilder();
        data.AppendFormat("{0} - {1}\n", player1,  player2);
        foreach (Point point in _clickedPoints)
        {
            string x = point.X.ToString();
            string y = point.Y.ToString();
            data.AppendFormat("({0}, {1})\n", x, y);
        }
        data.Append('\n');
        string result = data.ToString();
     
        
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory); // Crée le répertoire si nécessaire
        }

        // Utiliser FileMode.Create pour créer un fichier s'il n'existe pas

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            byte[] bit = System.Text.Encoding.UTF8.GetBytes(result);
            fs.Write(bit, 0, bit.Length);
        }

    }

/// lecture des donnés du fichier de sauvegarde pour avoir la liste des disposition de point  
    public List<Point> getPointList(){
        List<Point> obj = new List<Point>();  
        using (StreamReader reader = new StreamReader(path))
        {
            string ligne;
           List<Point> newPoints = new List<Point>();
            while ((ligne = reader.ReadLine()) != null)
            {
                // Retirer les parenthèses et les espaces inutiles
                string cleanedInput = ligne.Trim('(', ')').Trim();
            
                // Utiliser Split pour séparer les valeurs en fonction de la virgule
                string[] parts = cleanedInput.Split(',');

                // Initialiser une liste pour stocker les entiers
                int[] numbers = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i].Trim(), out int result))
                    {
                        numbers[i] = result;
                    }
                } 
                if( numbers.Length == 2 ) obj.Add(new Point(numbers[0],numbers[1]));
            }
              
        }
        return obj;
    }
/// lecture des donnés du fichier de sauvegarde pour avoir le nom des joueur par partie
    public static List<string[]> getPlayerList(){
        List<string[]> obj = new List<string[]>();
        using (StreamReader reader = new StreamReader(path))
        {
            string ligne;
          
            while ((ligne = reader.ReadLine()) != null)
            {
                if (!ligne.Contains('-')){
                    continue;
                }
                // Retirer les parenthèses et les espaces inutiles
                string cleanedInput = ligne.Trim();
                // Utiliser Split pour séparer les valeurs en fonction du trai d'uninon
                string[]  parts = cleanedInput.Split('-');

                for (int i = 0 ; i < parts.Length; i ++)
                {
                    parts[i] = parts[i].Trim();
                }
                
                obj.Add(parts);
            }
              
        }
        return obj;
    }
    public Save(){

    }
}


