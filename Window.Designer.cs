namespace point;

partial class Window
{
    
    private System.ComponentModel.IContainer components = null;
    private Panel space;
    private TableLayoutPanel tableLayoutPanel;
    private static Label label1 ;
    private static Label label2 ;
    private List<Point> clickedPoints = new List<Point>();
   
    private int tour = 0;
    private int maxPoint = 0 ;
    private Player player1;
    private Player player2;
    private bool inversed = false;
    private bool hasStarted = false;
    private bool game = false; 
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Initialise les composants visuels de la fenêtre principale.
    /// Crée et configure l'interface utilisateur en 5 sections :
    /// - Score (haut) : affiche les noms et scores des joueurs
    /// - LeftInterface (gauche) : bouton de suggestion pour joueur 1
    /// - RightInterface (droite) : bouton de suggestion pour joueur 2
    /// - Espace central : le terrain de jeu avec grille et points
    /// - Menu (bas) : boutons de contrôle (Start, Restart, Save, Load)
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Text = "Jeu de point";
        Panel Score  = new Panel();
        Panel LeftInterface = new Panel();
        Panel RightInterface = new Panel();
        Panel Menu = new Panel();
        
        //Score Section 
        Score.Width = 150;
        Score.Dock = DockStyle.Top;
        Score.Controls.Add(scoreTable(player1.nom, player2.nom));
        Score.PerformLayout();
        //LeftInterface
        LeftInterface.Width = 150;
        LeftInterface.Dock = DockStyle.Left;
        Button control1 = Suggest(player1, player2);
        
        LeftInterface.Controls.Add(control1);
       
        //RightInterface
        RightInterface.Width = 150;
        RightInterface.Dock = DockStyle.Right;
        Button control2 = Suggest(player2, player1);

        RightInterface.Controls.Add(control2);

        //Menu
        Menu.Height = 150;
        Menu.Dock = DockStyle.Bottom;
        Menu.Controls.Add(restart());
        Menu.Controls.Add(start());
        Menu.Controls.Add(save());
        Menu.Controls.Add(load());
       

        this.Controls.Add(espace());
        this.Controls.Add(Score);
        this.Controls.Add(LeftInterface);
        this.Controls.Add(RightInterface);
        this.Controls.Add(Menu);
        Line.Limit(space);
        
    }

    /// <summary>
    /// Initialise les deux joueurs du jeu avec leurs noms et couleurs distinctes.
    /// Crée deux instances de Player : joueur1 en rouge et joueur2 en bleu.
    /// </summary>
    /// <param name="nom1">Nom du premier joueur</param>
    /// <param name="nom2">Nom du deuxième joueur</param>
    private void InitializePlayer(string nom1, string nom2){
        player1 = new Player(nom1, 0, Color.Red);
        
        player2 = new Player(nom2, 1, Color.Blue);
    }

    /// <summary>
    /// Crée un tableau affichant le score des deux joueurs.
    /// Utilise deux Label colorés (rouge pour joueur1, bleu pour joueur2) pour afficher les noms.
    /// </summary>
    /// <param name="score1">Nom/pseudo du joueur 1</param>
    /// <param name="score2">Nom/pseudo du joueur 2</param>
    /// <returns>Un TableLayoutPanel contenant les informations de score</returns>
    private TableLayoutPanel scoreTable(string score1, string score2  ){
         // Créer le TableLayoutPanel
        this.tableLayoutPanel = new TableLayoutPanel();
        this.tableLayoutPanel.ColumnCount = 2; // Deux colonnes
        this.tableLayoutPanel.RowCount = 1; // Une seule ligne
        this.tableLayoutPanel.Dock = DockStyle.Fill; // Occupe tout l'espace du panneau parent
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Première colonne - 50%
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Deuxième colonne - 50%
        this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Ligne de 50 pixels de hauteur

        // Créer et configurer le premier label
        label1 = new Label();
        label1.Text = score1;
        label1.BackColor = Color.Red;
        
        label1.TextAlign = ContentAlignment.MiddleCenter; // Centre le texte dans le label
        label1.Anchor = AnchorStyles.None; // Centre le label dans sa cellule
        label1.BorderStyle = BorderStyle.FixedSingle; // Bordure autour du label
        label1.Size = new Size(100, 40);
        // Créer et configurer le deuxième label
        label2 = new Label();
        label2.Text = score2;
      
        label2.TextAlign = ContentAlignment.MiddleCenter; // Centre le texte dans le label
        label2.Anchor = AnchorStyles.None; // Centre le label dans sa cellule
        label2.BorderStyle = BorderStyle.FixedSingle; // Bordure autour du label
        label2.Size = new Size(100, 40);

        // Ajouter les labels au TableLayoutPanel
        this.tableLayoutPanel.Controls.Add(label1, 0, 0); // (control, column, row)
        this.tableLayoutPanel.Controls.Add(label2, 1, 0);

        return tableLayoutPanel;

    }

    /// <summary>
    /// Bouton permettant de redémarrer le jeu en remettant les points à zéro.
    /// Réinitialise : le compteur de tours (tour), la liste des points cliqués et l'indicateur de couleur.
    /// </summary>
    /// <returns>Un bouton "Restart" réinitialisant l'état du jeu</returns>
    private Button restart(){ // recomencer le jeu 
        Button RestartButton = new Button();
        RestartButton.Text = "Restart";
        RestartButton.Size = new System.Drawing.Size(100, 40); // Set the size of the button
        RestartButton.Location = new System.Drawing.Point((RestartButton.Width - (start().Width + 150) ) / 2, 10 ); 
        RestartButton.Anchor = AnchorStyles.Top; 
        RestartButton.MouseClick += (sender, e ) => {
            tour = 0;
            game = true;
            clickedPoints.Clear();
            Line.ClickedPoints.Clear();
            label1.BackColor = Color.Red;

            space.Invalidate();

        };
       return RestartButton;
    }

    /// <summary>
    /// Affiche un formulaire de démarrage permettant aux joueurs d'entrer leurs noms et le nombre maximum de points.
    /// Initialise les joueurs et valide les paramètres du jeu avant le lancement.
    /// </summary>
    private void ShowStartForm()
    {
        Form startForm = new Form();
        startForm.Text = "Entrer les noms des joueurs";
        startForm.Size = new Size(450, 250);

        Label labelPlayer1 = new Label() { Text = "Joueur 1:", Location = new Point(10, 20) };
        TextBox textBoxPlayer1 = new TextBox() {Text = "player1", Location = new Point(250, 10), Width = 150, Height = 50 };

        Label labelPlayer2 = new Label() { Text = "Joueur 2:", Location = new Point(10, 60) };
        TextBox textBoxPlayer2 = new TextBox() { Text = "player2", Location = new Point(250, 50), Width = 150, Height = 50 };

        Label Point = new Label() { Text = "Maximum de point:", Location = new Point(10, 100) }; /// maximum de point par joueur
        TextBox textMaxPoint = new TextBox() { Location = new Point(250, 100), Width = 150, Height = 50, Text = "0" };

        Button buttonOk = new Button() { Text = "OK", Location = new Point(300, 140), Width = 80, Height = 50 };
        buttonOk.Click += (sender, e) => 
        {   
            hasStarted = true;
            game = true;
            maxPoint = int.Parse(textMaxPoint.Text) ;
            InitializePlayer(textBoxPlayer1.Text, textBoxPlayer2.Text);
            startForm.Close();
        };

        startForm.Controls.Add(labelPlayer1);
        startForm.Controls.Add(textBoxPlayer1);
        startForm.Controls.Add(labelPlayer2);
        startForm.Controls.Add(textBoxPlayer2);
        startForm.Controls.Add(Point);
        startForm.Controls.Add(textMaxPoint);
        startForm.Controls.Add(buttonOk);

        startForm.ShowDialog();
    }

    /// <summary>
    /// Bouton de contrôle basique du jeu : bascule entre pausé et en cours.
    /// Alterne entre les états "Start" et "Pause" pour contrôler le déroulement du jeu.
    /// </summary>
    /// <returns>Un bouton "Start/Pause" contrôlant l'état du jeu</returns>
    private Button start() // mise en marche 
    {
        Button StartButton = new Button();
        StartButton.Text = "Pause";
        StartButton.Size = new System.Drawing.Size(100, 40); // Set the size of the button
        StartButton.Location = new System.Drawing.Point((150 - StartButton.Width) / 2, 10); 
        StartButton.Anchor = AnchorStyles.Top; 
        StartButton.MouseClick += (sender, e) => 
        {
            if (!game)
            {
                StartButton.Text = "Pause";
            }
            else
            {
                StartButton.Text = "Start";
            }
            game = !game;
            
        };
        return StartButton;
    }
  

    /// <summary>
    /// Bouton permettant de sauvegarder l'état actuel du jeu.
    /// Enregistre la liste des points cliqués et les noms des deux joueurs dans un fichier.
    /// Met ensuite le jeu en marche après la sauvegarde.
    /// </summary>
    /// <returns>Un bouton "Save" sauvegardant l'état du jeu</returns>
    private Button save()// sauvegarde 
    {
        Button SaveButton = new Button();
        SaveButton.Text = "Save";
        SaveButton.Size = new System.Drawing.Size(100, 40); // Set the size of the button
        SaveButton.Location = new System.Drawing.Point( ((start().Width + 250 ) - SaveButton.Width) / 2 , 10); 
        SaveButton.Anchor = AnchorStyles.Top; 
        SaveButton.MouseClick += (sender, e ) => {
            Save sauvegarde = new Save(clickedPoints);
            sauvegarde.Write(player1.nom, player2.nom);
            game = true;
        };
       return SaveButton;
    }

    /// <summary>
    /// Bouton permettant de charger une partie sauvegardée.
    /// Restaure la liste des points cliqués précédemment sauvegardés et redessine le plateau.
    /// </summary>
    /// <returns>Un bouton "Load" chargeant une partie sauvegardée</returns>
    private Button load(){//load
        Button LoadButton = new Button();
        LoadButton.Text = "Load";
        LoadButton.Size = new System.Drawing.Size(100, 40); // Set the size of the button
        LoadButton.Location = new System.Drawing.Point( ((start().Width + save().Width + 350) - LoadButton.Width) / 2 , 10); 
        LoadButton.Anchor = AnchorStyles.Top; 
        LoadButton.MouseClick += (sender, e ) => {
            Save sauvegarde = new Save();
            clickedPoints = sauvegarde.getPointList();
            Line.ClickedPoints = clickedPoints ; 
            game = true;
            space.Invalidate(); // Déclencher le redessin du panneau
        };
       return LoadButton;
    }
    /// <summary>
    /// Bouton d'aide stratégique suggérant le meilleur coup à jouer pour un joueur.
    /// Utilise une IA simple en quatre étapes : 
    /// 1. Cherche un point gagnant pour le joueur actuel
    /// 2. Cherche un point bloquant pour l'adversaire
    /// 3. Cherche un point créant une formation de 3 points
    /// 4. Affiche un message si aucun coup optimal n'est trouvé
    /// </summary>
    /// <param name="player">Le joueur pour lequel suggérer un coup</param>
    /// <param name="advers">Le joueur adverse</param>
    /// <returns>Un bouton "Suggest" proposant un coup au joueur</returns>
    private Button Suggest(Player player, Player advers ){ 

        Button SuggestButton = new Button();
        SuggestButton.Text = "Suggest "+ player.nom;
        SuggestButton.Size = new System.Drawing.Size(100, 80); // Set the size of the button
        SuggestButton.Location = new System.Drawing.Point( ( 150  - SuggestButton.Width) / 2 , (espace().Height + 300) / 2 ); 
        SuggestButton.Anchor = AnchorStyles.Top; 
          
        SuggestButton.MouseClick += (sender, e ) => {
            
            if(tour % 2 == player.Order && game){

                Point p = new Point(0, 0); // Valeur spéciale pour représenter "non défini"

                if (player.CanWin())
                {
                    p = Player.Suggest(player, 4);
                }

                if (p.X == 0 && p.Y == 0 && advers.CanWin()) 
                {
                    p = Player.Suggest(advers, 4);
                }

                if (p.X == 0 && p.Y == 0 && player.has(3)) 
                {
                    p = Player.Suggest(player, 3);
                }

                if (p.X == 0 && p.Y == 0)    
                {
                    MessageBox.Show("Aucun point à suggérer");
                }
                else 
                {
                    add(p);
                }
            }
        };
       
        
       return SuggestButton;
    }

    
    /// <summary>
    /// Crée le panneau principal du terrain de jeu (grille carrée).
    /// Ce panneau gère les événements de dessin et les clics de souris pour placer des points.
    /// Il occupe toute la zone centrale de la fenêtre (DockStyle.Fill).
    /// </summary>
    /// <returns>Un panneau configuré pour servir de terrain de jeu</returns>
    private Panel espace()
    {
        space = new Panel
        {
            Dock = DockStyle.Fill
        };
        space.Paint += paint;
        if(game)space.MouseClick += space_MouseClick;
        return space;
    }
    /// <summary>
    /// Gestionnaire d'événement déclenchée lors d'un clic de souris sur le terrain.
    /// Détecte le clic le plus proche d'une intersection de la grille et ajoute un point si :
    /// - Le clic est suffisamment proche d'une intersection (tolérance = 25 pixels)
    /// - Le point n'a pas déjà été cliqué
    /// - Le point ne se trouve pas sur une limite de frontière
    /// </summary>
    private void space_MouseClick(object sender, MouseEventArgs e)
    {
        if (game)
        {
           
            // Tolérance pour détecter les clics proches d'un point d'intersection
            int tolerance = 25;
            int gridSize = 50;
            // Trouver l'intersection la plus proche
            int nearestX = (int)Math.Round(e.X / (double)gridSize) * gridSize;
            int nearestY = (int)Math.Round(e.Y / (double)gridSize) * gridSize;
            // Vérifier si le clic est suffisamment proche de cette intersection
            if (Math.Abs(e.X - nearestX) <= tolerance && Math.Abs(e.Y - nearestY) <= tolerance)
            {
                Point p = new Point(nearestX, nearestY);
              
                if(!clickedPoints.Contains(p) && !Line.isLimit(p)){
                    add(p);
                } 
            }
        }
    } 
    /// <summary>
    /// Vérifie si le nombre maximum de points (maxPoint * 2) a été atteint.
    /// Si oui, inverse les rôles des joueurs (couleurs et ordre de jeu) et recommence une manche.
    /// Cela permet une alternance équitable des tours entre les deux joueurs.
    /// </summary>
    private void CheckPoint(){
        if(clickedPoints.Count == maxPoint * 2){
            inversed = !inversed;  
            Color cl = player1.color;
            player1.color =  player2.color; 
            player2.color =  cl;
            int order =  player1.Order;
            player1.Order =  player2.Order; 
            player2.Order =  order;
            
            for (int i = 1; i < clickedPoints.Count ; i++)
            {
                clickedPoints[i - 1] = clickedPoints[i]; 
            }
            clickedPoints.Remove(clickedPoints.Last() );
        }        
    }
    
    /// <summary>
    /// Ajoute un nouveau point au terrain de jeu et met à jour l'affichage.
    /// Incrémente le compteur de tours (tour) et ajoute le point à la liste des points cliqués.
    /// Déclenche un redraw du panneau pour visualiser le nouveau point et ses connexions.
    /// </summary>
    /// <param name="p">Le point (coordinate) à ajouter à la partie</param>
    private void add(Point p){
        tour++;
        clickedPoints.Add(p);
        space.Paint += Draw; // Ajouter le point d'intersection le plus proche
        space.Invalidate(); // Déclencher le redessin du panneau
    } 

    /// <summary>
    /// Dessine la grille de jeu et tous les points cliqués par les joueurs.
    /// Responsabilités :
    /// - Trace les lignes verticales et horizontales (espacement de 50 pixels)
    /// - Affiche chaque point avec sa couleur (rouge ou bleu selon le joueur)
    /// - Met à jour les indicateurs de couleur dans les labels de score
    /// - Vérifie si le nombre maximum de points est atteint (CheckPoint)
    /// </summary>
    private void paint(object sender, PaintEventArgs e){
        
    // Objet graphics du panneau
        Graphics graph = e.Graphics;
    // Définition de l'élément pour dessiner la ligne
        Pen BlackPen = new Pen(Color.Black, 2);
    // Définition des lignes verticales
        for(int i = 50; i < space.Width; i += 50){
            graph.DrawLine(BlackPen, i, 0 , i, space.Height); 
        }
    // Définition des lignes horizontales
        for(int j = 50; j < space.Height; j += 50){
            graph.DrawLine(BlackPen, 0, j, space.Width, j);
        } 
    // Dessiner les points colorés aux emplacements cliqués
        if (maxPoint != 0) CheckPoint();
        int k = !inversed ?  0 : 1;    

        foreach (var point in clickedPoints)
        {
            if( k % 2 == 0){
                graph.FillEllipse(Brushes.Red, point.X - 5, point.Y - 5, 10, 10);
                label1.BackColor = Color.White;
                label2.BackColor = Color.Blue;
            }
            else{
                graph.FillEllipse(Brushes.Blue, point.X - 5, point.Y - 5, 10, 10);
                label2.BackColor = Color.White;
                label1.BackColor = Color.Red;
            }
            k++;
        }    
    }

    /// <summary>
    /// Dessine les lignes entre les points et détecte la victoire d'un joueur.
    /// Responsabilités :
    /// - Passe la liste des points cliqués à la classe Line
    /// - Vérifie si le joueur 1 a un alignement gagnant (5 points alignés)
    /// - Vérifie si le joueur 2 a un alignement gagnant 
    /// - Appelle la fonction de dessin du joueur vainqueur et arrête le jeu
    /// </summary>
    public void Draw(object sender, PaintEventArgs e){
    // Objet graphics du panneau
        Graphics graph = e.Graphics;
        Line.ClickedPoints = clickedPoints;
        
        if(player1.has(5) ){
            player1.paint(sender, e);
            game = false;
            return;
        } 
        if(player2.has(5)){
            player2.paint(sender, e);
            game = false;
            return;
        } 


    }

}
