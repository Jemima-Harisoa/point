using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace point
{
    public enum MissileState { Ready, Flying, Destroyed }

    public class Missile
    {
        #region Properties
        public Point Position { get; set; }
        public int Power { get; set; }
        public MissileState State { get; set; }
        public Color OwnerColor { get; set; }
        public int Direction { get; set; } // 1 = droite, -1 = gauche
        public int CurrentStep { get; set; } // Pour l'animation
        public bool HitTarget { get; set; }
        private List<Point> Trajectory { get; set; }
        #endregion

        public Missile(Color ownerColor)
        {
            OwnerColor = ownerColor;
            State = MissileState.Ready;
            Trajectory = new List<Point>();
            CurrentStep = 0;
            HitTarget = false;
        }

        /// <summary>
        /// Lance le missile depuis une ligne donnée avec une puissance (1-9) et une direction
        /// direction = 1 (droite) ou -1 (gauche)
        /// Formule distance : floor((puissance × max_colonnes) / 9)
        /// </summary>
        public void Launch(Point from, int power, int direction)
        {
            Position = from;
            Power = power;
            Direction = direction;
            Trajectory = new List<Point>();
            CurrentStep = 0;
            HitTarget = false;

            // Nouvelle formule : distance_max = floor((puissance × max_colonnes) / 9)
            int maxColumns = GameConfig.GridColumns;
            int maxSteps = (int)Math.Floor((double)(power * maxColumns) / 9.0);

            // Limites horizontales réelles de la grille (peut être centrée avec offset)
            int minX = from.X;
            int maxX = from.X;
            if (direction > 0)
            {
                maxX = from.X + ((maxColumns - 1) * GameConfig.GridSize);
            }
            else
            {
                minX = from.X - ((maxColumns - 1) * GameConfig.GridSize);
            }

            Point currentPoint = from;

            for (int i = 0; i < maxSteps; i++)
            {
                // Direction : 1 = droite (+), -1 = gauche (-)
                Point nextPoint = new Point(
                    currentPoint.X + (direction * GameConfig.GridSize),
                    currentPoint.Y
                );

                // Vérifier les limites
                if (nextPoint.X < minX || nextPoint.X > maxX)
                {
                    break;
                }

                Trajectory.Add(nextPoint);
                currentPoint = nextPoint;
            }

            State = MissileState.Flying;
        }


        /// <summary>
        /// Retourne une copie de la trajectoire
        /// </summary>
        public List<Point> GetTrajectory()
        {
            return new List<Point>(Trajectory);
        }

        /// <summary>
        /// Restaure un missile depuis une sauvegarde
        /// Utilisé pour charger les missiles après un Load
        /// </summary>
        public void RestoreFromSave(Point launchPos, Point impactPos, int power, int direction, bool hitTarget)
        {
            Position = launchPos;
            Power = power;
            Direction = direction;
            State = MissileState.Destroyed;
            HitTarget = hitTarget;

            // Créer une trajectoire minimale avec juste le point d'impact
            Trajectory = new List<Point> { impactPos };
            CurrentStep = 0;
        }

        /// <summary>
        /// Vérifie si le missile entre en collision avec un point ennemi
        /// </summary>
        public bool CheckCollision(List<Point> enemyPoints)
        {
            foreach (var trajectoryPoint in Trajectory)
            {
                foreach (var enemyPoint in enemyPoints)
                {
                    if (trajectoryPoint == enemyPoint)
                    {
                        State = MissileState.Destroyed;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Dessine le missile sur le graphique (rectangle animé)
        /// </summary>
        public void paint(object sender, PaintEventArgs e)
        {
            if (State == MissileState.Flying && Trajectory.Count > 0)
            {
                // Dessiner uniquement le rectangle du missile à sa position actuelle
                if (CurrentStep < Trajectory.Count)
                {
                    Point missilePos = Trajectory[CurrentStep];

                    // Rectangle de taille GridSize/2 en largeur, GridSize/3 en hauteur
                    int rectWidth = GameConfig.GridSize / 2;
                    int rectHeight = GameConfig.GridSize / 3;

                    // Centrer verticalement sur la ligne
                    int rectX = missilePos.X - rectWidth / 2;
                    int rectY = missilePos.Y - rectHeight / 2;

                    // Dessiner le rectangle du missile
                    using (Brush brush = new SolidBrush(OwnerColor))
                    {
                        e.Graphics.FillRectangle(brush, rectX, rectY, rectWidth, rectHeight);
                    }

                    // Bordure noire pour mieux voir le missile
                    using (Pen pen = new Pen(Color.Black, 2))
                    {
                        e.Graphics.DrawRectangle(pen, rectX, rectY, rectWidth, rectHeight);
                    }
                }
            }
            else if (State == MissileState.Destroyed && Trajectory.Count > 0)
            {
                Point impact = GetCurrentPosition();
                if (HitTarget)
                {
                    // Collision: marquer avec un cercle plein
                    Color explosionColor = Color.FromArgb(170, OwnerColor.R, OwnerColor.G, OwnerColor.B);
                    using (Brush brush = new SolidBrush(explosionColor))
                    {
                        e.Graphics.FillEllipse(brush, impact.X - 10, impact.Y - 10, 20, 20);
                    }

                    using (Pen pen = new Pen(Color.FromArgb(220, OwnerColor), 2))
                    {
                        e.Graphics.DrawEllipse(pen, impact.X - 10, impact.Y - 10, 20, 20);
                    }
                }
                else
                {
                    // Pas de collision: marquer le point d'impact avec une croix
                    using (Pen crossPen = new Pen(Color.FromArgb(230, OwnerColor), 2))
                    {
                        int size = 8;
                        e.Graphics.DrawLine(crossPen, impact.X - size, impact.Y - size, impact.X + size, impact.Y + size);
                        e.Graphics.DrawLine(crossPen, impact.X - size, impact.Y + size, impact.X + size, impact.Y - size);
                    }
                }
            }
        }

        /// <summary>
        /// Avance le missile d'un pas dans sa trajectoire
        /// Retourne true si le missile est arrivé au bout
        /// </summary>
        public bool Step()
        {
            if (State == MissileState.Flying && CurrentStep < Trajectory.Count - 1)
            {
                CurrentStep++;
                return false;
            }
            return true; // Missile arrivé au bout
        }

        /// <summary>
        /// Retourne la position actuelle du missile dans l'animation
        /// </summary>
        public Point GetCurrentPosition()
        {
            if (CurrentStep < Trajectory.Count)
            {
                return Trajectory[CurrentStep];
            }
            return Trajectory.Count > 0 ? Trajectory[Trajectory.Count - 1] : Point.Empty;
        }
    }
}
