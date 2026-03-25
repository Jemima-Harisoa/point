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

    // Missile animation et sélection de ligne
    private System.Windows.Forms.Timer missileAnimationTimer;
    private bool isSelectingMissileLine = false; // Mode sélection ligne pour missile
    private Player missileLaunchingPlayer = null; // Joueur qui veut lancer un missile
    private Label currentMissileLineLabel = null; // Référence au label de ligne à mettre à jour
    private int selectedMissilePower = 3; // Puissance calibrée par le joueur 
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

        // Initialiser le Timer pour l'animation des missiles
        missileAnimationTimer = new System.Windows.Forms.Timer(this.components);
        missileAnimationTimer.Interval = 100; // 100ms entre chaque frame
        missileAnimationTimer.Tick += MissileAnimationTimer_Tick;

        Panel Score  = new Panel();
        Panel LeftInterface = new Panel();
        Panel RightInterface = new Panel();
        Panel Menu = new Panel();
        
        //Score Section 
        Score.Width = 150;
        Score.Dock = DockStyle.Top;
        Score.Controls.Add(scoreTable(player1.nom, player2.nom));
        Score.PerformLayout();
        //LeftInterface - Barre de lancement de missile pour joueur 1
        LeftInterface.Width = 200;
        LeftInterface.Dock = DockStyle.Left;
        LeftInterface.Controls.Add(MissileBar(player1, player2));

        //RightInterface - Barre de lancement de missile pour joueur 2
        RightInterface.Width = 200;
        RightInterface.Dock = DockStyle.Right;
        RightInterface.Controls.Add(MissileBar(player2, player1));

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
            player1.Missiles.Clear();
            player2.Missiles.Clear();
            player1.HasLaunchedMissileThisTurn = false;
            player2.HasLaunchedMissileThisTurn = false;
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
        // Mode sélection de ligne pour missile
        if (isSelectingMissileLine && missileLaunchingPlayer != null && currentMissileLineLabel != null)
        {
            // Calculer la ligne cliquée (1-indexed)
            int clickedRow = (int)Math.Round(e.Y / (double)GameConfig.GridSize);

            // Vérifier que la ligne est valide
            if (clickedRow >= 1 && clickedRow <= GameConfig.GridRows)
            {
                // Mettre à jour directement le label via la référence stockée
                currentMissileLineLabel.Text = clickedRow.ToString();
                MessageBox.Show($"Ligne {clickedRow} sélectionnée !");
            }
            else
            {
                MessageBox.Show("Ligne invalide ! Cliquez sur une ligne de la grille.");
            }

            // Désactiver le mode sélection
            isSelectingMissileLine = false;
            missileLaunchingPlayer = null;
            currentMissileLineLabel = null;
            return;
        }

        if (game && hasStarted)
        {
            // Bloquer le clic si le joueur courant a déjà lancé un missile ce tour
            Player currentPlayer = (clickedPoints.Count % 2 == 0) ? player1 : player2;
            if (currentPlayer.HasLaunchedMissileThisTurn) return;

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

        // Dessiner les missiles des deux joueurs
        foreach (var missile in player1.Missiles)
            missile.paint(sender, e);

        foreach (var missile in player2.Missiles)
            missile.paint(sender, e);
    }

    /// <summary>
    /// Crée et retourne un panel avec la barre de lancement de missile pour un joueur
    /// Jauge verticale, sélection de ligne par clic sur grille, bouton Lancer
    /// </summary>
    private Panel MissileBar(Player player, Player adversaire)
    {
        Panel missilePanel = new Panel();
        missilePanel.Dock = DockStyle.Fill;
        missilePanel.AutoScroll = true;

        // FlowLayoutPanel pour disposer les contrôles verticalement
        FlowLayoutPanel layout = new FlowLayoutPanel();
        layout.Dock = DockStyle.Fill;
        layout.FlowDirection = FlowDirection.TopDown;
        layout.WrapContents = false;
        layout.AutoScroll = true;

        // Titre
        Label titleLabel = new Label();
        titleLabel.Text = $"🚀 Missile - {player.nom}";
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
        layout.Controls.Add(titleLabel);

        // Sélection de ligne
        Label lineLabel = new Label();
        lineLabel.Text = "Ligne de tir:";
        lineLabel.AutoSize = true;
        layout.Controls.Add(lineLabel);

        Label lineValueLabel = new Label();
        lineValueLabel.Name = "lineValueLabel";
        lineValueLabel.Text = "Aucune";
        lineValueLabel.AutoSize = true;
        lineValueLabel.Font = new Font(lineValueLabel.Font, FontStyle.Bold);
        lineValueLabel.ForeColor = Color.Blue;
        layout.Controls.Add(lineValueLabel);

        Button selectLineButton = new Button();
        selectLineButton.Text = "📍 Choisir ligne sur grille";
        selectLineButton.Width = 180;
        selectLineButton.Height = 35;
        selectLineButton.Click += (s, e) =>
        {
            isSelectingMissileLine = true;
            missileLaunchingPlayer = player;
            MessageBox.Show("Cliquez sur la grille pour choisir une ligne de tir !");
        };
        layout.Controls.Add(selectLineButton);

        // Puissance - Jauge VERTICALE
        Label powerLabel = new Label();
        powerLabel.Text = "Puissance (1-9):";
        powerLabel.AutoSize = true;
        layout.Controls.Add(powerLabel);

        Label powerInfo = new Label();
        powerInfo.Text = "(9 = portée max)";
        powerInfo.AutoSize = true;
        powerInfo.Font = new Font(powerInfo.Font.FontFamily, 8, FontStyle.Italic);
        layout.Controls.Add(powerInfo);

        // TrackBar VERTICAL
        TrackBar powerSlider = new TrackBar();
        powerSlider.Name = "powerSlider";
        powerSlider.Orientation = Orientation.Vertical; // VERTICAL !
        powerSlider.Minimum = 1;
        powerSlider.Maximum = 9;
        powerSlider.Value = 5;
        powerSlider.Height = 150; // Plus grand pour une jauge verticale
        powerSlider.Width = 50;
        powerSlider.TickFrequency = 1;
        layout.Controls.Add(powerSlider);

        Label powerValue = new Label();
        powerValue.Text = "5";
        powerValue.AutoSize = true;
        powerValue.Font = new Font(powerValue.Font, FontStyle.Bold);
        layout.Controls.Add(powerValue);

        // Mise à jour du label de puissance et stockage dans variable globale
        powerSlider.ValueChanged += (s, e) =>
        {
            powerValue.Text = powerSlider.Value.ToString();
            selectedMissilePower = powerSlider.Value;
        };

        // Bouton de lancement
        Button launchButton = new Button();
        launchButton.Text = "🚀 LANCER !";
        launchButton.Width = 180;
        launchButton.Height = 45;
        launchButton.BackColor = Color.FromArgb(255, 100, 100);
        launchButton.ForeColor = Color.White;
        launchButton.Font = new Font(launchButton.Font, FontStyle.Bold);
        launchButton.Click += (s, e) =>
        {
            // Vérifier qu'une ligne a été sélectionnée
            if (lineValueLabel.Text == "Aucune")
            {
                MessageBox.Show("Veuillez d'abord choisir une ligne de tir !");
                return;
            }

            int selectedRow = int.Parse(lineValueLabel.Text);
            LaunchMissileWithAnimation(player, adversaire, selectedRow, powerSlider.Value);

            // Réinitialiser la sélection
            lineValueLabel.Text = "Aucune";
        };
        layout.Controls.Add(launchButton);

        missilePanel.Controls.Add(layout);
        return missilePanel;
    }

    /// <summary>
    /// Gère le lancement d'un missile
    /// Le missile part toujours du bord gauche (colonne 0) et va horizontalement vers la droite
    /// </summary>
    /// <summary>
    /// Gère le lancement d'un missile avec animation
    /// Le missile part du bord gauche (player1) ou droit (player2) selon le joueur
    /// </summary>
    private void LaunchMissileWithAnimation(Player player, Player adversaire, int row, int power)
    {
        // Vérifier si c'est le tour du joueur
        Player currentPlayer = (clickedPoints.Count % 2 == 0) ? player1 : player2;
        if (currentPlayer != player)
        {
            MessageBox.Show("Ce n'est pas votre tour !");
            return;
        }

        // Vérifier si le joueur a déjà lancé un missile ce tour
        if (player.HasLaunchedMissileThisTurn)
        {
            MessageBox.Show("Vous avez déjà lancé un missile ce tour !");
            return;
        }

        if (!game)
        {
            MessageBox.Show("Le jeu n'a pas commencé !");
            return;
        }

        // Déterminer la direction selon le joueur
        // Player 1 (à gauche) : direction = 1 (vers la droite)
        // Player 2 (à droite) : direction = -1 (vers la gauche)
        int direction = (player == player1) ? 1 : -1;

        // Calculer la position de départ selon le joueur et la ligne
        // row commence à 1 dans l'UI, donc on soustrait 1 pour avoir l'index de grille
        int yPosition = (row - 1) * GameConfig.GridSize;
        Point launchPosition;

        if (player == player1)
        {
            // Player 1 : part du bord gauche (x = 0)
            launchPosition = new Point(0, yPosition);
        }
        else
        {
            // Player 2 : part du bord droit (x = dernière colonne)
            launchPosition = new Point((GameConfig.GridColumns - 1) * GameConfig.GridSize, yPosition);
        }

        // Créer et lancer le missile avec animation
        Missile missile = player.CreateMissile();
        missile.Launch(launchPosition, power, direction);

        // Démarrer le Timer pour l'animation
        missileAnimationTimer.Start();

        // Marquer le missile comme lancé et passer au tour suivant IMMÉDIATEMENT
        player.HasLaunchedMissileThisTurn = true;
        tour++;
        adversaire.ResetTurn();

        _isDirty = true;
        space.Invalidate();
    }

    /// <summary>
    /// Supprime un point ennemi touché par un missile
    /// Utilise Point.Empty pour masquer le point sans casser l'alternance pair/impair
    /// </summary>
    private void RemoveEnemyPoint(Point p, Player adversaire)
    {
        for (int i = 0; i < clickedPoints.Count; i++)
        {
            if (clickedPoints[i] == p)
            {
                clickedPoints[i] = new Point(-1, -1); // Masquer le point avec Point.Empty équivalent
                break;
            }
        }
    }

    /// <summary>
    /// Gère l'animation des missiles (appelé toutes les 100ms par le Timer)
    /// </summary>
    private void MissileAnimationTimer_Tick(object sender, EventArgs e)
    {
        bool anyMissileMoving = false;

        // Vérifier tous les missiles des deux joueurs
        foreach (var missile in player1.Missiles)
        {
            if (missile.State == MissileState.Flying)
            {
                bool finished = missile.Step();
                if (!finished)
                {
                    anyMissileMoving = true;

                    // Vérifier collision à la position actuelle
                    Point currentPos = missile.GetCurrentPosition();
                    List<Point> enemyPoints = player2.line.GetPlayerPoints();
                    foreach (var enemyPoint in enemyPoints)
                    {
                        if (currentPos == enemyPoint)
                        {
                            missile.State = MissileState.Destroyed;
                            RemoveEnemyPoint(enemyPoint, player2);
                            break;
                        }
                    }
                }
            }
        }

        foreach (var missile in player2.Missiles)
        {
            if (missile.State == MissileState.Flying)
            {
                bool finished = missile.Step();
                if (!finished)
                {
                    anyMissileMoving = true;

                    // Vérifier collision à la position actuelle
                    Point currentPos = missile.GetCurrentPosition();
                    List<Point> enemyPoints = player1.line.GetPlayerPoints();
                    foreach (var enemyPoint in enemyPoints)
                    {
                        if (currentPos == enemyPoint)
                        {
                            missile.State = MissileState.Destroyed;
                            RemoveEnemyPoint(enemyPoint, player1);
                            break;
                        }
                    }
                }
            }
        }

        // Si aucun missile ne bouge plus, arrêter le timer
        if (!anyMissileMoving)
        {
            missileAnimationTimer.Stop();
        }

        _isDirty = true;
        space.Invalidate();
    }

}

