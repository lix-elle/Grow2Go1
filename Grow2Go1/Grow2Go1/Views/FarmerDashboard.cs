using Grow2Go.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard : Form
    {
        private User _currentUser;

        public FarmerDashboard()
        {
            InitializeComponent();
        }

        public FarmerDashboard(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void FarmerDashboard_Load(object sender, EventArgs e)
        {
            if (_currentUser != null)
                this.Text = "Farmer Dashboard - " + _currentUser.FullName;

            // Configure Marketplace button.
            // The cart icon comes from embedded resources via the designer.
            MarketplaceButton.Text = "Marketplace";
            MarketplaceButton.ImageSize = new Size(20, 20);
            MarketplaceButton.ImageAlign = HorizontalAlignment.Left;
            MarketplaceButton.TextAlign = HorizontalAlignment.Right;
            MarketplaceButton.FillColor = Color.Transparent;
            MarketplaceButton.UseTransparentBackground = true;
            MarketplaceButton.ForeColor = Color.White;
            MarketplaceButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
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

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}