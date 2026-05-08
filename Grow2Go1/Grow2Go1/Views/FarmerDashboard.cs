using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard : Form
    {
        public FarmerDashboard()
        {
            InitializeComponent();
        }

        private void FarmerDashboard_Load(object sender, EventArgs e)
        {
            // Path to your shopping cart icon in Assets
            string iconPath = Path.Combine(Application.StartupPath, "Assets", "Cart_icon.png");

            // Configure Marketplace button
            btnMarketplace.Text = "Marketplace";
            btnMarketplace.Image = Image.FromFile(iconPath);
            btnMarketplace.ImageSize = new Size(20, 20);
            btnMarketplace.ImageAlign = HorizontalAlignment.Left;
            btnMarketplace.TextAlign = HorizontalAlignment.Right;
            btnMarketplace.FillColor = Color.Transparent;
            btnMarketplace.UseTransparentBackground = true;
            btnMarketplace.ForeColor = Color.White;
            btnMarketplace.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void btnMarketplace_Click(object sender, EventArgs e)
        {
            // Action when Marketplace is clicked
            MessageBox.Show("Marketplace clicked!");
            // Later: switch to Marketplace panel or open Marketplace form
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }
    }
}