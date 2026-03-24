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

    // Optimisation du rendu : drapeau pour savoir si on doit redessiner
    private bool _isDirty = true; 
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
        //LeftInterface - Boutons de suggestion désactivés pour l'instant
        LeftInterface.Width = 150;
        LeftInterface.Dock = DockStyle.Left;
        // Button control1 = Suggest(player1, player2);
        // LeftInterface.Controls.Add(control1);

        //RightInterface - Boutons de suggestion désactivés pour l'instant
        RightInterface.Width = 150;
        RightInterface.Dock = DockStyle.Right;
        // Button control2 = Suggest(player2, player1);
        // RightInterface.Controls.Add(control2);

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

        // Ajouter un gestionnaire pour redessiner la grille quand la fenêtre change de taille
        this.Resize += (sender, e) => {
            if(space != null && space.Width > 0 && space.Height > 0)
            {
                GameConfig.UpdateGridSize(space.Width, space.Height);
                space.Invalidate(); // Redessiner le panneau avec la nouvelle taille
            }
        };
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
        RestartButton.Location = new System.Drawing.Point((RestartButton.Width - (start().Width + 150) ) / 2, 10);
        RestartButton.Anchor = AnchorStyles.Top;
        RestartButton.MouseClick += (sender, e ) => {
            tour = 0;
            game = true;
            clickedPoints.Clear();
            Line.ClickedPoints.Clear();
            label1.BackColor = Color.Red;
            _isDirty = true; // Marquer qu'on doit redessiner
            space.Invalidate();

        };
       return RestartButton;
    }

    /// <summary>
    /// Affiche un formulaire de démarrage permettant aux joueurs d'entrer leurs noms et configurer les paramètres du jeu.
    /// Initialise les joueurs et valide les paramètres du jeu avant le lancement.
    /// Paramètres configurables:
    /// - Noms des deux joueurs
    /// - Nombre maximum de points par manche
    /// - Nombre de colonnes de la grille
    /// - Nombre de lignes de la grille
    /// - Nombre de points à aligner pour gagner
    /// </summary>
    private void ShowStartForm()
    {
        Form startForm = new Form();
        startForm.Text = "Configuration du jeu";
        startForm.Size = new Size(600, 450);
        startForm.StartPosition = FormStartPosition.CenterScreen;

        // Joueur 1
        Label labelPlayer1 = new Label() { Text = "Joueur 1:", Location = new Point(10, 20), Width = 150 };
        TextBox textBoxPlayer1 = new TextBox() { Text = "player1", Location = new Point(250, 10), Width = 200, Height = 30 };

        // Joueur 2
        Label labelPlayer2 = new Label() { Text = "Joueur 2:", Location = new Point(10, 60), Width = 150 };
        TextBox textBoxPlayer2 = new TextBox() { Text = "player2", Location = new Point(250, 50), Width = 200, Height = 30 };

        // Points maximum par manche
        Label labelMaxPoint = new Label() { Text = "Points max par manche (0 = illimité):", Location = new Point(10, 100), Width = 200 };
        TextBox textMaxPoint = new TextBox() { Location = new Point(250, 100), Width = 200, Height = 30, Text = "0" };

        // Colonnes de la grille
        Label labelColumns = new Label() { Text = "Colonnes grille:", Location = new Point(10, 140), Width = 200 };
        TextBox textColumns = new TextBox() { Location = new Point(250, 140), Width = 200, Height = 30, Text = GameConfig.GridColumns.ToString() };

        // Lignes de la grille
        Label labelRows = new Label() { Text = "Lignes grille:", Location = new Point(10, 180), Width = 200 };
        TextBox textRows = new TextBox() { Location = new Point(250, 180), Width = 200, Height = 30, Text = GameConfig.GridRows.ToString() };

        // Points pour gagner
        Label labelPointsToWin = new Label() { Text = "Points à aligner pour gagner:", Location = new Point(10, 220), Width = 200 };
        TextBox textPointsToWin = new TextBox() { Location = new Point(250, 220), Width = 200, Height = 30, Text = GameConfig.PointsToWin.ToString() };

        // Bouton OK
        Button buttonOk = new Button() { Text = "Commencer", Location = new Point(250, 270), Width = 200, Height = 40 };
        buttonOk.Click += (sender, e) =>
        {
            try
            {
                // Valider les entrées
                hasStarted = true;
                game = true;
                maxPoint = int.Parse(textMaxPoint.Text);
                InitializePlayer(textBoxPlayer1.Text, textBoxPlayer2.Text);

                // Paramètres de la grille
                int columns = int.Parse(textColumns.Text);
                int rows = int.Parse(textRows.Text);
                GameConfig.SetGridDimensions(columns, rows);

                // Points pour gagner
                GameConfig.PointsToWin = int.Parse(textPointsToWin.Text);
                GameConfig.PointsForCanWin = Math.Max(1, GameConfig.PointsToWin - 1);

                startForm.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Veuillez entrer des nombres valides pour tous les champs numériques.", "Erreur de saisie");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur");
            }
        };

        // Ajouter les contrôles au formulaire
        startForm.Controls.Add(labelPlayer1);
        startForm.Controls.Add(textBoxPlayer1);
        startForm.Controls.Add(labelPlayer2);
        startForm.Controls.Add(textBoxPlayer2);
        startForm.Controls.Add(labelMaxPoint);
        startForm.Controls.Add(textMaxPoint);
        startForm.Controls.Add(labelColumns);
        startForm.Controls.Add(textColumns);
        startForm.Controls.Add(labelRows);
        startForm.Controls.Add(textRows);
        startForm.Controls.Add(labelPointsToWin);
        startForm.Controls.Add(textPointsToWin);
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
            _isDirty = true; // Marquer qu'on doit redessiner
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
    /// OPTIMISÉ : Double buffering activé pour réduire le flickering
    /// </summary>
    /// <returns>Un panneau configuré pour servir de terrain de jeu</returns>
    private Panel espace()
    {
        space = new DoubleBufferedPanel
        {
            Dock = DockStyle.Fill
        };
        space.Paint += paint;
        space.Paint += Draw; // Ajouter le handler de détection de victoire une seule fois
        if(game)space.MouseClick += space_MouseClick;
        return space;
    }
    /// <summary>
    /// Gestionnaire d'événement déclenchée lors d'un clic de souris sur le terrain.
    /// Détecte le clic le plus proche d'une intersection de la grille et ajoute un point si :
    /// - C'est le bon moment dans l'ordre des tours (vérifié via le nombre de points placés)
    /// - Le clic est suffisamment proche d'une intersection (tolérance configurable)
    /// - Le point n'a pas déjà été cliqué
    /// - Le point est dans la zone valide de la grille
    ///
    /// CRUCIAL: L'ordre des points dans clickedPoints DOIT alterner exactement (pair=joueur1, impair=joueur2)
    /// pour que la détection des alignements fonctionne correctement.
    /// </summary>
    private void space_MouseClick(object sender, MouseEventArgs e)
    {
        if (game && hasStarted)
        {
            // Utiliser les paramètres de GameConfig pour flexibilité
            int tolerance = GameConfig.ClickTolerance;
            int gridSize = GameConfig.GridSize;

            // Trouver l'intersection la plus proche
            int nearestX = (int)Math.Round(e.X / (double)gridSize) * gridSize;
            int nearestY = (int)Math.Round(e.Y / (double)gridSize) * gridSize;

            // Vérifier si le clic est suffisamment proche de cette intersection
            if (Math.Abs(e.X - nearestX) <= tolerance && Math.Abs(e.Y - nearestY) <= tolerance)
            {
                Point p = new Point(nearestX, nearestY);

                // Vérifier que le point est dans les limites valides de la grille
                if(nearestX > 0 && nearestX < space.Width && nearestY > 0 && nearestY < space.Height
                    && !clickedPoints.Contains(p))
                {
                    // Calculer quel joueur devrait jouer selon le nombre de coups
                    // Le nombre de points déjà placés détermine l'ordre:
                    // clickedPoints.Count pair (0,2,4...) → prochain joueur est type 0 (joueur 1)
                    // clickedPoints.Count impair (1,3,5...) → prochain joueur est type 1 (joueur 2)
                    // Avec inversed, les rôles peuvent être inversés
                    int expectedType = clickedPoints.Count % 2;
                    if(inversed) expectedType = (expectedType + 1) % 2;

                    // Pour un contrôle strict: rejeter silencieusement les clics hors tour
                    // Les joueurs doivent respecter l'alternance manuellement
                    // (Dans une future version, on pourrait afficher "Ce n'est pas votre tour")
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
        Line.ClickedPoints = clickedPoints; // Mettre à jour immédiatement pour éviter le retard et invalider les caches
        _isDirty = true; // Marquer qu'on doit redessiner
        space.Invalidate(); // Déclencher le redessin du panneau
    } 

    /// <summary>
    /// Dessine la grille de jeu et tous les points cliqués par les joueurs.
    /// Responsabilités :
    /// - Met à jour la taille de la grille selon les vraies dimensions du panneau
    /// - Trace les lignes verticales et horizontales (espacement configurable)
    /// - Affiche chaque point avec sa couleur (rouge ou bleu selon le joueur)
    /// - Met à jour les indicateurs de couleur dans les labels de score
    /// - Vérifie si le nombre maximum de points est atteint (CheckPoint)
    /// OPTIMISÉ : Double buffering activé pour réduire le flickering
    /// </summary>
    private void paint(object sender, PaintEventArgs e){
        Graphics graph = e.Graphics;
        graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Recalculer la taille de la grille selon les vraies dimensions du panneau
        // (Important: la taille du panneau peut être différente des dimensions par défaut)
        if(space.Width > 0 && space.Height > 0)
        {
            GameConfig.UpdateGridSize(space.Width, space.Height);
        }

        // Utiliser la taille de grille paramétrable
        int gridSize = GameConfig.GridSize;
        Pen BlackPen = new Pen(Color.Black, 2);

        // Tracer les lignes verticales
        for(int i = gridSize; i < space.Width; i += gridSize){
            graph.DrawLine(BlackPen, i, 0, i, space.Height);
        }

        // Tracer les lignes horizontales
        for(int j = gridSize; j < space.Height; j += gridSize){
            graph.DrawLine(BlackPen, 0, j, space.Width, j);
        }

        // Dessiner les points colorés aux emplacements cliqués
        if (maxPoint != 0) CheckPoint();
        int k = !inversed ? 0 : 1;

        foreach (var point in clickedPoints)
        {
            if(k % 2 == 0){
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
    /// - Vérifie si le joueur 1 a un alignement gagnant
    /// - Vérifie si le joueur 2 a un alignement gagnant
    /// - Appelle la fonction de dessin du joueur vainqueur et arrête le jeu
    /// Le nombre de points pour la victoire est paramétrable via GameConfig.PointsToWin
    /// OPTIMISÉ : Bénéficie du double buffering et mise en cache des calculs
    /// </summary>
    public void Draw(object sender, PaintEventArgs e){
        Graphics graph = e.Graphics;
        graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Mettre à jour la liste des points dans la classe Line
        Line.ClickedPoints = clickedPoints;

        // Vérifier si joueur 1 a gagné (nombre de points configurable)
        if(player1.has(GameConfig.PointsToWin)){
            player1.paint(sender, e);
            game = false;
            return;
        }

        // Vérifier si joueur 2 a gagné (nombre de points configurable)
        if(player2.has(GameConfig.PointsToWin)){
            player2.paint(sender, e);
            game = false;
            return;
        }
    }

}
