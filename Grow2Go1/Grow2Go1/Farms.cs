using Grow2Go1.Views;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Grow2Go1
{
    public partial class Farms : UserControl
    {
        private readonly string _connectionString =
            "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";

        private FlowLayoutPanel _farmsPanel;
        private bool _loaded = false;
        private Models.User _currentUser;
        private Panel _dropdownMenu;
        private bool _dropdownOpen = false;
        private DateTime _lastMenuClick = DateTime.MinValue;

        public event EventHandler<FarmEventArgs> ViewFarmProducts;
        public event EventHandler BackToMarketplace;
        public event EventHandler BackToFarmerDashboard;

        public Farms()
        {
            InitializeComponent();
        }

        public Farms(Models.User user) : this()
        {
            _currentUser = user;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WireButtons();

            if (!_loaded && this.Width > 300)
            {
                _loaded = true;
                SetupFarmsPanel();
                LoadFarms();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        private void WireButtons()
        {
            MenuButton.Click += MenuButton_Click;
            MarketplaceButton.Click += (s, e) => NavigateBack();
            FarmMapButton.Click += (s, e) => { };

            if (_currentUser?.Role == "farmer")
                MarketplaceButton.Visible = false;
        }

        private void NavigateBack()
        {
            if (_currentUser?.Role == "customer")
                BackToMarketplace?.Invoke(this, EventArgs.Empty);
            else
                BackToFarmerDashboard?.Invoke(this, EventArgs.Empty);
        }

        private void SetupFarmsPanel()
        {
            this.BackColor = Color.FromArgb(23, 34, 17);

            int topOffset = 345;

            var backBtn = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 36),
                Location = new Point(20, topOffset),
                Cursor = Cursors.Hand
            };
            backBtn.FlatAppearance.BorderSize = 0;
            backBtn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            backBtn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            backBtn.Click += (s, e) => NavigateBack();
            this.Controls.Add(backBtn);
            backBtn.BringToFront();

            var title = new Label
            {
                Text = _currentUser?.Role == "farmer" ? "🌿 Farms" : "🌿 Browse Farms",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(20, topOffset + 43),
                Size = new Size(400, 36)
            };
            this.Controls.Add(title);
            title.BringToFront();

            _farmsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, topOffset + 90),
                Size = new Size(this.Width - 40, this.Height - (topOffset + 110)),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(_farmsPanel);
            _farmsPanel.BringToFront();
        }

        private void LoadFarms()
        {
            _farmsPanel.Controls.Clear();

            var farms = new List<(int FarmId, int UserId, string FarmName, string Location,
                string Description, string Phone, string PicPath, int ProductCount, double AvgRating)>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string q = @"
                        SELECT f.farm_id, f.user_id, f.farm_name, f.location, f.description,
                               COALESCE(NULLIF(f.phone_number, ''), u.phone) AS phone_number,
                               f.profile_pic_path,
                               COUNT(DISTINCT p.product_id) AS product_count,
                               IFNULL(AVG(pr.rating), 0) AS avg_rating
                        FROM farms f
                        JOIN users u ON f.user_id = u.user_id
                        LEFT JOIN products p ON f.farm_id = p.farm_id AND p.is_available = 1
                        LEFT JOIN product_ratings pr ON p.product_id = pr.product_id
                        WHERE u.role = 'farmer'
                        GROUP BY f.farm_id, f.user_id, f.farm_name, f.location, f.description,
                                 f.phone_number, u.phone, f.profile_pic_path
                        ORDER BY f.farm_name ASC";

                    using (var cmd = new MySqlCommand(q, conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            farms.Add((
                                Convert.ToInt32(r["farm_id"]),
                                Convert.ToInt32(r["user_id"]),
                                r["farm_name"].ToString(),
                                r["location"].ToString(),
                                r["description"].ToString(),
                                r["phone_number"].ToString(),
                                r["profile_pic_path"].ToString(),
                                Convert.ToInt32(r["product_count"]),
                                Convert.ToDouble(r["avg_rating"])
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading farms: " + ex.Message);
                return;
            }

            if (farms.Count == 0)
            {
                _farmsPanel.Controls.Add(new Label
                {
                    Text = "No farms found.",
                    Font = new Font("Segoe UI", 13),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    Size = new Size(_farmsPanel.Width, 50),
                    TextAlign = ContentAlignment.MiddleCenter
                });
                return;
            }

            foreach (var farm in farms)
            {
                _farmsPanel.Controls.Add(CreateFarmCard(
                    farm.FarmId,
                    farm.UserId,
                    farm.FarmName,
                    farm.Location,
                    farm.Description,
                    farm.Phone,
                    farm.PicPath,
                    farm.ProductCount,
                    farm.AvgRating
                ));
            }
        }

        private Panel CreateFarmCard(int farmId, int farmUserId, string farmName, string location,
             string description, string phone, string picPath, int productCount, double avgRating)
        {
            int cardW = _farmsPanel.Width - 30;

            var card = new Panel
            {
                Size = new Size(cardW, 110),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    card.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            var picBox = new PictureBox
            {
                Location = new Point(16, 16),
                Size = new Size(78, 78),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(200, 200, 200)
            };

            picBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, picBox.Width - 1, picBox.Height - 1);
                    picBox.Region = new Region(gp);
                }
            };

            if (!string.IsNullOrEmpty(picPath) && File.Exists(picPath))
            {
                try { picBox.Image = Image.FromFile(picPath); } catch { }
            }

            card.Controls.Add(picBox);

            card.Controls.Add(new Label
            {
                Text = farmName,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(110, 12),
                Size = new Size(400, 26),
                AutoEllipsis = true
            });

            int filledStars = (int)Math.Round(avgRating);
            card.Controls.Add(new Label
            {
                Text = new string('★', filledStars) + new string('☆', 5 - filledStars) +
                       (avgRating > 0 ? "  " + avgRating.ToString("0.0") : "  No ratings"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Location = new Point(110, 38),
                Size = new Size(220, 20)
            });

            card.Controls.Add(new Label
            {
                Text = productCount + " products available",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(110, 58),
                Size = new Size(200, 18)
            });

            card.Controls.Add(new Label
            {
                Text = "📍 " + location,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                Location = new Point(110, 78),
                Size = new Size(300, 18)
            });

            card.Controls.Add(new Label
            {
                Text = "📞 " + (string.IsNullOrEmpty(phone) ? "N/A" : phone),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                Location = new Point(500, 38),
                Size = new Size(250, 18)
            });

            bool isOpen = productCount > 0;
            card.Controls.Add(new Label
            {
                Text = isOpen ? "Open" : "Closed",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isOpen ? Color.FromArgb(49, 91, 23) : Color.Red,
                BackColor = Color.Transparent,
                Location = new Point(cardW - 200, 20),
                Size = new Size(80, 24),
                TextAlign = ContentAlignment.MiddleRight
            });

            bool isCustomer = _currentUser?.Role == "customer";
            bool isFarmer = _currentUser?.Role == "farmer";
            bool isMyFarm = isFarmer && farmUserId == _currentUser.UserId;

            if (isCustomer || isMyFarm)
            {
                string btnText = isCustomer ? "View Products" : "Go to Farm Profile";

                var actionBtn = new Button
                {
                    Text = btnText,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(30, 50, 15),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(160, 36),
                    Location = new Point(cardW - 178, 58),
                    Cursor = Cursors.Hand
                };

                actionBtn.FlatAppearance.BorderSize = 0;

                actionBtn.Click += (s, e) =>
                {
                    if (isMyFarm)
                    {
                        BackToFarmerDashboard?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ViewFarmProducts?.Invoke(this, new FarmEventArgs
                        {
                            FarmId = farmId,
                            FarmName = farmName
                        });
                    }
                };

                card.Controls.Add(actionBtn);
            }

            return card;
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastMenuClick).TotalMilliseconds < 400) return;

            _lastMenuClick = DateTime.Now;

            if (_dropdownOpen) CloseDropdown();
            else OpenDropdown();
        }

        private void OpenDropdown()
        {
            _dropdownOpen = true;

            _dropdownMenu = new Panel
            {
                Size = new Size(220, _currentUser?.Role == "farmer" ? 180 : 320),
                BackColor = Color.FromArgb(30, 50, 15),
                Cursor = Cursors.Default
            };

            _dropdownMenu.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(_dropdownMenu.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.FromArgb(30, 50, 15)))
                {
                    _dropdownMenu.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                }
            };

            Point pos = this.PointToClient(
                MenuButton.Parent.PointToScreen(
                    new Point(MenuButton.Right - 220, MenuButton.Bottom + 5)));
            _dropdownMenu.Location = pos;

            var avatar = new Panel
            {
                Location = new Point(85, 16),
                Size = new Size(50, 50),
                BackColor = Color.Transparent
            };

            string avatarPicPath = "";

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string picQ = _currentUser?.Role == "farmer"
                        ? "SELECT profile_pic_path FROM farms WHERE user_id=@id"
                        : "SELECT profile_pic_path FROM users WHERE user_id=@id";

                    using (var cmd = new MySqlCommand(picQ, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser?.UserId);
                        avatarPicPath = cmd.ExecuteScalar()?.ToString() ?? "";
                    }
                }
            }
            catch { }

            string capturedPic = avatarPicPath;

            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var b = new Rectangle(0, 0, 49, 49);

                if (!string.IsNullOrEmpty(capturedPic) && File.Exists(capturedPic))
                {
                    try
                    {
                        using (var img = Image.FromFile(capturedPic))
                        using (var gp = new GraphicsPath())
                        {
                            gp.AddEllipse(b);
                            e.Graphics.SetClip(gp);
                            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            e.Graphics.DrawImage(img, b);
                            e.Graphics.ResetClip();
                        }
                    }
                    catch
                    {
                        DrawInitials(e.Graphics, b);
                    }
                }
                else
                {
                    DrawInitials(e.Graphics, b);
                }
            };

            _dropdownMenu.Controls.Add(avatar);

            _dropdownMenu.Controls.Add(new Label
            {
                Text = _currentUser?.FullName ?? "User",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 72),
                Size = new Size(220, 22)
            });

            _dropdownMenu.Controls.Add(new Label
            {
                Text = _currentUser?.Role == "farmer" ? "Farmer" : "Customer",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 220, 150),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 94),
                Size = new Size(220, 18)
            });

            _dropdownMenu.Controls.Add(new Panel
            {
                Location = new Point(15, 118),
                Size = new Size(190, 1),
                BackColor = Color.FromArgb(80, 100, 60)
            });

            if (_currentUser?.Role == "customer")
            {
                string[] menuItems = { "👤  View Profile", "📦  My Orders", "🛒  Cart", "🌿  Saved Products" };
                int itemY = 128;

                foreach (var item in menuItems)
                {
                    var btn = new Button
                    {
                        Text = item,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Location = new Point(10, itemY),
                        Size = new Size(200, 36),
                        Cursor = Cursors.Hand,
                        Padding = new Padding(10, 0, 0, 0)
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 90, 30);

                    btn.Click += (s, ev) =>
                    {
                        CloseDropdown();
                        NavigateBack();
                    };

                    _dropdownMenu.Controls.Add(btn);
                    itemY += 38;
                }
            }

            int logoutY = _currentUser?.Role == "farmer" ? 132 : 282;

            var logoutBtn = new Button
            {
                Text = "Logout",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 36),
                Location = new Point(20, logoutY),
                Cursor = Cursors.Hand
            };

            logoutBtn.FlatAppearance.BorderSize = 0;

            logoutBtn.Click += (s, ev) =>
            {
                CloseDropdown();
                var parent = this.FindForm();
                parent?.Hide();
                new LoginForm().Show();
            };

            _dropdownMenu.Controls.Add(logoutBtn);

            this.Controls.Add(_dropdownMenu);
            _dropdownMenu.BringToFront();
        }

        private void CloseDropdown()
        {
            if (_dropdownMenu != null)
            {
                this.Controls.Remove(_dropdownMenu);
                _dropdownMenu.Dispose();
                _dropdownMenu = null;
            }

            _dropdownOpen = false;
        }

        private void DrawInitials(Graphics g, Rectangle b)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                g.FillEllipse(brush, b);

            string initials = _currentUser?.FullName?.Length > 0
                ? _currentUser.FullName[0].ToString().ToUpper()
                : "?";

            using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                SizeF sz = g.MeasureString(initials, font);
                g.DrawString(initials, font, brush,
                    (b.Width - sz.Width) / 2,
                    (b.Height - sz.Height) / 2);
            }
        }

        private GraphicsPath MakeRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);  
            path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void Farms_Load(object sender, EventArgs e)
        {
        }
    }
}