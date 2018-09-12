using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winrun
{
    public partial class MainForm : Form
    {
        KeyboardHook hook = new KeyboardHook();
        Dictionary<string, string> database = new Dictionary<string, string>();
                                                            //skrót, lokalizacja
        bool first_time = true;
        Point location;
        readonly string cfg_file_location = Environment.CurrentDirectory + @"\shortcuts.acfg";
        const char seperator = '|';

        public MainForm()
        {
            this.Opacity = 0;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            location = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (this.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (this.Size.Height / 2));

            if (!File.Exists(cfg_file_location))
            {
                File.Create(cfg_file_location);
                MessageBox.Show("File shortcuts.acfg was not found.\nProgram will now exit.", "Winrun");
                hook.Dispose();
                Environment.Exit(2);
            }

            notifyIcon.Visible = true;
            LoadDatabase();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(ShowWindow);
            hook.RegisterHotKey(winrun.ModifierKeys.Win, Keys.W); //Win + W
            ShowWindow(null, null);
        }

        private void ShowWindow(object sender, KeyPressedEventArgs e)
        {
            this.Location = location;
            InputBox.Text = null;
            this.Show();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Execute(InputBox.Text);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
        }

        private void LoadDatabase()
        {
            InputBox.Items.Clear();
            database.Clear();
            database.Add(String.Empty, null); 
            using (StringReader reader = new StringReader(File.ReadAllText(cfg_file_location)))
            {
                string line;
                string[] tokens;
                uint current_line = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    current_line++;
                    tokens = line.Split(seperator);

                    if (tokens.Length != 2)
                    {
                        MessageBox.Show("Invalid shortcut at line:" + current_line.ToString() + ".\nProgram will now exit.", "Winrun");
                        hook.Dispose();
                        Environment.Exit(3);
                    }

                    database.Add(tokens[0],tokens[1]);
                    InputBox.Items.Add(tokens[0]);
                    //inputBox.AutoCompleteCustomSource.Add(tokens[0]);
                }
            }
        }

        private void Execute(string command)
        {
            string dupa = database.FirstOrDefault(t => t.Key.StartsWith(command)).Value;
            if(dupa != null)
            {
                Process.Start(dupa);
                this.Hide();
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow(null, null);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if(first_time)
            {
                this.Hide();
                this.Opacity = 100;
                first_time = false;
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            hook.Dispose();
            Environment.Exit(1);
        }

        private void ShowMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow(null,null);
        }

        private void InputBox_TextChanged(object sender, EventArgs e)
        {
            string dupa = database.FirstOrDefault(t => t.Key.StartsWith(InputBox.Text)).Key;
            if (dupa != null)
                CorrectLabel.Text = dupa;
            else
                CorrectLabel.Text = null;
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            Execute(InputBox.Text);
        }
    }
}
