using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Pyramid
{
    public class Canvas : Control
    {
        public readonly List<Actor> actors;

        public Canvas()
        {
            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            BackColor = Color.Transparent;

            actors = new List<Actor>();
        }


        public void AddActor(Actor actor)
        {
            actors.Add(actor);
        }

        public void Tick(int frame)
        {
            foreach (var actor in actors)
            {
                actor.Tick(frame);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = e.ClipRectangle;
            var state = e.Graphics.Save();

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            // Center the display.
            //e.Graphics.TranslateTransform(rect.Right / 2f, rect.Bottom / 2f);
            // Put the Y pointing at the top.
            //e.Graphics.ScaleTransform(1, -1);

            foreach (var actor in actors)
            {
                actor.Paint(e.Graphics);
            }

            e.Graphics.Restore(state);
        }
    }
}

