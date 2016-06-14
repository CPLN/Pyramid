using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

#if !LINUX
using ScintillaNET;
#endif

namespace Pyramid
{
    public class MainForm : Form
    {
        public Hero Hero { get; private set; }

        public string Code
        {
            get { return editor.Text; }
            set { editor.Text = value; }
        }

        private List<Actor> actors;
        private Control editor;
        private Canvas fond;

        private ToolStrip toolStrip;

        private int frame;
        private bool done;

        public event EventHandler NewLevel;
        public event EventHandler RunLevel;
        public event EventHandler DeadHero;

        public MainForm()
        {
            Text = "Lost in the Pyramid";
            frame = 0;

            Hero = new Hero
            {
                Color = Color.Blue,
                Speed = 5,
                AABB =
                {
                    X = 0,
                    Y = 0,
                    Width = 50,
                    Height = 50,
                }
            };

            actors = new List<Actor>();

            KeyPreview = true;

            initializeComponents();
            initializeEvents();
        }

        private void initializeComponents()
        {
            var fontFamily = "Consolas";
            var fontSize = 14;

            var toolStripContainer = new ToolStripContainer
            {
                Parent = this,
                Dock = DockStyle.Fill,
            };
            toolStrip = new ToolStrip
            {
                Name = "Toolbar"
            };

            toolStrip.Items.AddRange(new ToolStripItem[]
                {
                    new ToolStripButton
                    {
                        Name = "run_button",
                        Text = "Exécuter (F5)",

                    }
                });

            toolStripContainer.TopToolStripPanel.Controls.Add(toolStrip);

            var splitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Parent = toolStripContainer.ContentPanel
            };

#if !LINUX
            var scintilla = new Scintilla();
            scintilla.Margins[0].Width = 0;

            scintilla.Lexer = Lexer.Cpp;
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = fontFamily;
            scintilla.Styles[Style.Default].Size = fontSize;
            scintilla.StyleClearAll();

            scintilla.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using var virtual while");
            scintilla.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");

            var Green = Color.FromArgb(0, 128, 0);
            var Gray = Color.FromArgb(128, 128, 128);
            var Red = Color.FromArgb(163, 21, 21);
            scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            scintilla.Styles[Style.Cpp.Comment].ForeColor = Green;
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Green;
            scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Gray;
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.String].ForeColor = Red;
            scintilla.Styles[Style.Cpp.Character].ForeColor = Red;
            scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Red;
            scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
#else
            var scintilla = new TextBox {
                WordWrap = true,
                Multiline = true,
                Font = new Font(fontFamily, fontSize, FontStyle.Regular)
            };
#endif

            scintilla.Dock = DockStyle.Fill;
            scintilla.Parent = splitter.Panel1;
            scintilla.Text = "// Bienvenue dans l'éditeur C#\n\n";

            editor = scintilla;

            fond = new Canvas
            {
                Parent = splitter.Panel2,
                Dock = DockStyle.Fill
            };
            fond.AddActor(Hero);

            splitter.Panel1.Controls.Add(scintilla);
            splitter.Panel2.Controls.Add(fond);
            toolStripContainer.ContentPanel.Controls.Add(splitter);
            Controls.Add(toolStripContainer);
        }

        private void initializeEvents()
        {
            toolStrip.ItemClicked += OnItemClicked;
            KeyDown += OnKeyDown;

            Load += (object sender, EventArgs e) => NewLevel(this, new EventArgs());
        }

        private void OnItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Evaluate(editor.Text);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                Evaluate(editor.Text);
            }
        }

        private void Evaluate(string code)
        {
            // Dispatch the signal
            RunLevel(this, new EventArgs());

            // reset
            Hero.Actors = actors;
            Hero.Dead = false;
            done = false;
            frame = 0;

            var globals = new Globals { Hero = Hero };
            var timeTask = new Task(OnTick);
            var heroTask = new Task(() =>
            {
                globals.Run(code);
                End();
            });

            // FIXME: kill them, eventually.
            // Because this program leaks like crazy ;-)
            timeTask.Start();
            heroTask.Start();
        }


        private async void OnTick()
        {
            Invoke(new MethodInvoker(OnUpdate), null);
            while (!done)
            {
                await Task.Delay(1000 / 60);
                Invoke(new MethodInvoker(OnUpdate), null);
            }
        }

        private void OnUpdate()
        {
            frame++;
            fond.Tick(frame);

            foreach (var actor in actors)
            {
                if (actor.AABB.IntersectsWith(Hero.AABB))
                {
                    actor.Collision(Hero);
                    Hero.Collision(actor);
                }
            }

            fond.Invalidate();
            fond.Update();

            if (Hero.Dead)
            {
                done = true;
                DeadHero(this, new EventArgs());
            }
        }

        public void AddActor(Actor actor)
        {
            actors.Add(actor);
            fond.AddActor(actor);
        }

        public void End()
        {
            done = true;
        }
    }
}