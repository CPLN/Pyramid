using System.Drawing;
using System.Threading;

namespace Pyramid
{
    public class Actor
    {
        public bool Dead;
        public Color Color;
        public PointF Speed;
        public RectangleF AABB;

        protected int SleepTime = 1000 / 30;

        public Actor()
        {
            Dead = false;
        }

        public void Move(int steps) {
            while (steps > 0)
            {
                AABB.X += Speed.X;
                AABB.Y += Speed.Y;
                Thread.Sleep(SleepTime);
                steps--;
            }
        }

        public void Paint(Graphics g)
        {
            g.DrawRectangle(
                new Pen(Color, 1f),
                Rectangle.Round(AABB)
            );
        }
    }
}

