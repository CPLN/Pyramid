using System;
using System.Drawing;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (var actor in actors)
            {
                actor.Paint(e.Graphics);
            }
        }
    }
}

