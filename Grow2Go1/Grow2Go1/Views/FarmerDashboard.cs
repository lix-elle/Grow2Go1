using Grow2Go.Models;
using Grow2Go.Repositories;
using Grow2Go1.Models;
using Grow2Go1.Repositories;
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

        private FarmerDashboard1 _productsView;
        private FarmerDashboard2 _ordersView;
        private FarmerDashboard3 _farmProfileView;

        private static readonly HashSet<string> ChromeNames = new HashSet<string>
        {
            "Logo", "MenuButton", "FarmMapButton", "MarketplaceButton",
            "FarmerDashboardLabel", "Tagline",
            "OverviewButton", "ProductsButton", "OrdersButton", "FarmProfileButton",
            "guna2CustomGradientPanel1", "guna2CustomGradientPanel2"
        };

        private List<Control> _overviewOnlyControls;

        private const int SectionTop = 375;
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
            var tabFont = new Font("Segoe UI", 14F, FontStyle.Bold);
            OverviewButton.Font = tabFont;
            ProductsButton.Font = tabFont;
            OrdersButton.Font = tabFont;
            FarmProfileButton.Font = tabFont;

            if (_currentUser != null)
                this.Text = "Farmer Dashboard - " + _currentUser.FullName;

            // Snapshot the overview-only controls (everything that isn't chrome)
            _overviewOnlyControls = this.Controls.Cast<Control>()
                .Where(c => !ChromeNames.Contains(c.Name))
                .ToList();

            OverviewButton.Click += (s, ev) => ShowOverview();
            ProductsButton.Click += (s, ev) => ShowProducts();
            OrdersButton.Click += (s, ev) => ShowOrders();
            FarmProfileButton.Click += (s, ev) => ShowFarmProfile();

            // ── Load overview stats ──
            LoadOverviewStats();

            ShowOverview();
        }

        // ── Loads real counts into the Overview stat cards ───────────────────
        private void LoadOverviewStats()
        {
            if (_currentUser == null) return;

            try
            {
                int farmId = GetCurrentFarmId();
                var connStr = "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";
                var repo = new ProductRepository(connStr);
                List<Product> products = repo.GetProductsByFarm(farmId);

                // Update Total Products card
                TotalProdNum.Text = products.Count.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadOverviewStats error: " + ex.Message);
                TotalProdNum.Text = "0";
            }
        }

        private void ShowOverview()
        {
            HideAllSections();
            foreach (var c in _overviewOnlyControls) c.Visible = true;
            HighlightActiveTab(OverviewButton);

            // Refresh stats every time Overview is shown
            LoadOverviewStats();
        }

        private void ShowProducts()
        {
            if (_productsView == null)
            {
                int farmId = GetCurrentFarmId();
                _productsView = new FarmerDashboard1(farmId);
                MountSection(_productsView);
            }
            ActivateSection(_productsView);
            HighlightActiveTab(ProductsButton);
        }

        private int GetCurrentFarmId()
        {
            if (_currentUser == null) return 0;
            var userRepo = new UserRepository();
            return userRepo.GetFarmIdByUserId(_currentUser.UserId);
        }

        private void ShowOrders()
        {
            if (_ordersView == null)
            {
                int farmId = GetCurrentFarmId();
                _ordersView = new FarmerDashboard2(farmId);
                MountSection(_ordersView);
            }
            ActivateSection(_ordersView);
            HighlightActiveTab(OrdersButton);
        }

        private void ShowFarmProfile()
        {
            if (_farmProfileView == null)
            {
                int farmId = GetCurrentFarmId();
                _farmProfileView = new FarmerDashboard3(farmId, _currentUser.UserId);
                MountSection(_farmProfileView);
            }
            ActivateSection(_farmProfileView);
            HighlightActiveTab(FarmProfileButton);
        }

        private void MountSection(UserControl section)
        {
            HideChromeIn(section);
            ShiftContentToTop(section);

            section.Location = new Point(SectionSidePad, SectionTop);
            section.Size = new Size(
                this.ClientSize.Width - SectionSidePad * 2,
                this.ClientSize.Height - SectionTop - SectionBottomPad);
            section.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                AnchorStyles.Right | AnchorStyles.Bottom;
            section.AutoScroll = true;
            section.BackColor = this.BackColor;
            section.Visible = false;
            this.Controls.Add(section);
        }

        private void ActivateSection(UserControl section)
        {
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
                if (ChromeNames.Contains(c.Name)) c.Visible = false;
        }

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

        // ── Keep these so the designer doesn't break ──
        private void btnMarketplace_Click(object sender, EventArgs e) { }
        private void guna2Button4_Click(object sender, EventArgs e) { }
        private void guna2Button1_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e) { }
        private void FarmMapButton_Click(object sender, EventArgs e) { }
        private void ProductsButton_Click(object sender, EventArgs e) { }
        private void MarketplaceButton1_Click(object sender, EventArgs e) { }
        private void MarketplaceButton_Click(object sender, EventArgs e) { }
        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}