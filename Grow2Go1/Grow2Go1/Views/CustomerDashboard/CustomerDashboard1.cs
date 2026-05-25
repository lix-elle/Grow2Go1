using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grow2Go1.Views.CustomerDashboard
{
    public partial class CustomerDashboard1 : Form
    {
        public CustomerDashboard1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#4CAF50");
            guna2GradientPanel1.FillColor = ColorTranslator.FromHtml("#4CAF50"); // Green
            guna2GradientPanel1.FillColor2 = ColorTranslator.FromHtml("#F44336"); // Red
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {
            // Hide any child control whose name contains 'logout', otherwise hide the entire panel as a fallback.
            bool found = false;
            foreach (Control c in guna2Panel2.Controls)
            {
                if (!string.IsNullOrEmpty(c.Name) && c.Name.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    c.Visible = false;
                    found = true;
                }
            }
            if (!found)
            {
                guna2Panel2.Visible = false;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
