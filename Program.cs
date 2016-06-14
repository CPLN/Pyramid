using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pyramid
{
    public class Program
    {
        private static CrazyEnemy enemy;
        private static ExitDoor door;

        [STAThread]
        public static void Main(string[] args)
        {
            var form = new MainForm();
            form.NewLevel += OnNewLevel;
            form.RunLevel += OnRunLevel;
            form.DeadHero += OnDeadHero;

            Application.EnableVisualStyles();
            Application.Run(form);
        }

        private static void OnNewLevel(object sender, EventArgs e)
        {
            var form = (MainForm)sender;

            // do-while is buggy: https://github.com/RupertAvery/csharpeval/pull/10
            form.Code = @"// Demo level

Hero.Right(100);
Hero.Down(50);
//var ennemi = Hero.Radar();
//while (ennemi != null && ennemi.Danger) {
//    ennemi = Hero.Radar();
//}
Hero.Down(150);
Hero.Right(100);";

            enemy = new CrazyEnemy()
            {
                AABB =
                {
                    X = 100,
                    Y = 100,
                    Width = 50,
                    Height = 50,
                }
            };
            door = new ExitDoor()
            {
                Main = form,
                Color = Color.Green,
                AABB =
                {
                    X = 200,
                    Y = 200,
                    Width = 50,
                    Height = 50
                }
            };

            form.AddActor(enemy);
            form.AddActor(door);
        }

        private static void OnRunLevel(object sender, EventArgs e)
        {
            var form = (MainForm)sender;

            form.Hero.AABB.X = 0;
            form.Hero.AABB.Y = 0;

            Console.WriteLine(form.Hero.AABB);
        }

        private static void OnDeadHero(object sender, EventArgs e)
        {
            MessageBox.Show("Votre héros est mort...");
        }
    }

    public class CrazyEnemy : Actor
    {
        private Color[] colors = { Color.Gray, Color.Orange, Color.Red };
        private int round;

        public CrazyEnemy()
            : base()
        {
            Tick(0);
        }

        public override void Tick(int frame)
        {
            round = (frame / 30) % colors.Length;

            Color = colors[round];
            Danger = round > 0;
        }

        public override void Collision(Actor other)
        {
            // Kill
            other.Dead |= Color == Color.Red;
        }
    }

    public class ExitDoor : Actor
    {
        public MainForm Main;

        public override void Collision(Actor other)
        {
            Console.WriteLine("Victoire!");
            Main.End();
        }
    }
}