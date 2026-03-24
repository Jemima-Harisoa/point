using System.Windows.Forms;

namespace point
{
    /// <summary>
    /// Panel personnalisé avec double buffering activé pour réduire le flickering.
    /// Le double buffering permet de dessiner d'abord sur un buffer en mémoire,
    /// puis de copier le buffer sur l'écran en une seule fois.
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
