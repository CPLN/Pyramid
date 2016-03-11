using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Pyramid
{
    public class Actor
    {
        public bool Danger;
        public bool Dead;
        public Color Color;
        public float Speed;
        public PointF Direction;
        public RectangleF AABB;

        protected int SleepTime = 1000 / 30;

        public Actor()
        {
            Speed = 1;
            Danger = false;
            Dead = false;
        }

        public virtual void Tick(int frame)
        {
            // Do nothing.
        }

        public void Paint(Graphics g)
        {
            g.DrawRectangle(
                new Pen(Color, 1f),
                Rectangle.Round(AABB)
            );
        }

        public virtual void Collision(Actor other)
        {
            // Do nothing is case of a collision
        }
    }

    public class Hero : Actor
    {
        public int X { get { return (int) (AABB.X + .5); } }
        public int Y { get { return (int) (AABB.Y + .5); } }
        public int Width { get { return (int) (AABB.Width + .5); } }
        public int Height { get { return (int) (AABB.Height + .5); } }

        public List<Actor> Actors {
            set { actors = value; }
        }

        private List<Actor> actors;


        /// <summary>
        /// Radar gives us the other actor that are in sight or null if none is found.
        /// </summary>
        public Actor Radar() {
            if (Dead)
                return null;

            Thread.Sleep(SleepTime);

            var dx = 0;
            var dy = 0;
            var width = Width;
            var height = Height;

            if (Direction.X > 0)
            {
                width /= 2;
                dx += Width;

            }
            else if(Direction.X < 0)
            {
                width /= 2;
                dx -= width;
            }
            else if (Direction.Y > 0)
            {
                height /= 2;
                dy += Height;

            }
            else if(Direction.Y < 0)
            {
                height /= 2;
                dy -= height;
            }

            var view = new RectangleF {
                X = AABB.X + dx,
                Y = AABB.Y + dy,
                Width = width,
                Height = height
            };
            foreach(var actor in actors) {
                if (view.IntersectsWith(actor.AABB)) {
                    return actor;
                }
            }
            return null;
        }

        /// <summary>
        /// Move the Hero to the left by steps pixels
        /// </summary>
        /// <param name="steps">number of pixels to move left</param>
        public void Left(int steps) {
            Direction.X = -1;
            Direction.Y = 0;
            Move(steps);
        }

        /// <summary>
        /// Move the Hero to the right by steps pixels
        /// </summary>
        /// <param name="steps">number of pixels to move right</param>
        public void Right(int steps) {
            Direction.X = 1;
            Direction.Y = 0;
            Move(steps);
        }

        /// <summary>
        /// Move the Hero to the bottom by steps pixels
        /// </summary>
        /// <param name="steps">number of pixels to move down</param>
        public void Down(int steps) {
            Direction.X = 0;
            Direction.Y = -1;
            Move(steps);
        }

        /// <summary>
        /// Move the Hero to the top by steps pixels
        /// </summary>
        /// <param name="steps">number of pixels to move up</param>
        public void Up(int steps) {
            Direction.X = 0;
            Direction.Y = 1;
            Move(steps);
        }

        protected void Move(int steps) {
            while (steps > 0 && !Dead)
            {
                AABB.X += Direction.X * Speed;
                AABB.Y += Direction.Y * Speed;
                Thread.Sleep(SleepTime);
                steps -= (int) Speed;
            }
        }
    }
}

