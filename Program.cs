using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pyramid
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
            var form = new MainForm();
            form.NewLevel += onNewLevel;
            form.RunLevel += onRunLevel;
            form.DeadHero += onDeadHero;

            Application.EnableVisualStyles();
			Application.Run(form);
		}

        private static void onNewLevel(object sender, EventArgs e)
        {
            var form = (MainForm) sender;

            // do-while is buggy: https://github.com/RupertAvery/csharpeval/pull/10
            form.Code = "// Demo level\n\n"
                + "Hero.Right(100);\n"
                + "Hero.Up(50);\n\n"
                + "var ennemi = Hero.Radar();\n"
                + "while (ennemi != null && ennemi.Danger) {\n"
                + "    ennemi = Hero.Radar();\n"
                + "}\n\n"
                + "Hero.Up(150);\n"
                + "Hero.Right(100);\n";
        }

        private static void onRunLevel(object sender, EventArgs e)
        {
            var form = (MainForm) sender;

            form.Hero.AABB.X = 0;
            form.Hero.AABB.Y = 0;

            form.AddActor(
                new CrazyEnemi
                {
                    Color = Color.Blue,
                    AABB =
                    {
                        X = 100,
                        Y = 100,
                        Width = 50,
                        Height = 50,
                    }
                });
            form.AddActor(new ExitDoor
                {
                    Main = form,
                    Color = Color.Green,
                    AABB = {
                        X = 200,
                        Y = 200,
                        Width = 50,
                        Height = 50
                    }
                });
        }

        private static void onDeadHero(object sender, EventArgs e)
        {
            // Ceci ne peut pas être fait, car on est dans un autre thread...
            //MessageBox.Show("Votre héros est mort...");
            Console.WriteLine("Votre héros est mort...");
        }
	}

    public class CrazyEnemi : Actor
    {
        private Color[] colors = { Color.Gray, Color.Orange, Color.Red };
        private int round;

        public override void Tick(int frame)
        {
            round = (frame / 50) % colors.Length;

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