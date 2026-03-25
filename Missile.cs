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
        private List<Point> Trajectory { get; set; }
        #endregion

        public Missile(Color ownerColor)
        {
            OwnerColor = ownerColor;
            State = MissileState.Ready;
            Trajectory = new List<Point>();
        }

        /// <summary>
        /// Lance le missile depuis une ligne donnée avec une puissance (1-9)
        /// Trajectoire toujours horizontale, de gauche à droite
        /// </summary>
        public void Launch(Point from, int power)
        {
            Position = from;
            Power = power;
            Trajectory = new List<Point>();

            int maxWidth = GameConfig.GridColumns * GameConfig.GridSize;
            Point currentPoint = from;

            // Calcul du nombre de cases à parcourir
            // Puissance 9 = jusqu'à la dernière colonne
            int maxSteps = power;

            for (int i = 0; i < maxSteps; i++)
            {
                Point nextPoint = new Point(currentPoint.X + GameConfig.GridSize, currentPoint.Y);

                // Vérifier que le point reste dans les limites horizontales
                if (nextPoint.X >= maxWidth)
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
        /// Dessine le missile sur le graphique
        /// </summary>
        public void paint(object sender, PaintEventArgs e)
        {
            if (State == MissileState.Flying && Trajectory.Count > 0)
            {
                // Dessiner la ligne de trajectoire
                using (Pen pen = new Pen(OwnerColor, 3))
                {
                    for (int i = 0; i < Trajectory.Count - 1; i++)
                    {
                        e.Graphics.DrawLine(pen, Trajectory[i], Trajectory[i + 1]);
                    }
                }

                // Dessiner une flèche/cercle au bout de la trajectoire
                Point impact = Trajectory[Trajectory.Count - 1];
                using (Brush brush = new SolidBrush(OwnerColor))
                {
                    e.Graphics.FillEllipse(brush, impact.X - 4, impact.Y - 4, 8, 8);
                }
            }
            else if (State == MissileState.Destroyed && Trajectory.Count > 0)
            {
                // Dessiner un cercle orange/rouge semi-transparent pour montrer l'impact
                Point impact = Trajectory[Trajectory.Count - 1];
                Color explosionColor = Color.FromArgb(180, 255, 80, 0);
                using (Brush brush = new SolidBrush(explosionColor))
                {
                    e.Graphics.FillEllipse(brush, impact.X - 10, impact.Y - 10, 20, 20);
                }
            }
        }
    }
}
