using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chess0._0
{
    public partial class Menu : Form
    {
        Label copyright;
        bool G;

        public Menu()
        {
            InitializeComponent();

            this.Icon = new Icon("../../Resources/icon1.ico");
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            copyright = new Label();
            copyright.Location = new Point(710, 620);
            copyright.Size = new Size(300, 45);
            copyright.Text = "-------(c)2023--------\nMihai Ghergheles";
            copyright.ForeColor = Color.White;
            copyright.BackColor = Color.Transparent;
            copyright.Parent = this;
            copyright.Anchor = AnchorStyles.None;

            pictureBox1.BackgroundImage = Image.FromFile(@"../../Resources/bobby_fisher.png");
            label3.Text = "Tribute to GM Bobby Fisher (1943 - 2008)";
            label3.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel);

            engine.flip = false;
        }

        private void Menu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //engine.flip = G;
                Form1 x = new Form1();
                this.Hide();
                x.ShowDialog();
                this.Close();
            }
            if (e.KeyCode == Keys.G)
            {
                G = !G;
                if (G)
                {
                    pictureBox1.BackgroundImage = Image.FromFile(@"../../Resources/emory_andrew_iii.png");
                    label3.Text = "Tribute to GM Emory Andrew (1958 - 2015)";
                    label3.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel);
                }
                else
                {
                    pictureBox1.BackgroundImage = Image.FromFile(@"../../Resources/bobby_fisher.png");
                    label3.Text = "Tribute to GM Bobby Fisher (1943 - 2008)";
                    label3.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel);
                }
            }
            if (e.KeyCode == Keys.Escape) Application.Exit();
            if (e.KeyCode == Keys.F1) Process.Start("https://en.wikipedia.org/wiki/Rules_of_chess");
        }
    }
}
