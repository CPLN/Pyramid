using System;
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
        private Actor actor;
        private TextBox editor;
        private Canvas fond;
        private ToolStrip toolStrip;
        private Thread runner;
        private System.Threading.Timer tick;
        private int frame;

        public MainForm()
        {
            frame = 0;

            actor = new Actor
            {
                Color = Color.Red,
                    Speed = {
                        X = 5,
                        Y = 5
                    },
                AABB = {
                    X = 0,
                    Y = 0,
                    Width = 50,
                    Height = 50,
                },
            };

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
            var statusLabelStrip = new ToolStripStatusLabel
            {
                Dock = DockStyle.Fill,
                Text = "Status"
            };
            statusStrip.Items.AddRange(new ToolStripItem[]{ statusLabelStrip });

            toolStrip.Items.AddRange(new ToolStripItem[]
                {
                    new ToolStripButton
                    {
                        Name = "run_button",
                        Text = "Exécuter",
                    }
                });

            toolStripContainer.TopToolStripPanel.Controls.Add(toolStrip);
            toolStripContainer.BottomToolStripPanel.Controls.Add(statusStrip);

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
            scintilla.Text = "// Bienvenue dans l'éditeur C#\n\n"
                + "Actor.Move(10);\n"
                + "Console.WriteLine(\"{0}; {1}\", Actor.AABB.X, Actor.AABB.Y);";

            editor = scintilla;

            fond = new Canvas {
                Parent = splitter.Panel2,
                Dock = DockStyle.Fill
            };
            fond.AddActor(actor);

            splitter.Panel1.Controls.Add(scintilla);
            splitter.Panel2.Controls.Add(fond);
            toolStripContainer.ContentPanel.Controls.Add(splitter);
            Controls.Add(toolStripContainer);
        }

        private void initializeEvents()
        {
            toolStrip.ItemClicked += onItemClicked;

            FormClosing += onFormClosing;

            tick = new System.Threading.Timer(_ => onTick(), null, 1000, 1000 / 60);
        }

        private void onItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Console.WriteLine("Clicked on {0}", ((ToolStripItem) e.ClickedItem).Text);

            evaluate(editor.Text);
        }

        private void onTick()
        {
            try
            {
                Invoke(new MethodInvoker(onUpdate), null);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void onFormClosing(object sender, FormClosingEventArgs e)
        {
            tick.Change(Timeout.Infinite, Timeout.Infinite);
            tick.Dispose();
            tick = null;
            if (runner != null)
            {
                runner.Join();
            }
        }

        private void onUpdate()
        {
            fond.Invalidate();
        }

        private void evaluate(string code)
        {
            var evaluator = new Evaluator
            {
                Code = code,
                Actor = actor
            };
            runner = new Thread(new ThreadStart(evaluator.Run));
            runner.Start();
        }
    }
}