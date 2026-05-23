using Grow2Go.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard : Form
    {
        private User _currentUser;

        // Section UserControls (lazy-instantiated on first navigation).
        private FarmerDashboard1 _productsView;
        private FarmerDashboard2 _ordersView;
        private FarmerDashboard3 _farmProfileView;

        // Snapshot of the controls that make up the Overview screen,
        // so we can hide them as a group when a section is shown.
        private List<Control> _overviewControls;

        public FarmerDashboard()
        {
            InitializeComponent();
        }

        public FarmerDashboard(User user) : this()
        {
            _currentUser = user;
        }

        private void FarmerDashboard_Load(object sender, EventArgs e)
        {
            // Configure Marketplace button styling
            MarketplaceButton.Text = "Marketplace";
            MarketplaceButton.TextAlign = HorizontalAlignment.Right;
            MarketplaceButton.FillColor = Color.Transparent;
            MarketplaceButton.UseTransparentBackground = true;
            MarketplaceButton.ForeColor = Color.White;
            MarketplaceButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            MarketplaceButton.ImageSize = new Size(20, 20);
            MarketplaceButton.ImageAlign = HorizontalAlignment.Left;

            if (_currentUser != null)
            {
                this.Text = "Farmer Dashboard - " + _currentUser.FullName;
            }

            // Remember which controls make up the Overview view.
            _overviewControls = this.Controls.Cast<Control>().ToList();

            // Wire the four top-nav tabs on the main form.
            OverviewButton.Click += (s, ev) => ShowOverview();
            ProductsButton.Click += (s, ev) => ShowProducts();
            OrdersButton.Click += (s, ev) => ShowOrders();
            FarmProfileButton.Click += (s, ev) => ShowFarmProfile();

            ShowOverview();
        }

        private void ShowOverview()
        {
            HideAllSections();
            foreach (var c in _overviewControls) c.Visible = true;
            HighlightActiveTab(OverviewButton);
        }

        private void ShowProducts()
        {
            if (_productsView == null)
            {
                _productsView = new FarmerDashboard1();
                MountSection(_productsView);
            }
            ActivateSection(_productsView);
            HighlightActiveTab(ProductsButton);
        }

        private void ShowOrders()
        {
            if (_ordersView == null)
            {
                _ordersView = new FarmerDashboard2();
                MountSection(_ordersView);
            }
            ActivateSection(_ordersView);
            HighlightActiveTab(OrdersButton);
        }

        private void ShowFarmProfile()
        {
            if (_farmProfileView == null)
            {
                _farmProfileView = new FarmerDashboard3();
                MountSection(_farmProfileView);
            }
            ActivateSection(_farmProfileView);
            HighlightActiveTab(FarmProfileButton);
        }

        private void MountSection(UserControl section)
        {
            section.Dock = DockStyle.Fill;
            section.Visible = false;
            this.Controls.Add(section);
            WireSectionNav(section);
        }

        private void ActivateSection(UserControl section)
        {
            // Hide overview content
            foreach (var c in _overviewControls) c.Visible = false;
            // Hide other sections
            if (_productsView != null && _productsView != section) _productsView.Visible = false;
            if (_ordersView != null && _ordersView != section) _ordersView.Visible = false;
            if (_farmProfileView != null && _farmProfileView != section) _farmProfileView.Visible = false;

            section.Visible = true;
            section.BringToFront();
        }

        private void HideAllSections()
        {
            if (_productsView != null) _productsView.Visible = false;
            if (_ordersView != null) _ordersView.Visible = false;
            if (_farmProfileView != null) _farmProfileView.Visible = false;
        }

        // Each section UserControl embeds its own nav (OverviewButton, ProductsButton,
        // OrdersButton, FarmProfileButton). Route those clicks back to the parent
        // switching logic so users can navigate between sections from any view.
        private void WireSectionNav(Control container)
        {
            foreach (Control c in container.Controls)
            {
                switch (c.Name)
                {
                    case "OverviewButton":
                        c.Click += (s, e) => ShowOverview();
                        break;
                    case "ProductsButton":
                        c.Click += (s, e) => ShowProducts();
                        break;
                    case "OrdersButton":
                        c.Click += (s, e) => ShowOrders();
                        break;
                    case "FarmProfileButton":
                        c.Click += (s, e) => ShowFarmProfile();
                        break;
                }
                if (c.HasChildren) WireSectionNav(c);
            }
        }

        private void HighlightActiveTab(Guna.UI2.WinForms.Guna2Button active)
        {
            var tabs = new[] { OverviewButton, ProductsButton, OrdersButton, FarmProfileButton };
            foreach (var tab in tabs)
            {
                bool isActive = tab == active;
                tab.FillColor = isActive ? Color.White : Color.Transparent;
                tab.ForeColor = isActive ? Color.Black : Color.Black;
            }
        }

        private void btnMarketplace_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Marketplace clicked!");
        }

        private void guna2Button4_Click(object sender, EventArgs e) { }
        private void guna2Button1_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e) { }
    }
}
