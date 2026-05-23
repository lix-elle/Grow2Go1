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

        // Persistent dashboard chrome (header bar, title, tab strip).
        // These stay visible at all times; only the overview content swaps out.
        private static readonly HashSet<string> ChromeNames = new HashSet<string>
        {
            "Logo", "MenuButton", "FarmMapButton", "MarketplaceButton",
            "FarmerDashboardLabel", "Tagline",
            "OverviewButton", "ProductsButton", "OrdersButton", "FarmProfileButton",
            "guna2CustomGradientPanel1", "guna2CustomGradientPanel2"
        };

        // Overview-only controls (stat cards, recent-orders panel) - hidden
        // when a section UC is shown.
        private List<Control> _overviewOnlyControls;

        // Section host area sits just below the tab strip.
        private const int SectionTop = 460;
        private const int SectionSidePad = 20;
        private const int SectionBottomPad = 10;

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
            // Marketplace button look (designer already binds the cart icon).
            MarketplaceButton.Text = "Marketplace";
            MarketplaceButton.TextAlign = HorizontalAlignment.Right;
            MarketplaceButton.FillColor = Color.Transparent;
            MarketplaceButton.UseTransparentBackground = true;
            MarketplaceButton.ForeColor = Color.White;
            MarketplaceButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            MarketplaceButton.ImageSize = new Size(20, 20);
            MarketplaceButton.ImageAlign = HorizontalAlignment.Left;

            // Shrink the oversized designer fonts at runtime so they don't get
            // overwritten every time VS regenerates Designer.cs.
            FarmerDashboardLabel.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            FarmerDashboardLabel.Size = new Size(700, 70);
            FarmerDashboardLabel.Location = new Point(57, 210);

            Tagline.Font = new Font("Segoe UI", 13F, FontStyle.Regular);
            Tagline.Size = new Size(600, 32);
            Tagline.Location = new Point(65, 290);

            var tabFont = new Font("Segoe UI", 14F, FontStyle.Bold);
            OverviewButton.Font = tabFont;
            ProductsButton.Font = tabFont;
            OrdersButton.Font = tabFont;
            FarmProfileButton.Font = tabFont;

            if (_currentUser != null)
            {
                this.Text = "Farmer Dashboard - " + _currentUser.FullName;
            }

            // Snapshot the overview-only controls (everything that isn't chrome).
            _overviewOnlyControls = this.Controls.Cast<Control>()
                .Where(c => !ChromeNames.Contains(c.Name))
                .ToList();

            OverviewButton.Click += (s, ev) => ShowOverview();
            ProductsButton.Click += (s, ev) => ShowProducts();
            OrdersButton.Click += (s, ev) => ShowOrders();
            FarmProfileButton.Click += (s, ev) => ShowFarmProfile();

            ShowOverview();
        }

        private void ShowOverview()
        {
            HideAllSections();
            foreach (var c in _overviewOnlyControls) c.Visible = true;
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
            // The teammate built each section as a full-page layout that bakes
            // in its own copy of the logo/nav/title/tab strip. Hide that
            // duplicate chrome and slide the remaining section-specific content
            // up so the section starts cleanly below the parent's tab strip.
            HideChromeIn(section);
            ShiftContentToTop(section);

            section.Location = new Point(SectionSidePad, SectionTop);
            section.Size = new Size(
                this.ClientSize.Width - SectionSidePad * 2,
                this.ClientSize.Height - SectionTop - SectionBottomPad);
            section.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            section.AutoScroll = true;
            section.BackColor = this.BackColor;
            section.Visible = false;
            this.Controls.Add(section);
        }

        private void ActivateSection(UserControl section)
        {
            // Hide ONLY overview content; the chrome stays visible.
            foreach (var c in _overviewOnlyControls) c.Visible = false;
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

        private static void HideChromeIn(Control container)
        {
            foreach (Control c in container.Controls)
            {
                if (ChromeNames.Contains(c.Name)) c.Visible = false;
            }
        }

        // After hiding the chrome, the section-specific content (which the
        // teammate placed at y~470+ in the UC) leaves a big empty band at top.
        // Slide everything still visible up so the section starts at y=20.
        private static void ShiftContentToTop(Control container)
        {
            var visible = container.Controls.Cast<Control>().Where(c => c.Visible).ToList();
            if (visible.Count == 0) return;
            int minY = visible.Min(c => c.Top);
            int offset = minY - 20;
            if (offset <= 0) return;
            foreach (var c in visible) c.Top -= offset;
        }

        private void HighlightActiveTab(Guna.UI2.WinForms.Guna2Button active)
        {
            var tabs = new[] { OverviewButton, ProductsButton, OrdersButton, FarmProfileButton };
            foreach (var tab in tabs)
            {
                tab.FillColor = tab == active ? Color.White : Color.Transparent;
                tab.ForeColor = Color.Black;
            }
        }

        private void btnMarketplace_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Marketplace clicked!");
        }

        private void guna2Button4_Click(object sender, EventArgs e) { }
        private void guna2Button1_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e) { }
        private void FarmMapButton_Click(object sender, EventArgs e) { }
        private void ProductsButton_Click(object sender, EventArgs e) { }
    }
}
