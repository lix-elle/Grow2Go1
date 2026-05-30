using Grow2Go1.Models;
using Grow2Go.Repositories;
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
        private Panel _farmerDropdown;
        private bool _farmerDropdownOpen = false;
        private DateTime _lastFarmerMenuClick = DateTime.MinValue;

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
        private void FarmerMenuButton_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastFarmerMenuClick).TotalMilliseconds < 400)
                return;

            _lastFarmerMenuClick = DateTime.Now;

            if (_farmerDropdownOpen)
                CloseFarmerDropdown();
            else
                OpenFarmerDropdown();
        }

        private void CloseFarmerDropdown()
        {
            if (_farmerDropdown != null)
            {
                this.Controls.Remove(_farmerDropdown);
                _farmerDropdown.Dispose();
                _farmerDropdown = null;
            }
            _farmerDropdownOpen = false;
        }
        private void DrawInitialsAvatar(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                g.FillEllipse(brush, bounds);
            string initials = _currentUser?.FullName?.Length > 0
                ? _currentUser.FullName[0].ToString().ToUpper() : "?";
            using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                SizeF sz = g.MeasureString(initials, font);
                g.DrawString(initials, font, brush,
                    (bounds.Width - sz.Width) / 2,
                    (bounds.Height - sz.Height) / 2);
            }
        }
        private void OpenFarmerDropdown()
        {
            _farmerDropdownOpen = true;

            _farmerDropdown = new Panel
            {
                Size = new Size(220, 200),
                BackColor = Color.FromArgb(30, 50, 15),
                Cursor = Cursors.Default
            };

            _farmerDropdown.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = MakeDropdownRoundedRect(_farmerDropdown.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.FromArgb(30, 50, 15)))
                {
                    _farmerDropdown.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                }
            };

            // Position below MenuButton
            Point pos = this.PointToClient(
                MenuButton.Parent.PointToScreen(
                    new Point(MenuButton.Right - 220, MenuButton.Bottom + 5)));
            _farmerDropdown.Location = pos;

            // ── Avatar ────────────────────────────────────────────────────────
            var avatar = new Panel
            {
                Location = new Point(85, 16),
                Size = new Size(50, 50),
                BackColor = Color.Transparent
            };
            // Load farm profile pic path from DB
            string farmPicPath = "";
            try
            {
                int farmId = GetCurrentFarmId();
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(
                    "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;"))
                {
                    conn.Open();
                    var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                        "SELECT profile_pic_path FROM farms WHERE farm_id = @farmId", conn);
                    cmd.Parameters.AddWithValue("@farmId", farmId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        farmPicPath = result.ToString();
                }
            }
            catch { }

            string capturedFarmPic = farmPicPath;
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var bounds = new Rectangle(0, 0, 49, 49);

                if (!string.IsNullOrEmpty(capturedFarmPic) && System.IO.File.Exists(capturedFarmPic))
                {
                    try
                    {
                        using (var img = System.Drawing.Image.FromFile(capturedFarmPic))
                        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            path.AddEllipse(bounds);
                            e.Graphics.SetClip(path);
                            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            e.Graphics.DrawImage(img, bounds);
                            e.Graphics.ResetClip();
                        }
                        // Border ring
                        using (var pen = new Pen(Color.FromArgb(180, 220, 150), 2))
                            e.Graphics.DrawEllipse(pen, bounds);
                    }
                    catch
                    {
                        DrawInitialsAvatar(e.Graphics, bounds);
                    }
                }
                else
                {
                    DrawInitialsAvatar(e.Graphics, bounds);
                }
            };
            _farmerDropdown.Controls.Add(avatar);

            // ── Farmer name ───────────────────────────────────────────────────
            _farmerDropdown.Controls.Add(new Label
            {
                Text = _currentUser?.FullName ?? "Farmer",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 72),
                Size = new Size(220, 22)
            });

            // ── Role label ────────────────────────────────────────────────────
            _farmerDropdown.Controls.Add(new Label
            {
                Text = "Farmer",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 220, 150),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 94),
                Size = new Size(220, 18)
            });

            // ── Divider ───────────────────────────────────────────────────────
            _farmerDropdown.Controls.Add(new Panel
            {
                Location = new Point(15, 118),
                Size = new Size(190, 1),
                BackColor = Color.FromArgb(80, 100, 60)
            });

            // ── Logout button ─────────────────────────────────────────────────
            var logoutBtn = new Button
            {
                Text = "Logout",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 36),
                Location = new Point(20, 148),
                Cursor = Cursors.Hand
            };
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.Click += (s, e) =>
            {
                CloseFarmerDropdown();
                this.Hide();
                var login = new LoginForm();
                login.Show();
            };
            _farmerDropdown.Controls.Add(logoutBtn);

            this.Controls.Add(_farmerDropdown);
            _farmerDropdown.BringToFront();
        }

        private System.Drawing.Drawing2D.GraphicsPath MakeDropdownRoundedRect(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
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
            MenuButton.Click += FarmerMenuButton_Click;

            // ── Load overview stats ──
            LoadOverviewStats();
            LoadRecentOrders();

            ShowOverview();
        }
        private void LoadRecentOrders()
        {
            try
            {
                // Clear existing content inside RecentOrder panel
                var toRemove = RecentOrder.Controls.Cast<Control>()
                    .Where(c => c.Name != "lblNoOrders" && c.Name != "lblRecentTitle" && c.Name != "lblRecentSub")
                    .ToList();
                foreach (var c in toRemove) { RecentOrder.Controls.Remove(c); c.Dispose(); }

                int farmId = GetCurrentFarmId();
                var repo = new OrderRepository();
                var orders = repo.GetOrdersByFarm(farmId);

                // Take only last 5 orders
                var recent = orders.Take(5).ToList();

                if (recent.Count == 0)
                {
                    if (!RecentOrder.Controls.ContainsKey("lblNoOrders"))
                    {
                        RecentOrder.Controls.Add(new Label
                        {
                            Name = "lblNoOrders",
                            Text = "No Orders Available",
                            Font = new Font("Segoe UI", 11),
                            ForeColor = Color.FromArgb(180, 180, 180),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Size = new Size(RecentOrder.Width, 40),
                            Location = new Point(0, RecentOrder.Height / 2 - 20),
                            BackColor = Color.Transparent
                        });
                    }
                    return;
                }

                // Remove "No Orders" label if exists
                if (RecentOrder.Controls.ContainsKey("lblNoOrders"))
                    RecentOrder.Controls.RemoveByKey("lblNoOrders");

                int rowY = 10;

                foreach (var order in recent)
                {
                    Color statusColor = order.Status == "pending" ? Color.FromArgb(255, 165, 0) :
                                        order.Status == "confirmed" ? Color.FromArgb(49, 91, 23) :
                                        order.Status == "shipped" ? Color.FromArgb(33, 150, 243) :
                                        order.Status == "completed" ? Color.FromArgb(0, 128, 0) :
                                                                      Color.FromArgb(200, 50, 50);

                    var row = new Panel
                    {
                        Location = new Point(10, rowY),
                        Size = new Size(RecentOrder.Width - 30, 44),
                        BackColor = Color.FromArgb(248, 248, 248),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };

                    row.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                    };

                    // Order ID
                    row.Controls.Add(new Label
                    {
                        Text = "ORD-" + order.OrderId.ToString("D3"),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(20, 20, 20),
                        BackColor = Color.Transparent,
                        Location = new Point(12, 12),
                        Size = new Size(100, 20)
                    });

                    // Customer name
                    row.Controls.Add(new Label
                    {
                        Text = order.CustomerName,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(80, 80, 80),
                        BackColor = Color.Transparent,
                        Location = new Point(120, 12),
                        Size = new Size(200, 20)
                    });

                    // Date
                    row.Controls.Add(new Label
                    {
                        Text = order.CreatedAt.ToString("MMM dd, yyyy"),
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.Gray,
                        BackColor = Color.Transparent,
                        Location = new Point(330, 12),
                        Size = new Size(120, 20)
                    });

                    // Total ── moved left to avoid overlap
                    row.Controls.Add(new Label
                    {
                        Text = "₱" + order.TotalAmount.ToString("0.00"),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(49, 91, 23),
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleRight,
                        Location = new Point(row.Width - 260, 12),
                        Size = new Size(120, 20),
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    });

                    // Status badge ── wider to fit full text
                    row.Controls.Add(new Label
                    {
                        Text = char.ToUpper(order.Status[0]) + order.Status.Substring(1),
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = statusColor,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(row.Width - 130, 9),
                        Size = new Size(120, 24),
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    });

                    RecentOrder.Controls.Add(row);
                    rowY += 54;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadRecentOrders error: " + ex.Message);
            }
        }

        // ── Loads real counts into the Overview stat cards ───────────────────
        private void LoadOverviewStats()
        {
            if (_currentUser == null) return;

            try
            {
                int farmId = GetCurrentFarmId();
                var connStr = "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";

                // ── Total Products ────────────────────────────────────────────────
                var productRepo = new ProductRepository(connStr);
                var products = productRepo.GetProductsByFarm(farmId);
                TotalProdNum.Text = products.Count.ToString();

                // ── Active Orders (pending + confirmed + shipped) ─────────────────
                // ── Monthly Revenue (completed orders this month) ─────────────────
                // ── Total Customers (distinct customers who ordered) ──────────────
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
                {
                    conn.Open();

                    // Active Orders
                    string activeQuery = @"
                SELECT COUNT(*) FROM orders
                WHERE farm_id = @farmId
                AND status IN ('pending', 'confirmed', 'shipped')";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(activeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        ActiveOrdNum.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Monthly Revenue
                    string revenueQuery = @"
                SELECT IFNULL(SUM(total_amount), 0) FROM orders
                WHERE farm_id    = @farmId
                AND status       = 'completed'
                AND MONTH(created_at) = MONTH(CURDATE())
                AND YEAR(created_at)  = YEAR(CURDATE())";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(revenueQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        decimal revenue = Convert.ToDecimal(cmd.ExecuteScalar());
                        MonthlyRevNum.Text = "₱" + revenue.ToString("0.00");
                    }

                    // Average Rating
                    string ratingQuery = @"
    SELECT IFNULL(AVG(pr.rating), 0)
    FROM product_ratings pr
    JOIN products p ON pr.product_id = p.product_id
    WHERE p.farm_id = @farmId";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(ratingQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        double avgRating = Convert.ToDouble(cmd.ExecuteScalar());
                        AverageRateNum.Text = avgRating > 0 ? avgRating.ToString("0.0") + " ★" : "N/A";
                    }

                    // Total Customers
                    string customerQuery = @"
                SELECT COUNT(DISTINCT customer_id) FROM orders
                WHERE farm_id = @farmId";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(customerQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", farmId);
                        TotalCostumerNum.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadOverviewStats error: " + ex.Message);
                TotalProdNum.Text = "0";
                ActiveOrdNum.Text = "0";
                MonthlyRevNum.Text = "₱0.00";
                AverageRateNum.Text = "N/A";
                TotalCostumerNum.Text = "0";
            }
        }

        private void ShowOverview()
        {
            HideAllSections();
            foreach (var c in _overviewOnlyControls) c.Visible = true;
            HighlightActiveTab(OverviewButton);

            // Refresh stats every time Overview is shown
            LoadOverviewStats();
            LoadRecentOrders();
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
        private void FarmMapButton_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.Controls)
                if (c.Name == "farmsOverlay") { this.Controls.Remove(c); c.Dispose(); break; }

            var farmsUC = new Farms(_currentUser);
            farmsUC.Name = "farmsOverlay";
            farmsUC.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
            farmsUC.Location = new Point(0, 0);
            farmsUC.BackColor = Color.FromArgb(23, 34, 17);  // ← solid dark bg matching theme
            farmsUC.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                             AnchorStyles.Right | AnchorStyles.Bottom;

            farmsUC.BackToFarmerDashboard += (s, ev) =>
            {
                this.Controls.Remove(farmsUC);
                farmsUC.Dispose();
            };

            this.Controls.Add(farmsUC);
            farmsUC.BringToFront();
        }
        private void ProductsButton_Click(object sender, EventArgs e) { }
        private void MarketplaceButton1_Click(object sender, EventArgs e) { }
        private void MarketplaceButton_Click(object sender, EventArgs e) { }
        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}