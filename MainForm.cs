using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Threading;
using System.Windows.Forms;

#if !LINUX
using ScintillaNET;
#endif

namespace Pyramid
{
    public class MainForm : Form
    {
        public Hero Hero { get; private set; }

        public string Code {
            get { return editor.Text; }
            set { editor.Text = value; }
        }

        private List<Actor> actors;
        private TextBox editor;
        private Canvas fond;

        private ToolStrip toolStrip;
        private Thread runner;
        private System.Threading.Timer tick;

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
                AABB = {
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
            };
            var statusStrip = new StatusStrip
            {
                Parent = toolStripContainer.BottomToolStripPanel,
                Dock = DockStyle.Fill,
                LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow
            };
            /*var statusLabelStrip = new ToolStripStatusLabel
            {
                Dock = DockStyle.Fill,
                Text = "Status"
            };
            statusStrip.Items.AddRange(new ToolStripItem[]{ statusLabelStrip });
            */

            toolStrip.Items.AddRange(new ToolStripItem[]
                {
                    new ToolStripButton
                    {
                        Name = "run_button",
                        Text = "Exécuter (F5)",

                    }
                });

            toolStripContainer.TopToolStripPanel.Controls.Add(toolStrip);
            //toolStripContainer.BottomToolStripPanel.Controls.Add(statusStrip);

            var splitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Parent = toolStripContainer.ContentPanel
            };

#if !LINUX
            var scintilla = new Scintilla();
            // TODO
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

            fond = new Canvas {
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
            toolStrip.ItemClicked += onItemClicked;
            KeyDown += onKeyDown;

            FormClosing += onFormClosing;

            Load += (object sender, EventArgs e) => NewLevel(this, new EventArgs());
        }

        private void onItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            evaluate(editor.Text);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                evaluate(editor.Text);
            }
        }

        private void onTick()
        {
            try
            {
                Console.WriteLine("tick");
                Invoke(new MethodInvoker(onUpdate), null);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void onFormClosing(object sender, FormClosingEventArgs e)
        {
            if (tick != null)
            {
                tick.Change(Timeout.Infinite, Timeout.Infinite);
                tick.Dispose();
                tick = null;
            }
            if (runner != null)
            {
                runner.Join();
            }
        }

        private void onUpdate()
        {
            frame++;
            fond.Tick(frame);

            foreach (var actor in actors) {
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
            if (done)
            {
                Console.WriteLine("kill tick");

                tick.Change(Timeout.Infinite, Timeout.Infinite);
                tick.Dispose();
                tick = null;
            }
        }

        private void evaluate(string code)
        {
            // Wait for the previous thread to be done.
            if (runner != null)
            {
                Hero.Dead = true;
                runner.Join();
                Hero.Dead = false;
            }

            RunLevel(this, new EventArgs());
            var evaluator = new Evaluator
            {
                Code = code,
                Hero = Hero
            };
            Hero.Actors = actors;
            done = false;
            frame = 0;
            tick = new System.Threading.Timer(_ => onTick(), null, 0, 1000 / 60);

            runner = new Thread(new ThreadStart(evaluator.Run));
            runner.Start();
        }

        public void AddActor(Actor actor)
        {
            actors.Add(actor);                
            fond.AddActor(actor);
        }

        public void End()
        {
            //MessageBox.Show("Victoire!");
            done = true;
        }
    }
}