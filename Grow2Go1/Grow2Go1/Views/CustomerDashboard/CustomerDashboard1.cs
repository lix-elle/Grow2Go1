using Grow2Go.Helpers;
using Grow2Go1.Models;

using Grow2Go1.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace Grow2Go1.Views.CustomerDashboard
{
    public partial class CustomerDashboard1 : Form
    {
        private User _currentUser;
        private List<Product> _allProducts = new List<Product>();
        private Panel _contentPanel;
        private FlowLayoutPanel _productsPanel;
        private Panel _filterPanel;
        private ComboBox _cmbCategory;
        private ComboBox _cmbSortBy;
        private TextBox _txtMinPrice;
        private TextBox _txtMaxPrice;
        private CartRepository _cartRepo = new CartRepository();
        private Panel _dropdownMenu;
        private List<CartItem> _cart = new List<CartItem>();
        private bool _dropdownOpen = false;
        private bool _filterVisible = false;
        private int _searchRowBottom;

        private int _modalRating = 0;
        private int _modalQuantity = 1;
        private List<Label> _modalStars = new List<Label>();

        private readonly Color _navGreen = Color.FromArgb(68, 110, 21);
        private readonly string _connectionString =
            "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";

        public CustomerDashboard1(User user)
        {
            InitializeComponent();
            _currentUser = user;
            this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnFilters.Click += BtnFilters_Click;
            this.Shown += CustomerDashboard1_Shown;
            txtSearch.TextChanged += (s, ev) => ApplyFilters();
            MenuButton.Click += MenuButton_Click;
        }
        private void ShowMyOrdersPanel()
        {
            // Remove existing if open
            foreach (Control c in this.Controls)
                if (c.Name == "ordersPanel") { this.Controls.Remove(c); c.Dispose(); break; }

            var ordersPanel = new Panel
            {
                Name = "ordersPanel",
                Size = new Size(420, this.ClientSize.Height - 120),
                Location = new Point(this.ClientSize.Width - 430, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            ordersPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(ordersPanel.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    ordersPanel.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Header ────────────────────────────────────────────────────────
            ordersPanel.Controls.Add(new Label
            {
                Text = "📦  My Orders",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 15),
                BackColor = Color.Transparent,
                Location = new Point(16, 16),
                Size = new Size(280, 30)
            });

            // ── Close button ──────────────────────────────────────────────────
            var closeBtn = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(378, 14),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) =>
            {
                this.Controls.Remove(ordersPanel);
                ordersPanel.Dispose();
            };
            ordersPanel.Controls.Add(closeBtn);

            // ── Divider ───────────────────────────────────────────────────────
            ordersPanel.Controls.Add(new Panel
            {
                Location = new Point(0, 52),
                Size = new Size(420, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Load orders ───────────────────────────────────────────────────
            var repo = new OrderRepository();
            var orders = repo.GetOrdersByCustomer(_currentUser.UserId);

            if (orders.Count == 0)
            {
                ordersPanel.Controls.Add(new Label
                {
                    Text = "You have no orders yet.",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 160),
                    Size = new Size(420, 40)
                });
            }
            else
            {
                // Scrollable area for orders
                var scrollPanel = new Panel
                {
                    Location = new Point(0, 58),
                    Size = new Size(420, ordersPanel.Height - 58),
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };
                ordersPanel.Controls.Add(scrollPanel);

                int itemY = 10;

                foreach (var order in orders)
                {
                    // Status color
                    Color statusColor = order.Status == "pending" ? Color.FromArgb(255, 165, 0) :
                    order.Status == "confirmed" ? Color.FromArgb(49, 91, 23) :
                    order.Status == "shipped" ? Color.FromArgb(33, 150, 243) :
                    order.Status == "completed" ? Color.FromArgb(0, 128, 0) :
                                                  Color.FromArgb(200, 50, 50);

                    // Order card
                    var card = new Panel
                    {
                        Location = new Point(10, itemY),
                        Size = new Size(390, 110),
                        BackColor = Color.FromArgb(248, 248, 248)
                    };
                    card.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var path = MakeRoundedRect(card.ClientRectangle, 10))
                        using (var brush = new SolidBrush(Color.FromArgb(248, 248, 248)))
                        using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                        {
                            card.Region = new Region(path);
                            e.Graphics.FillPath(brush, path);
                            e.Graphics.DrawPath(pen, path);
                        }
                    };

                    // Order ID + Farm
                    card.Controls.Add(new Label
                    {
                        Text = "Order #" + order.OrderId + "  —  " + order.FarmName,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(20, 20, 20),
                        BackColor = Color.Transparent,
                        Location = new Point(12, 10),
                        Size = new Size(280, 22)
                    });

                    // Status badge
                    card.Controls.Add(new Label
                    {
                        Text = order.Status.ToUpper(),
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        ForeColor = statusColor,
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleRight,
                        Location = new Point(270, 10),
                        Size = new Size(108, 22)
                    });

                    // Date
                    card.Controls.Add(new Label
                    {
                        Text = order.CreatedAt.ToString("MMM dd, yyyy  hh:mm tt"),
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.Gray,
                        BackColor = Color.Transparent,
                        Location = new Point(12, 34),
                        Size = new Size(366, 18)
                    });

                    // Delivery + Payment
                    card.Controls.Add(new Label
                    {
                        Text = "🚚 " + order.DeliveryMode + "   💵 " + order.PaymentMethod,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(80, 80, 80),
                        BackColor = Color.Transparent,
                        Location = new Point(12, 54),
                        Size = new Size(366, 18)
                    });

                    // Total
                    card.Controls.Add(new Label
                    {
                        Text = "Total:  ₱" + order.TotalAmount.ToString("0.00"),
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.FromArgb(49, 91, 23),
                        BackColor = Color.Transparent,
                        Location = new Point(12, 76),
                        Size = new Size(366, 24)
                    });
                    var capturedOrder = order;
                    card.Cursor = Cursors.Hand;
                    card.Click += (s, e) => ShowOrderDetailPanel(capturedOrder);
                    foreach (Control ctrl in card.Controls)
                        ctrl.Click += (s, e) => ShowOrderDetailPanel(capturedOrder);

                    scrollPanel.Controls.Add(card);
                    itemY += 120;
                }
            }

            this.Controls.Add(ordersPanel);
            ordersPanel.BringToFront();
        }
        private void ShowOrderDetailPanel(Order order)
        {
            // Remove existing detail panel
            foreach (Control c in this.Controls)
                if (c.Name == "orderDetailPanel") { this.Controls.Remove(c); c.Dispose(); break; }

            var detailPanel = new Panel
            {
                Name = "orderDetailPanel",
                Size = new Size(420, this.ClientSize.Height - 120),
                Location = new Point(this.ClientSize.Width - 430, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            detailPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(detailPanel.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    detailPanel.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Back button ───────────────────────────────────────────────────
            var backBtn = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(10, 14),
                Size = new Size(90, 30),
                Cursor = Cursors.Hand
            };
            backBtn.FlatAppearance.BorderSize = 0;
            backBtn.Click += (s, e) =>
            {
                this.Controls.Remove(detailPanel);
                detailPanel.Dispose();
                ShowMyOrdersPanel();
            };
            detailPanel.Controls.Add(backBtn);

            // ── Header ────────────────────────────────────────────────────────
            detailPanel.Controls.Add(new Label
            {
                Text = "Order #" + order.OrderId,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 15),
                BackColor = Color.Transparent,
                Location = new Point(16, 50),
                Size = new Size(280, 28)
            });

            // Status
            Color statusColor = order.Status == "pending" ? Color.FromArgb(255, 165, 0) :
                    order.Status == "confirmed" ? Color.FromArgb(49, 91, 23) :
                    order.Status == "shipped" ? Color.FromArgb(33, 150, 243) :
                    order.Status == "completed" ? Color.FromArgb(0, 128, 0) :
                                                  Color.FromArgb(200, 50, 50);
            detailPanel.Controls.Add(new Label
            {
                Text = order.Status.ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = statusColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(280, 50),
                Size = new Size(128, 28)
            });

            // Farm + Date
            detailPanel.Controls.Add(new Label
            {
                Text = "🌿 " + order.FarmName,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                Location = new Point(16, 82),
                Size = new Size(390, 22)
            });

            detailPanel.Controls.Add(new Label
            {
                Text = "📅 " + order.CreatedAt.ToString("MMM dd, yyyy  hh:mm tt"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(16, 106),
                Size = new Size(390, 20)
            });

            detailPanel.Controls.Add(new Label
            {
                Text = "🚚 " + order.DeliveryMode + "   💵 " + order.PaymentMethod,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                Location = new Point(16, 128),
                Size = new Size(390, 20)
            });
            string deliveryDateText = order.EstimatedDelivery.HasValue
              ? "📦 Est. " + order.DeliveryMode + " by: " +
                   order.EstimatedDelivery.Value.ToString("MMM dd, yyyy")
             : "📦 Estimated delivery not set";

            detailPanel.Controls.Add(new Label
            {
                Text = deliveryDateText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                Location = new Point(16, 150),
                Size = new Size(390, 22)
            });

            // ── Divider ───────────────────────────────────────────────────────
            detailPanel.Controls.Add(new Panel
            {
                Location = new Point(0, 178),
                Size = new Size(420, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Items header ──────────────────────────────────────────────────
            detailPanel.Controls.Add(new Label
            {
                Text = "Items Ordered",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(16, 186),
                Size = new Size(200, 24)
            });

            // ── Load order items ──────────────────────────────────────────────
            var repo = new OrderRepository();
            var items = repo.GetOrderItems(order.OrderId);

            var scrollPanel = new Panel
            {
                Location = new Point(0, 214),
                Size = new Size(420, detailPanel.Height - 450),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            detailPanel.Controls.Add(scrollPanel);

            int itemY = 8;
            foreach (var item in items)
            {
                // Product name
                scrollPanel.Controls.Add(new Label
                {
                    Text = item.ProductName,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 20, 20),
                    BackColor = Color.Transparent,
                    Location = new Point(16, itemY),
                    Size = new Size(240, 22)
                });

                // Subtotal
                scrollPanel.Controls.Add(new Label
                {
                    Text = "₱" + (item.UnitPrice * item.Quantity).ToString("0.00"),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(49, 91, 23),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(260, itemY),
                    Size = new Size(140, 22)
                });

                // Qty x price
                scrollPanel.Controls.Add(new Label
                {
                    Text = "Qty: " + item.Quantity + "  ×  ₱" + item.UnitPrice.ToString("0"),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    Location = new Point(16, itemY + 22),
                    Size = new Size(380, 18)
                });

                itemY += 52;

                // Divider
                scrollPanel.Controls.Add(new Panel
                {
                    Location = new Point(16, itemY - 4),
                    Size = new Size(380, 1),
                    BackColor = Color.FromArgb(235, 235, 235)
                });
            }
            // Only show cancel button if order is pending
            if (order.Status == "pending")
            {
                var cancelBtn = new Button
                {
                    Text = "✕  Cancel Order",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(200, 50, 50),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(180, 40),
                    Location = new Point(16, detailPanel.Height - 150),
                    Cursor = Cursors.Hand
                };
                cancelBtn.FlatAppearance.BorderSize = 0;
                cancelBtn.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show(
                        "Are you sure you want to cancel Order #" + order.OrderId + "?",
                        "Cancel Order",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        var cancelRepo = new OrderRepository();
                        bool success = cancelRepo.CancelOrder(order.OrderId);

                        if (success)
                        {
                            // Reload products (stock restored)
                            LoadProducts();

                            MessageBox.Show("Order #" + order.OrderId + " has been cancelled.",
                                "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Go back to orders list
                            this.Controls.Remove(detailPanel);
                            detailPanel.Dispose();
                            ShowMyOrdersPanel();
                        }
                        else
                        {
                            MessageBox.Show("Failed to cancel order. Please try again.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };
                detailPanel.Controls.Add(cancelBtn);
            }
            if (order.Status == "shipped")
            {
                var confirmDeliveryBtn = new Button
                {
                    Text = "✅  Confirm Delivery",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(49, 91, 23),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(200, 40),
                    Location = new Point(16, detailPanel.Height - 150),
                    Cursor = Cursors.Hand
                };
                confirmDeliveryBtn.FlatAppearance.BorderSize = 0;
                confirmDeliveryBtn.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show(
                        "Confirm that you received Order #" + order.OrderId + "?",
                        "Confirm Delivery",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm == DialogResult.Yes)
                    {
                        var deliveryrepo = new OrderRepository();
                        bool success = deliveryrepo.UpdateOrderStatus(order.OrderId, "completed");

                        if (success)
                        {
                            MessageBox.Show("Thank you! Order #" + order.OrderId +
                                " marked as completed.",
                                "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.Controls.Remove(detailPanel);
                            detailPanel.Dispose();
                            ShowMyOrdersPanel();
                        }
                    }
                };
                detailPanel.Controls.Add(confirmDeliveryBtn);
            }

            // ── Total ─────────────────────────────────────────────────────────
            detailPanel.Controls.Add(new Panel
            {
                Location = new Point(0, detailPanel.Height - 70),
                Size = new Size(420, 1),
                BackColor = Color.FromArgb(200, 200, 200)
            });

            detailPanel.Controls.Add(new Label
            {
                Text = "Total:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(16, detailPanel.Height - 60),
                Size = new Size(100, 28)
            });

            detailPanel.Controls.Add(new Label
            {
                Text = "₱" + order.TotalAmount.ToString("0.00"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(200, detailPanel.Height - 60),
                Size = new Size(200, 28)
            });

            this.Controls.Add(detailPanel);
            detailPanel.BringToFront();
        }
      

        private void CustomerDashboard1_Shown(object sender, EventArgs e)
        {
            Point abs = this.PointToClient(
                btnFilters.Parent.PointToScreen(new Point(0, btnFilters.Bottom)));
            _searchRowBottom = abs.Y;

            SetupContentPanel();
            SetupFilterPanel();
            SetupProductsPanel();
            LoadProducts();
            _cart = _cartRepo.GetCart(_currentUser.UserId);
        }

        // ── MAIN CONTENT PANEL ───────────────────────────────────────────────
        private void SetupContentPanel()
        {
            int contentY = _searchRowBottom + 30;
            _contentPanel = new Panel
            {
                Location = new Point(0, contentY),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - contentY),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                            AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(_contentPanel);
            _contentPanel.BringToFront();
        }

        // ── FILTER PANEL ─────────────────────────────────────────────────────
        private void SetupFilterPanel()
        {
            _filterPanel = new Panel
            {
                Location = new Point(20, 10),
                Size = new Size(_contentPanel.Width - 40, 85),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _filterPanel.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(_filterPanel.ClientRectangle, 15))
                using (var brush = new SolidBrush(_navGreen))
                    ev.Graphics.FillPath(brush, path);
            };

            _filterPanel.Controls.Add(new Label
            {
                Text = "Category",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            });

            _cmbCategory = new ComboBox
            {
                Location = new Point(20, 36),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 80, 50),
                ForeColor = Color.White
            };
            _cmbCategory.Items.AddRange(new object[] {
                "All","Fruits","Vegetables","Herbs","Dairy & Eggs","Meat","Grains"
            });
            _cmbCategory.SelectedIndex = 0;
            _cmbCategory.SelectedIndexChanged += (s, ev) => ApplyFilters();
            _filterPanel.Controls.Add(_cmbCategory);

            _filterPanel.Controls.Add(new Label
            {
                Text = "Sort By",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(250, 12),
                Size = new Size(100, 20),
                BackColor = Color.Transparent
            });

            _cmbSortBy = new ComboBox
            {
                Location = new Point(250, 36),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 80, 50),
                ForeColor = Color.White
            };
            _cmbSortBy.Items.AddRange(new object[] {
                "Price: Low to High","Price: High to Low",
                "Rating: Low to High","Rating: High to Low",
                "Name: A-Z","Name: Z-A"
            });
            _cmbSortBy.SelectedIndex = 0;
            _cmbSortBy.SelectedIndexChanged += (s, ev) => ApplyFilters();
            _filterPanel.Controls.Add(_cmbSortBy);

            _filterPanel.Controls.Add(new Label
            {
                Text = "Price Range",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(510, 12),
                Size = new Size(120, 20),
                BackColor = Color.Transparent
            });

            _txtMinPrice = new TextBox
            {
                Location = new Point(510, 36),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 10),
                Text = "Min ₱",
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(45, 80, 50),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtMinPrice.Enter += (s, ev) => {
                if (_txtMinPrice.Text == "Min ₱")
                { _txtMinPrice.Text = ""; _txtMinPrice.ForeColor = Color.White; }
            };
            _txtMinPrice.Leave += (s, ev) => {
                if (string.IsNullOrWhiteSpace(_txtMinPrice.Text))
                { _txtMinPrice.Text = "Min ₱"; _txtMinPrice.ForeColor = Color.Gray; }
            };
            _txtMinPrice.TextChanged += (s, ev) => ApplyFilters();
            _filterPanel.Controls.Add(_txtMinPrice);

            _filterPanel.Controls.Add(new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(638, 38),
                Size = new Size(20, 22),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            });

            _txtMaxPrice = new TextBox
            {
                Location = new Point(665, 36),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 10),
                Text = "Max ₱",
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(45, 80, 50),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtMaxPrice.Enter += (s, ev) => {
                if (_txtMaxPrice.Text == "Max ₱")
                { _txtMaxPrice.Text = ""; _txtMaxPrice.ForeColor = Color.White; }
            };
            _txtMaxPrice.Leave += (s, ev) => {
                if (string.IsNullOrWhiteSpace(_txtMaxPrice.Text))
                { _txtMaxPrice.Text = "Max ₱"; _txtMaxPrice.ForeColor = Color.Gray; }
            };
            _txtMaxPrice.TextChanged += (s, ev) => ApplyFilters();
            _filterPanel.Controls.Add(_txtMaxPrice);

            _contentPanel.Controls.Add(_filterPanel);
        }

        private void BtnFilters_Click(object sender, EventArgs e)
        {
            _filterVisible = !_filterVisible;
            _filterPanel.Visible = _filterVisible;
            RepositionProductsPanel();
        }

        // ── PRODUCTS PANEL ───────────────────────────────────────────────────
        private void SetupProductsPanel()
        {
            _productsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 10),
                Size = new Size(_contentPanel.Width - 20, _contentPanel.Height - 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                AnchorStyles.Right | AnchorStyles.Bottom
            };
            _productsPanel.HorizontalScroll.Maximum = 0;
            _productsPanel.HorizontalScroll.Enabled = false;
            _productsPanel.HorizontalScroll.Visible = false;
            _productsPanel.AutoScroll = true;
            _contentPanel.Controls.Add(_productsPanel);
            _productsPanel.BringToFront();
        }

        private void RepositionProductsPanel()
        {
            if (_productsPanel == null) return;
            int productsY = _filterVisible ? _filterPanel.Bottom + 10 : 10;
            _productsPanel.Location = new Point(20, productsY);
            _productsPanel.Size = new Size(
                _contentPanel.Width - 20,
                _contentPanel.Height - productsY - 10);
            _productsPanel.Refresh();
        }

        // ── LOAD FROM DB ─────────────────────────────────────────────────────
        private void LoadProducts()
        {
            try
            {
                var repo = new ProductRepository(_connectionString);
                _allProducts = repo.GetAllAvailableProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load products: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allProducts = new List<Product>();
            }
            ApplyFilters();
        }

        // ── FILTER + SORT ────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (_allProducts == null || _productsPanel == null) return;
            var filtered = _allProducts.AsEnumerable();
            string search = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.FarmName != null && p.FarmName.ToLower().Contains(search)));

            if (_cmbCategory?.SelectedItem?.ToString() != "All")
                filtered = filtered.Where(p =>
                    p.Category == _cmbCategory.SelectedItem.ToString());

            if (_txtMinPrice?.Text != "Min ₱" &&
                decimal.TryParse(_txtMinPrice?.Text, out decimal minP))
                filtered = filtered.Where(p => p.Price >= minP);

            if (_txtMaxPrice?.Text != "Max ₱" &&
                decimal.TryParse(_txtMaxPrice?.Text, out decimal maxP))
                filtered = filtered.Where(p => p.Price <= maxP);

            switch (_cmbSortBy?.SelectedItem?.ToString())
            {
                case "Price: Low to High": filtered = filtered.OrderBy(p => p.Price); break;
                case "Price: High to Low": filtered = filtered.OrderByDescending(p => p.Price); break;
                case "Rating: Low to High": filtered = filtered.OrderBy(p => p.Name); break;
                case "Rating: High to Low": filtered = filtered.OrderByDescending(p => p.Name); break;
                case "Name: A-Z": filtered = filtered.OrderBy(p => p.Name); break;
                case "Name: Z-A": filtered = filtered.OrderByDescending(p => p.Name); break;
                default: filtered = filtered.OrderBy(p => p.Name); break;
            }
            ShowProductCards(filtered.ToList());
        }

        // ── RENDER CARDS ─────────────────────────────────────────────────────
        private void ShowProductCards(List<Product> products)
        {
            _productsPanel.Controls.Clear();

            if (products.Count == 0)
            {
                _productsPanel.Controls.Add(new Label
                {
                    Text = "No Products Found",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.FromArgb(150, 150, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(_productsPanel.Width, 80),
                    BackColor = Color.Transparent
                });
                return;
            }

            int totalMargin = 8 * 2 * 4;
            int cardWidth = (_productsPanel.Width - totalMargin - 20) / 4;

            foreach (var product in products)
                _productsPanel.Controls.Add(CreateProductCard(product, cardWidth));
        }

        // ── CARD BUILDER ─────────────────────────────────────────────────────
        private Panel CreateProductCard(Product product, int cardWidth)
        {
            var card = new Panel
            {
                Size = new Size(cardWidth, 350),
                Margin = new Padding(8),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    ev.Graphics.FillPath(brush, path);
                    ev.Graphics.DrawPath(pen, path);
                }
            };

            Image img = null;
            if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
                try { img = Image.FromFile(product.ImagePath); } catch { }

            var capturedImg = img;
            var imageBox = new PictureBox
            {
                Size = new Size(cardWidth, 240),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Normal
            };

            imageBox.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ev.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (var clip = new GraphicsPath())
                {
                    int r = 12;
                    clip.AddArc(0, 0, r * 2, r * 2, 180, 90);
                    clip.AddArc(imageBox.Width - r * 2, 0, r * 2, r * 2, 270, 90);
                    clip.AddLine(imageBox.Width, r, imageBox.Width, imageBox.Height);
                    clip.AddLine(imageBox.Width, imageBox.Height, 0, imageBox.Height);
                    clip.AddLine(0, imageBox.Height, 0, r);
                    clip.CloseFigure();
                    ev.Graphics.SetClip(clip);
                }
                if (capturedImg != null)
                {
                    float iR = (float)capturedImg.Width / capturedImg.Height;
                    float bR = (float)imageBox.Width / imageBox.Height;
                    RectangleF src;
                    if (iR > bR) { float w = capturedImg.Height * bR; src = new RectangleF((capturedImg.Width - w) / 2f, 0, w, capturedImg.Height); }
                    else { float h = capturedImg.Width / bR; src = new RectangleF(0, (capturedImg.Height - h) / 2f, capturedImg.Width, h); }
                    ev.Graphics.DrawImage(capturedImg,
                        new Rectangle(0, 0, imageBox.Width, imageBox.Height), src, GraphicsUnit.Pixel);
                }
                else
                {
                    ev.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)),
                        0, 0, imageBox.Width, imageBox.Height);
                    using (var f = new Font("Segoe UI", 11))
                    {
                        var sz = ev.Graphics.MeasureString("No Image", f);
                        ev.Graphics.DrawString("No Image", f, new SolidBrush(Color.FromArgb(180, 180, 180)),
                            (imageBox.Width - sz.Width) / 2f, (imageBox.Height - sz.Height) / 2f);
                    }
                }
                ev.Graphics.ResetClip();
            };
            card.Controls.Add(imageBox);

            card.Controls.Add(new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(12, 252),
                Size = new Size(cardWidth - 24, 22),
                AutoEllipsis = true
            });
            card.Controls.Add(new Label
            {
                Text = "₱" + product.Price.ToString("0") + " / " + product.Unit,
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = Color.FromArgb(76, 175, 80),
                BackColor = Color.Transparent,
                Location = new Point(12, 276),
                Size = new Size(cardWidth / 2, 22)
            });

            var viewBtn = new Button
            {
                Text = "🛒  View Product",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(cardWidth / 2 - 10, 308),
                Size = new Size(cardWidth / 2 + 4, 30),
                Cursor = Cursors.Hand
            };
            viewBtn.FlatAppearance.BorderColor = Color.FromArgb(76, 175, 80);
            viewBtn.FlatAppearance.BorderSize = 1;
            viewBtn.Click += (s, ev) => OpenProductDetail(product);
            card.Controls.Add(viewBtn);

            return card;
        }

        // ── PRODUCT DETAIL MODAL ──────────────────────────────────────────────
        private void OpenProductDetail(Product product)
        {
            // Load customer's existing rating for this product
            int existingRating = 0;
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "SELECT rating FROM product_ratings WHERE customer_id=@cid AND product_id=@pid", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                        cmd.Parameters.AddWithValue("@pid", product.ProductId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            existingRating = Convert.ToInt32(result);
                    }
                }
            }
            catch { }
            _modalRating = existingRating;
            if (_contentPanel == null) return;

            _modalRating = 0;
            _modalQuantity = 1;
            _modalStars.Clear();

            for (int i = _contentPanel.Controls.Count - 1; i >= 0; i--)
                if (_contentPanel.Controls[i].Name == "dimOverlay")
                {
                    var old = _contentPanel.Controls[i];
                    _contentPanel.Controls.RemoveAt(i);
                    old.Dispose();
                }

            var overlay = new DimOverlayPanel
            {
                Name = "dimOverlay",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            _contentPanel.Controls.Add(overlay);
            overlay.BringToFront();

            // ── Modal dimensions ──────────────────────────────────────────────
            int modalW = Math.Min(1550, Math.Max(1050,
                         (int)(_contentPanel.ClientSize.Width * 0.82)));
            int modalH = 470;
            int pad = 28;
            int gap = 28;
            int radius = 18;
            int innerH = modalH - (pad * 2); // 414px

            var modal = new Panel
            {
                Name = "productModal",
                Size = new Size(modalW, modalH),
                BackColor = Color.White,
                Cursor = Cursors.Default
            };

            modal.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(
                           new Rectangle(0, 0, modal.Width - 1, modal.Height - 1), radius))
                using (var brush = new SolidBrush(Color.White))
                {
                    modal.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                }
            };

            Action centerModal = () =>
            {
                modal.Left = Math.Max(0, (overlay.ClientSize.Width - modal.Width) / 2);
                modal.Top = Math.Max(10, (overlay.ClientSize.Height - modal.Height) / 2);
            };
            centerModal();
            overlay.Resize += (s, e) => centerModal();
            overlay.Controls.Add(modal);
            modal.BringToFront();

            overlay.Click += (s, e) =>
            {
                _contentPanel.Controls.Remove(overlay);
                overlay.Dispose();
                _productsPanel?.BringToFront();
            };
            modal.Click += (s, e) => { };

            // ── LEFT: Image ───────────────────────────────────────────────────
            int imageW = (int)(modalW * 0.46);

            var imageContainer = new Panel
            {
                Location = new Point(pad, pad),
                Size = new Size(imageW, innerH),
                BackColor = Color.FromArgb(235, 235, 235)
            };
            imageContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(
                           new Rectangle(0, 0, imageContainer.Width - 1, imageContainer.Height - 1), 14))
                using (var brush = new SolidBrush(imageContainer.BackColor))
                {
                    imageContainer.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                }
            };

            var productImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 235, 235),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrWhiteSpace(product.ImagePath) && File.Exists(product.ImagePath))
                try { productImage.Image = Image.FromFile(product.ImagePath); } catch { }

            if (productImage.Image == null)
                productImage.Paint += (s, e) =>
                {
                    using (var f = new Font("Segoe UI", 18, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(150, 150, 150)))
                    {
                        string t = "No Image";
                        SizeF sz = e.Graphics.MeasureString(t, f);
                        e.Graphics.DrawString(t, f, b,
                            (productImage.Width - sz.Width) / 2, (productImage.Height - sz.Height) / 2);
                    }
                };

            imageContainer.Controls.Add(productImage);
            modal.Controls.Add(imageContainer);

            // ── RIGHT: Details ────────────────────────────────────────────────
            int detailsX = imageContainer.Right + gap;
            int detailsW = modalW - detailsX - pad;

            var detailsPanel = new Panel
            {
                Location = new Point(detailsX, pad),
                Size = new Size(detailsW, innerH),
                BackColor = Color.White
            };
            modal.Controls.Add(detailsPanel);

            int y = 4;

            // Product name
            detailsPanel.Controls.Add(new Label
            {
                Text = product.Name ?? "Unnamed Product",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 15),
                BackColor = Color.Transparent,
                Location = new Point(0, y),
                Size = new Size(detailsW - 90, 42),
                AutoEllipsis = true
            });

            // ♡ Heart button — 75 × 70 (added 20px to each dimension)
            bool isFav = false;
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkQ = "SELECT COUNT(*) FROM saved_products WHERE customer_id=@cid AND product_id=@pid";
                    using (var cmd = new MySqlCommand(checkQ, conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                        cmd.Parameters.AddWithValue("@pid", product.ProductId);
                        isFav = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { }

            var heartBtn = new Button
            {
                Text = isFav ? "♥" : "♡",
                Font = new Font("Segoe UI", 40, FontStyle.Bold),
                ForeColor = isFav ? Color.Red : Color.FromArgb(40, 40, 40),
                BackColor = Color.FromArgb(230, 230, 230),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(75, 70),
                Location = new Point(detailsW - 78, 0),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            heartBtn.FlatAppearance.BorderSize = 0;
            heartBtn.Click += (s, e) =>
            {
                isFav = !isFav;
                heartBtn.Text = isFav ? "♥" : "♡";
                heartBtn.ForeColor = isFav ? Color.Red : Color.FromArgb(40, 40, 40);

                try
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();
                        if (isFav)
                        {
                            string insertQ = "INSERT IGNORE INTO saved_products (customer_id, product_id) VALUES (@cid, @pid)";
                            using (var cmd = new MySqlCommand(insertQ, conn))
                            {
                                cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                                cmd.Parameters.AddWithValue("@pid", product.ProductId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string deleteQ = "DELETE FROM saved_products WHERE customer_id=@cid AND product_id=@pid";
                            using (var cmd = new MySqlCommand(deleteQ, conn))
                            {
                                cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                                cmd.Parameters.AddWithValue("@pid", product.ProductId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving product: " + ex.Message);
                }
            };
            detailsPanel.Controls.Add(heartBtn);

            y += 62;

            // Description header — 14pt Semibold
            detailsPanel.Controls.Add(new Label
            {
                Text = "Description",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(0, y),
                Size = new Size(detailsW, 26)
            });
            y += 32;

            // Description text — 12pt
            detailsPanel.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(product.Description)
                                   ? "No description available."
                                   : product.Description,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(65, 65, 65),
                BackColor = Color.Transparent,
                Location = new Point(0, y),
                Size = new Size(detailsW - 10, 78),
                AutoEllipsis = true
            });
            y += 88;

            // Price — 24pt Semibold
            string unit = string.IsNullOrWhiteSpace(product.Unit) ? "piece" : product.Unit;
            detailsPanel.Controls.Add(new Label
            {
                Text = "₱" + product.Price.ToString("0") + " / " + unit,
                Font = new Font("Segoe UI Semibold", 24, FontStyle.Regular),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                Location = new Point(0, y),
                Size = new Size(detailsW, 46)
            });
            y += 56;

            // ── Location + Farm Row ───────────────────────────────────────────
            int leftColW = detailsW / 2 - 10;
            int rightColX = detailsW / 2 + 10;
            int rightColW = detailsW / 2 - 18;

            // 📍 Location — left
            detailsPanel.Controls.Add(new Label
            {
                Text = "📍 Cebu City",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                Location = new Point(0, y),
                Size = new Size(leftColW, 24)
            });

            // "Farm" small header — right top
            detailsPanel.Controls.Add(new Label
            {
                Text = "Farm",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(130, 130, 130),
                BackColor = Color.Transparent,
                Location = new Point(rightColX, y),
                Size = new Size(rightColW, 18)
            });

            y += 26;

            // ★ Stars — left, below location
            for (int i = 1; i <= 5; i++)
            {
                var star = new Label
                {
                    Text = i <= _modalRating ? "★" : "☆",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = i <= _modalRating ? Color.Gold : Color.Gray,
                    BackColor = Color.Transparent,
                    Location = new Point((i - 1) * 42, y),
                    Size = new Size(40, 44),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Cursors.Hand,
                    Tag = i
                };
                star.Click += (s, e) =>
                {
                    _modalRating = (int)((Label)s).Tag;
                    foreach (var st in _modalStars)
                    {
                        int val = (int)st.Tag;
                        st.Text = val <= _modalRating ? "★" : "☆";
                        st.ForeColor = val <= _modalRating ? Color.Gold : Color.Gray;
                    }

                    // Save rating to DB
                    try
                    {
                        using (var conn = new MySqlConnection(_connectionString))
                        {
                            conn.Open();
                            string q = @"INSERT INTO product_ratings (customer_id, product_id, rating)
                             VALUES (@cid, @pid, @rating)
                             ON DUPLICATE KEY UPDATE rating = @rating";
                            using (var cmd = new MySqlCommand(q, conn))
                            {
                                cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                                cmd.Parameters.AddWithValue("@pid", product.ProductId);
                                cmd.Parameters.AddWithValue("@rating", _modalRating);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving rating: " + ex.Message);
                    }
                };
                _modalStars.Add(star);
                detailsPanel.Controls.Add(star);
            }

            // Farm avatar placeholder — right, beside farm name
            // (will be replaced with real image in next step)
            var farmAvatar = new Panel
            {
                Location = new Point(rightColX, y + 4),
                Size = new Size(36, 36),
                BackColor = Color.Transparent
            };
            string capturedPicPath = product.FarmProfilePicPath;
            farmAvatar.Paint += (s, e2) =>
            {
                e2.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var b = new Rectangle(1, 1, farmAvatar.Width - 2, farmAvatar.Height - 2);
                if (!string.IsNullOrEmpty(capturedPicPath) && File.Exists(capturedPicPath))
                {
                    try
                    {
                        using (var img = Image.FromFile(capturedPicPath))
                        using (var gp = new GraphicsPath())
                        {
                            gp.AddEllipse(b);
                            e2.Graphics.SetClip(gp);
                            e2.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            e2.Graphics.DrawImage(img, b);
                            e2.Graphics.ResetClip();
                        }
                    }
                    catch
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(190, 190, 190)))
                            e2.Graphics.FillEllipse(brush, b);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(Color.FromArgb(190, 190, 190)))
                        e2.Graphics.FillEllipse(brush, b);
                }
            };
            detailsPanel.Controls.Add(farmAvatar);

            // Farm name — right, beside avatar
            detailsPanel.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(product.FarmName)
                                   ? "Unknown Farm" : product.FarmName,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                BackColor = Color.Transparent,
                Location = new Point(rightColX + 42, y + 10),
                Size = new Size(rightColW - 44, 24),
                AutoEllipsis = true
            });

            y += 50;

            // ── Bottom: Quantity + Cart ───────────────────────────────────────
            int bottomY = innerH - 72;
            int btnH = 44;

            var minusBtn = new Button
            {
                Text = "−",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.Black,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, bottomY),
                Size = new Size(50, btnH),
                Cursor = Cursors.Hand
            };
            minusBtn.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 120);
            detailsPanel.Controls.Add(minusBtn);

            var qtyBox = new Label
            {
                Text = "1",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.Black,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(62, bottomY),
                Size = new Size(100, btnH)
            };
            detailsPanel.Controls.Add(qtyBox);

            var plusBtn = new Button
            {
                Text = "+",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.Black,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(174, bottomY),
                Size = new Size(50, btnH),
                Cursor = Cursors.Hand
            };
            plusBtn.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 120);
            detailsPanel.Controls.Add(plusBtn);

            var cartBtn = new Button
            {
                Text = "🛒   Add to Cart",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(35, 72, 13),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 60),
                Location = new Point(detailsW - 200, bottomY - 8),
                Cursor = Cursors.Hand
            };
            cartBtn.FlatAppearance.BorderSize = 0;
            detailsPanel.Controls.Add(cartBtn);

            minusBtn.Click += (s, e) =>
            {
                if (_modalQuantity > 1)
                { _modalQuantity--; qtyBox.Text = _modalQuantity.ToString(); }
            };
            plusBtn.Click += (s, e) =>
            {
                if (_modalQuantity < product.Stock)
                { _modalQuantity++; qtyBox.Text = _modalQuantity.ToString(); }
                else MessageBox.Show("Only " + product.Stock + " in stock.",
                    "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            cartBtn.Click += (s, e) =>
            {
                // Check if product from same farm already in cart
                var existing = _cart.FirstOrDefault(c => c.Product.ProductId == product.ProductId);
                if (existing != null)
                {
                    existing.Quantity += _modalQuantity;
                    _cartRepo.UpdateQuantity(_currentUser.UserId, product.ProductId, existing.Quantity);
                }
                else
                {
                    _cart.Add(new CartItem { Product = product, Quantity = _modalQuantity });
                    _cartRepo.AddToCart(_currentUser.UserId, product.ProductId, _modalQuantity);
                }

                // Close modal
                _contentPanel.Controls.Remove(overlay);
                overlay.Dispose();
                _productsPanel?.BringToFront();

                MessageBox.Show(
                    "✅ " + product.Name + " added to cart!\nCart items: " + _cart.Count,
                    "Added to Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        // ── DIM OVERLAY ───────────────────────────────────────────────────────
        private class DimOverlayPanel : Panel
        {
            private readonly int _alpha = 130;
            public DimOverlayPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var dark = new SolidBrush(Color.FromArgb(_alpha, 0, 0, 0)))
                    e.Graphics.FillRectangle(dark, ClientRectangle);
                using (var glass = new SolidBrush(Color.FromArgb(18, 255, 255, 100)))
                    e.Graphics.FillRectangle(glass, ClientRectangle);
            }
        }

        // ── ROUNDED RECT HELPER ───────────────────────────────────────────────
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

        // ── KEEP THESE ────────────────────────────────────────────────────────
        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#4CAF50");
            guna2GradientPanel1.FillColor = ColorTranslator.FromHtml("#4CAF50");
            guna2GradientPanel1.FillColor2 = ColorTranslator.FromHtml("#F44336");
        }
        private void guna2Panel2_Paint(object sender, PaintEventArgs e) { }
        private void guna2Button2_Click(object sender, EventArgs e) { }
        private void guna2Button1_Click(object sender, EventArgs e) { }
        private void guna2Button5_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel1_Click(object sender, EventArgs e) { }
        private void guna2Button8_Click(object sender, EventArgs e) { }
        private void guna2Button4_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel8_Click(object sender, EventArgs e) { }
        private void guna2Button11_Click(object sender, EventArgs e) { }
        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e) { }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void OpenDropdown()
        {
            _dropdownOpen = true;

            _dropdownMenu = new Panel
            {
                Size = new Size(220, 320),
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

            // Position it below the MenuButton
            Point pos = this.PointToClient(
                MenuButton.Parent.PointToScreen(
                    new Point(MenuButton.Right - 220, MenuButton.Bottom + 5)));
            _dropdownMenu.Location = pos;

            // ── Avatar circle ─────────────────────────────────────────────────
            // ── Avatar circle ─────────────────────────────────────────────────
            var avatar = new Panel
            {
                Location = new Point(85, 16),
                Size = new Size(50, 50),
                BackColor = Color.Transparent
            };

            // Load profile pic from DB
            string avatarPicPath = "";
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT profile_pic_path FROM users WHERE user_id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                        avatarPicPath = cmd.ExecuteScalar()?.ToString() ?? "";
                    }
                }
            }
            catch { }

            string capturedAvatarPath = avatarPicPath;
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var b = new Rectangle(0, 0, 49, 49);

                if (!string.IsNullOrEmpty(capturedAvatarPath) && File.Exists(capturedAvatarPath))
                {
                    try
                    {
                        using (var img = Image.FromFile(capturedAvatarPath))
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
                        // fallback to initials
                        using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                            e.Graphics.FillEllipse(brush, b);
                    }
                }
                else
                {
                    // No pic — show initials
                    using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                        e.Graphics.FillEllipse(brush, b);
                    string initials = _currentUser?.FullName?.Length > 0
                        ? _currentUser.FullName[0].ToString().ToUpper() : "?";
                    using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        SizeF sz = e.Graphics.MeasureString(initials, font);
                        e.Graphics.DrawString(initials, font, brush,
                            (49 - sz.Width) / 2, (49 - sz.Height) / 2);
                    }
                }
            };
            _dropdownMenu.Controls.Add(avatar);
            // ── User name ─────────────────────────────────────────────────────
            _dropdownMenu.Controls.Add(new Label
            {
                Text = _currentUser?.FullName ?? "Customer",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 72),
                Size = new Size(220, 22)
            });

            // ── Role label ────────────────────────────────────────────────────
            _dropdownMenu.Controls.Add(new Label
            {
                Text = "Customer",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 220, 150),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 94),
                Size = new Size(220, 18)
            });

            // ── Divider ───────────────────────────────────────────────────────
            _dropdownMenu.Controls.Add(new Panel
            {
                Location = new Point(15, 118),
                Size = new Size(190, 1),
                BackColor = Color.FromArgb(80, 100, 60)
            });

            // ── Menu items ────────────────────────────────────────────────────
            string[] menuItems = { "👤  View Profile", "📦  My Orders", "🛒  Cart", "🌿  Saved Products" };
            int itemStartY = 128;

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
                    Location = new Point(10, itemStartY),
                    Size = new Size(200, 36),
                    Cursor = Cursors.Hand,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 90, 30);
                btn.Tag = item;
                btn.Click += DropdownItem_Click;
                _dropdownMenu.Controls.Add(btn);
                itemStartY += 38;
            }

            // ── Logout button ─────────────────────────────────────────────────
            var logoutBtn = new Button
            {
                Text = "Logout",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 36),
                Location = new Point(20, 282),
                Cursor = Cursors.Hand
            };
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.Click += (s, e) =>
            {
                CloseDropdown();
                this.Hide();
                var login = new LoginForm();
                login.Show();
            };
            _dropdownMenu.Controls.Add(logoutBtn);

            this.Controls.Add(_dropdownMenu);
            _dropdownMenu.BringToFront();
        }
        private void DropdownItem_Click(object sender, EventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            CloseDropdown();

            if (tag.Contains("View Profile"))
                ShowProfilePanel();
            else if (tag.Contains("My Orders"))
                ShowMyOrdersPanel();
            else if (tag.Contains("Cart"))
                ShowCartPanel();
            else if (tag.Contains("Saved Products"))
                ShowSavedProductsPanel();
        }
        private void ShowSavedProductsPanel()
        {
            foreach (Control c in this.Controls)
                if (c.Name == "savedProductsPanel") { this.Controls.Remove(c); c.Dispose(); break; }

            var panel = new Panel
            {
                Name = "savedProductsPanel",
                Size = new Size(420, this.ClientSize.Height - 120),
                Location = new Point(this.ClientSize.Width - 430, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(panel.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    panel.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Header ────────────────────────────────────────────────────────
            panel.Controls.Add(new Label
            {
                Text = "❤️  Saved Products",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 15),
                BackColor = Color.Transparent,
                Location = new Point(16, 16),
                Size = new Size(280, 30)
            });

            // ── Close button ──────────────────────────────────────────────────
            var closeBtn = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(378, 14),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => { this.Controls.Remove(panel); panel.Dispose(); };
            panel.Controls.Add(closeBtn);

            // ── Divider ───────────────────────────────────────────────────────
            panel.Controls.Add(new Panel
            {
                Location = new Point(0, 52),
                Size = new Size(420, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Load saved products from DB ───────────────────────────────────
            var savedProducts = new List<(int ProductId, string ProductName, decimal Price, string Unit, string ImagePath, string FarmName)>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string q = @"SELECT p.product_id, p.product_name, p.price, p.unit, 
                                p.image_path, f.farm_name
                         FROM saved_products sp
                         JOIN products p ON sp.product_id = p.product_id
                         JOIN farms f ON p.farm_id = f.farm_id
                         WHERE sp.customer_id = @id
                         ORDER BY sp.created_at DESC";
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                savedProducts.Add((
                                    Convert.ToInt32(r["product_id"]),
                                    r["product_name"].ToString(),
                                    Convert.ToDecimal(r["price"]),
                                    r["unit"].ToString(),
                                    r["image_path"].ToString(),
                                    r["farm_name"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading saved products: " + ex.Message);
            }

            // ── Empty state ───────────────────────────────────────────────────
            if (savedProducts.Count == 0)
            {
                panel.Controls.Add(new Label
                {
                    Text = "You haven't saved any products yet.\nHeart a product to save it!",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 160),
                    Size = new Size(420, 60)
                });
            }
            else
            {
                var scroll = new Panel
                {
                    Location = new Point(0, 58),
                    Size = new Size(420, panel.Height - 58),
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };
                panel.Controls.Add(scroll);

                int itemY = 10;

                foreach (var prod in savedProducts)
                {
                    var captured = prod;

                    var card = new Panel
                    {
                        Location = new Point(10, itemY),
                        Size = new Size(390, 100),
                        BackColor = Color.FromArgb(248, 248, 248)
                    };
                    card.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var path = MakeRoundedRect(card.ClientRectangle, 10))
                        using (var brush = new SolidBrush(Color.FromArgb(248, 248, 248)))
                        using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                        {
                            card.Region = new Region(path);
                            e.Graphics.FillPath(brush, path);
                            e.Graphics.DrawPath(pen, path);
                        }
                    };

                    // ── Product image ─────────────────────────────────────────
                    var picBox = new PictureBox
                    {
                        Location = new Point(12, 12),
                        Size = new Size(76, 76),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(220, 220, 220)
                    };
                    if (!string.IsNullOrEmpty(captured.ImagePath) && File.Exists(captured.ImagePath))
                        try { picBox.Image = Image.FromFile(captured.ImagePath); } catch { }
                    card.Controls.Add(picBox);

                    // ── Product name ──────────────────────────────────────────
                    card.Controls.Add(new Label
                    {
                        Text = captured.ProductName,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(20, 20, 20),
                        BackColor = Color.Transparent,
                        Location = new Point(100, 12),
                        Size = new Size(240, 20),
                        AutoEllipsis = true
                    });

                    // ── Farm name ─────────────────────────────────────────────
                    card.Controls.Add(new Label
                    {
                        Text = "🌿 " + captured.FarmName,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.Gray,
                        BackColor = Color.Transparent,
                        Location = new Point(100, 34),
                        Size = new Size(240, 18)
                    });

                    // ── Price ─────────────────────────────────────────────────
                    card.Controls.Add(new Label
                    {
                        Text = "₱" + captured.Price.ToString("0") + " / " + captured.Unit,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(49, 91, 23),
                        BackColor = Color.Transparent,
                        Location = new Point(100, 54),
                        Size = new Size(180, 20)
                    });

                    // ── Unsave button ─────────────────────────────────────────
                    var unsaveBtn = new Button
                    {
                        Text = "♥",
                        Font = new Font("Segoe UI", 14),
                        ForeColor = Color.Red,
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        Size = new Size(36, 36),
                        Location = new Point(342, 10),
                        Cursor = Cursors.Hand
                    };
                    unsaveBtn.FlatAppearance.BorderSize = 0;
                    unsaveBtn.Click += (s, e) =>
                    {
                        try
                        {
                            using (var conn = new MySqlConnection(_connectionString))
                            {
                                conn.Open();
                                string del = "DELETE FROM saved_products WHERE customer_id=@cid AND product_id=@pid";
                                using (var cmd = new MySqlCommand(del, conn))
                                {
                                    cmd.Parameters.AddWithValue("@cid", _currentUser.UserId);
                                    cmd.Parameters.AddWithValue("@pid", captured.ProductId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            this.Controls.Remove(panel);
                            panel.Dispose();
                            ShowSavedProductsPanel();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    };
                    card.Controls.Add(unsaveBtn);

                    scroll.Controls.Add(card);
                    itemY += 110;
                }
            }

            this.Controls.Add(panel);
            panel.BringToFront();
        }
        private void ShowProfilePanel()
        {
            // Remove existing if open
            foreach (Control c in this.Controls)
                if (c.Name == "profilePanel") { this.Controls.Remove(c); c.Dispose(); break; }

            var profilePanel = new Panel
            {
                Name = "profilePanel",
                Size = new Size(480, this.ClientSize.Height - 120),
                Location = new Point(this.ClientSize.Width - 490, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            typeof(Panel).GetProperty("DoubleBuffered",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
    ?.SetValue(profilePanel, true);

            profilePanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(profilePanel.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    profilePanel.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Header ────────────────────────────────────────────────────────
            profilePanel.Controls.Add(new Label
            {
                Text = "👤  My Profile",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 15),
                BackColor = Color.Transparent,
                Location = new Point(16, 16),
                Size = new Size(280, 30)
            });

            // ── Close button ──────────────────────────────────────────────────
            var closeBtn = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(438, 14),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => { this.Controls.Remove(profilePanel); profilePanel.Dispose(); };
            profilePanel.Controls.Add(closeBtn);

            // ── Divider ───────────────────────────────────────────────────────
            profilePanel.Controls.Add(new Panel
            {
                Location = new Point(0, 52),
                Size = new Size(480, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Scrollable content ────────────────────────────────────────────
            var scroll = new Panel
            {
                Location = new Point(0, 58),
                Size = new Size(480, profilePanel.Height - 58),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            profilePanel.Controls.Add(scroll);

            int y = 16;
            bool isEditing = false;
            string newPicPath = null;

            // ── Profile pic ───────────────────────────────────────────────────
            var picBox = new System.Windows.Forms.PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(190, 16),
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
            scroll.Controls.Add(picBox);
            y += 116;

            // ── Upload pic button ─────────────────────────────────────────────
            var uploadBtn = new Button
            {
                Text = "📷 Change Photo",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 28),
                Location = new Point(300, 50),
                Cursor = Cursors.Hand,
                Visible = false
            };
            uploadBtn.FlatAppearance.BorderSize = 0;
            uploadBtn.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string folder = @"C:\Programming\C#\Grow2Go1\ProductImages";
                        Directory.CreateDirectory(folder);
                        string ext = Path.GetExtension(ofd.FileName);
                        string fileName = "user_" + _currentUser.UserId + "_profile" + ext;
                        string destPath = Path.Combine(folder, fileName);
                        File.Copy(ofd.FileName, destPath, true);

                        newPicPath = destPath;
                        picBox.Image = Image.FromFile(newPicPath);
                    }
                }
            };
            scroll.Controls.Add(uploadBtn);
           

            // ── Stats row ─────────────────────────────────────────────────────
            int totalOrders = 0;
            decimal totalSpent = 0;

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE customer_id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                        totalOrders = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    using (var cmd = new MySqlCommand("SELECT IFNULL(SUM(total_amount),0) FROM orders WHERE customer_id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                        totalSpent = Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
            }
            catch { }

            // Stats boxes
            var statsPanel = new Panel
            {
                Location = new Point(16, y),
                Size = new Size(444, 70),
                BackColor = Color.Transparent
            };

            var ordersBox = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(210, 70),
                BackColor = Color.FromArgb(245, 250, 240)
            };
            ordersBox.Controls.Add(new Label
            {
                Text = totalOrders.ToString(),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                Location = new Point(0, 8),
                Size = new Size(210, 32),
                TextAlign = ContentAlignment.MiddleCenter
            });
            ordersBox.Controls.Add(new Label
            {
                Text = "Total Orders",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(0, 42),
                Size = new Size(210, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });
            statsPanel.Controls.Add(ordersBox);

            var spentBox = new Panel
            {
                Location = new Point(224, 0),
                Size = new Size(220, 70),
                BackColor = Color.FromArgb(245, 250, 240)
            };
            spentBox.Controls.Add(new Label
            {
                Text = "₱" + totalSpent.ToString("N2"),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(49, 91, 23),
                BackColor = Color.Transparent,
                Location = new Point(0, 8),
                Size = new Size(220, 32),
                TextAlign = ContentAlignment.MiddleCenter
            });
            spentBox.Controls.Add(new Label
            {
                Text = "Total Spent",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(0, 42),
                Size = new Size(220, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });
            statsPanel.Controls.Add(spentBox);
            scroll.Controls.Add(statsPanel);
            y += 86;

            // ── Helper to make a field ────────────────────────────────────────
            TextBox MakeField(string label, string value, int fieldY)
            {
                scroll.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    BackColor = Color.Transparent,
                    Location = new Point(16, fieldY),
                    Size = new Size(444, 18)
                });
                var txt = new TextBox
                {
                    Text = value,
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(16, fieldY + 20),
                    Size = new Size(444, 32),
                    BorderStyle = BorderStyle.FixedSingle,
                    ReadOnly = true,
                    BackColor = Color.FromArgb(248, 248, 248)
                };
                scroll.Controls.Add(txt);
                return txt;
            }

            // ── Load user data from DB ────────────────────────────────────────
            string dbName = "", dbEmail = "", dbPhone = "", dbBio = "",
                   dbDob = "", dbAddress = "", dbCity = "", dbState = "",
                   dbZip = "", dbPicPath = "", dbCreatedAt = "";
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string q = @"SELECT full_name, email, phone, bio, date_of_birth,
                                street_address, city, state, zip_code,
                                profile_pic_path, created_at
                         FROM users WHERE user_id = @id";
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                dbName = r["full_name"].ToString();
                                dbEmail = r["email"].ToString();
                                dbPhone = r["phone"].ToString();
                                dbBio = r["bio"].ToString();
                                dbAddress = r["street_address"].ToString();
                                dbCity = r["city"].ToString();
                                dbState = r["state"].ToString();
                                dbZip = r["zip_code"].ToString();
                                dbPicPath = r["profile_pic_path"].ToString();
                                dbCreatedAt = r["created_at"] != DBNull.Value
                                    ? Convert.ToDateTime(r["created_at"]).ToString("MMM dd, yyyy") : "";
                                dbDob = r["date_of_birth"] != DBNull.Value
                                    ? Convert.ToDateTime(r["date_of_birth"]).ToString("yyyy-MM-dd") : "";
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(dbPicPath) && File.Exists(dbPicPath))
                    picBox.Image = Image.FromFile(dbPicPath);
            }
            catch { }

            // ── Fields ────────────────────────────────────────────────────────
            var txtName = MakeField("Full Name", dbName, y); y += 66;
            var txtEmail = MakeField("Email", dbEmail, y); y += 66;
            var txtPhone = MakeField("Phone", dbPhone, y); y += 66;
            var txtDob = MakeField("Date of Birth", dbDob, y); y += 66;
            var txtBio = MakeField("Bio", dbBio, y); y += 66;
            var txtAddress = MakeField("Street Address", dbAddress, y); y += 66;
            var txtCity = MakeField("City", dbCity, y); y += 66;
            var txtState = MakeField("State", dbState, y); y += 66;
            var txtZip = MakeField("ZIP Code", dbZip, y); y += 66;

            // Member since
            scroll.Controls.Add(new Label
            {
                Text = "Member since: " + dbCreatedAt,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(16, y),
                Size = new Size(444, 20)
            });
            y += 36;

            // ── Edit / Save button ────────────────────────────────────────────
            var editBtn = new Button
            {
                Text = "✏️  Edit Profile",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(49, 91, 23),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(444, 44),
                Location = new Point(16, y),
                Cursor = Cursors.Hand
            };
            editBtn.FlatAppearance.BorderSize = 0;

            editBtn.Click += (s, e) =>
            {
                isEditing = !isEditing;

                TextBox[] fields = { txtName, txtEmail, txtPhone, txtDob, txtBio, txtAddress, txtCity, txtState, txtZip };
                foreach (var f in fields)
                {
                    f.ReadOnly = !isEditing;
                    f.BackColor = isEditing ? Color.White : Color.FromArgb(248, 248, 248);
                }

                uploadBtn.Visible = isEditing;
                editBtn.Text = isEditing ? "💾  Save Changes" : "✏️  Edit Profile";

                if (!isEditing)
                {
                    // Save to DB
                    try
                    {
                        using (var conn = new MySqlConnection(_connectionString))
                        {
                            conn.Open();
                            string q = @"UPDATE users SET
                                    full_name=@name, email=@email, phone=@phone,
                                    bio=@bio, date_of_birth=@dob, street_address=@addr,
                                    city=@city, state=@state, zip_code=@zip,
                                    profile_pic_path=@pic
                                 WHERE user_id=@id";
                            using (var cmd = new MySqlCommand(q, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                                cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                                cmd.Parameters.AddWithValue("@bio", txtBio.Text.Trim());
                                cmd.Parameters.AddWithValue("@addr", txtAddress.Text.Trim());
                                cmd.Parameters.AddWithValue("@city", txtCity.Text.Trim());
                                cmd.Parameters.AddWithValue("@state", txtState.Text.Trim());
                                cmd.Parameters.AddWithValue("@zip", txtZip.Text.Trim());
                                cmd.Parameters.AddWithValue("@pic", newPicPath ?? dbPicPath);

                                if (DateTime.TryParse(txtDob.Text, out DateTime dob))
                                    cmd.Parameters.AddWithValue("@dob", dob);
                                else
                                    cmd.Parameters.AddWithValue("@dob", DBNull.Value);

                                cmd.Parameters.AddWithValue("@id", _currentUser.UserId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        _currentUser.FullName = txtName.Text.Trim();
                        _currentUser.Email = txtEmail.Text.Trim();
                        _currentUser.Phone = txtPhone.Text.Trim();

                        MessageBox.Show("Profile saved!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving: " + ex.Message);
                    }
                }
            };
            scroll.Controls.Add(editBtn);

            this.Controls.Add(profilePanel);
            profilePanel.BringToFront();
        }
        private DateTime _lastMenuClick = DateTime.MinValue;

        private void MenuButton_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastMenuClick).TotalMilliseconds < 400)
                return;

            _lastMenuClick = DateTime.Now;

            if (_dropdownOpen)
                CloseDropdown();
            else
                OpenDropdown();
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
        private void RefreshCartPanel()
        {
            foreach (Control c in this.Controls)
                if (c.Name == "cartPanel")
                {
                    this.Controls.Remove(c);
                    c.Dispose();
                    break;
                }
            ShowCartPanel();
        }
        private void ShowCartPanel()
        {
            // Remove existing cart panel if open
            foreach (Control c in this.Controls)
                if (c.Name == "cartPanel") { this.Controls.Remove(c); c.Dispose(); break; }

            var cartPanel = new Panel
            {
                Name = "cartPanel",
                Size = new Size(380, this.ClientSize.Height - 120),
                Location = new Point(this.ClientSize.Width - 390, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            cartPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(cartPanel.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    cartPanel.Region = new Region(path);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Header ────────────────────────────────────────────────────────
            cartPanel.Controls.Add(new Label
            {
                Text = "🛒  My Cart",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 15),
                BackColor = Color.Transparent,
                Location = new Point(16, 16),
                Size = new Size(240, 30)
            });

            // Close cart button
            var closeCart = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(338, 14),
                Cursor = Cursors.Hand
            };
            closeCart.FlatAppearance.BorderSize = 0;
            closeCart.Click += (s, e) =>
            {
                this.Controls.Remove(cartPanel);
                cartPanel.Dispose();
            };
            cartPanel.Controls.Add(closeCart);

            // ── Divider ───────────────────────────────────────────────────────
            cartPanel.Controls.Add(new Panel
            {
                Location = new Point(0, 52),
                Size = new Size(380, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            });

            // ── Cart items ────────────────────────────────────────────────────
            int itemY = 62;

            if (_cart.Count == 0)
            {
                cartPanel.Controls.Add(new Label
                {
                    Text = "Your cart is empty.",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 160),
                    Size = new Size(380, 40)
                });
            }
            else
            {
                foreach (var item in _cart.ToList())
                {
                    var capturedItem = item;

                    // Product name
                    cartPanel.Controls.Add(new Label
                    {
                        Text = item.Product.Name,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(20, 20, 20),
                        BackColor = Color.Transparent,
                        Location = new Point(16, itemY),
                        Size = new Size(200, 22)
                    });

                    // Subtotal
                    cartPanel.Controls.Add(new Label
                    {
                        Text = "₱" + item.Subtotal.ToString("0.00"),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(49, 91, 23),
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleRight,
                        Location = new Point(240, itemY),
                        Size = new Size(120, 22)
                    });

                    // ── Minus button ──────────────────────────────────────────────
                    var minusBtn = new Button
                    {
                        Text = "−",
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.Black,
                        BackColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Location = new Point(16, itemY + 26),
                        Size = new Size(30, 28),
                        Cursor = Cursors.Hand
                    };
                    minusBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                    minusBtn.Click += (s, e) =>
                    {
                        if (capturedItem.Quantity > 1)
                        {
                            capturedItem.Quantity--;
                            _cartRepo.UpdateQuantity(_currentUser.UserId,
                                capturedItem.Product.ProductId, capturedItem.Quantity);
                        }
                        else
                        {
                            _cart.Remove(capturedItem);
                            _cartRepo.RemoveFromCart(_currentUser.UserId, capturedItem.Product.ProductId);
                        }
                        RefreshCartPanel();
                    };
                    cartPanel.Controls.Add(minusBtn);

                    // ── Qty label ─────────────────────────────────────────────────
                    cartPanel.Controls.Add(new Label
                    {
                        Text = item.Quantity.ToString(),
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.Black,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(50, itemY + 26),
                        Size = new Size(40, 28)
                    });

                    // ── Plus button ───────────────────────────────────────────────
                    var plusBtn = new Button
                    {
                        Text = "+",
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.Black,
                        BackColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Location = new Point(94, itemY + 26),
                        Size = new Size(30, 28),
                        Cursor = Cursors.Hand
                    };
                    plusBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                    plusBtn.Click += (s, e) =>
                    {
                        if (capturedItem.Quantity < capturedItem.Product.Stock)
                        {
                            capturedItem.Quantity++;
                            _cartRepo.UpdateQuantity(_currentUser.UserId,
                                capturedItem.Product.ProductId, capturedItem.Quantity);
                        }
                        else
                            MessageBox.Show("Only " + capturedItem.Product.Stock + " in stock.",
                                "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        RefreshCartPanel();
                    };
                    cartPanel.Controls.Add(plusBtn);

                    // ── Remove button ─────────────────────────────────────────────
                    var removeBtn = new Button
                    {
                        Text = "✕ Remove",
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.Red,
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        Location = new Point(200, itemY + 26),
                        Size = new Size(90, 28),
                        Cursor = Cursors.Hand
                    };
                    removeBtn.FlatAppearance.BorderSize = 0;
                    removeBtn.Click += (s, e) =>
                    {
                        _cart.Remove(capturedItem);
                        _cartRepo.RemoveFromCart(_currentUser.UserId, capturedItem.Product.ProductId);
                        RefreshCartPanel();
                    };
                    cartPanel.Controls.Add(removeBtn);

                    itemY += 70;

                    // Item divider
                    cartPanel.Controls.Add(new Panel
                    {
                        Location = new Point(16, itemY - 6),
                        Size = new Size(348, 1),
                        BackColor = Color.FromArgb(235, 235, 235)
                    });
                }

                // ── Total ─────────────────────────────────────────────────────
                decimal grandTotal = _cart.Sum(c => c.Subtotal);

                cartPanel.Controls.Add(new Panel
                {
                    Location = new Point(0, cartPanel.Height - 130),
                    Size = new Size(380, 1),
                    BackColor = Color.FromArgb(200, 200, 200)
                });

                cartPanel.Controls.Add(new Label
                {
                    Text = "Total:",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 20, 20),
                    BackColor = Color.Transparent,
                    Location = new Point(16, cartPanel.Height - 120),
                    Size = new Size(100, 28)
                });

                cartPanel.Controls.Add(new Label
                {
                    Text = "₱" + grandTotal.ToString("0.00"),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(49, 91, 23),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(200, cartPanel.Height - 120),
                    Size = new Size(160, 28)
                });
                // ── Delivery Mode ─────────────────────────────────────────────
                cartPanel.Controls.Add(new Label
                {
                    Text = "Delivery Mode:",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 20, 20),
                    BackColor = Color.Transparent,
                    Location = new Point(16, cartPanel.Height - 175),
                    Size = new Size(130, 24)
                });

                var cmbDelivery = new ComboBox
                {
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(150, cartPanel.Height - 177),
                    Size = new Size(210, 28)
                };
                cmbDelivery.Items.AddRange(new object[] { "Delivery", "Pickup" });
                cmbDelivery.SelectedIndex = 0;
                cartPanel.Controls.Add(cmbDelivery);

                // ── Payment Method ────────────────────────────────────────────
                cartPanel.Controls.Add(new Label
                {
                    Text = "Payment:",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 20, 20),
                    BackColor = Color.Transparent,
                    Location = new Point(16, cartPanel.Height - 145),
                    Size = new Size(130, 24)
                });

                cartPanel.Controls.Add(new Label
                {
                    Text = "💵 Cash on Delivery",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    BackColor = Color.Transparent,
                    Location = new Point(150, cartPanel.Height - 145),
                    Size = new Size(210, 24)
                });

                // ── Place Order button ─────────────────────────────────────────
                var placeOrderBtn = new Button
                {
                    Text = "Place Order",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(35, 72, 13),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(348, 50),
                    Location = new Point(16, cartPanel.Height - 80),
                    Cursor = Cursors.Hand
                };
                placeOrderBtn.FlatAppearance.BorderSize = 0;
                placeOrderBtn.Click += (s, e) =>
                {
                    string selectedDelivery = cmbDelivery.SelectedItem.ToString();

                    var repo = new OrderRepository();
                    bool success = repo.PlaceOrder(
                        _currentUser.UserId, _cart, selectedDelivery, "Cash");

                    if (success)
                    {
                        // Clear cart from DB and memory
                        _cartRepo.ClearCart(_currentUser.UserId);
                        _cart.Clear();

                        // Close cart panel
                        foreach (Control c in this.Controls)
                            if (c.Name == "cartPanel") { this.Controls.Remove(c); c.Dispose(); break; }

                        // Reload products (stock updated)
                        LoadProducts();

                        MessageBox.Show("✅ Order placed successfully!\nStatus: Pending",
                            "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to place order. Please try again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                cartPanel.Controls.Add(placeOrderBtn);
            }

            this.Controls.Add(cartPanel);
            cartPanel.BringToFront();
        }
        private void ShowFarmsPage()
        {
            // Remove existing overlay if open
            foreach (Control c in this.Controls)
                if (c.Name == "farmsOverlay") { this.Controls.Remove(c); c.Dispose(); break; }

            var farmsUC = new Farms(_currentUser);
            farmsUC.Name = "farmsOverlay";
            farmsUC.Dock = DockStyle.Fill;
            farmsUC.BackColor = Color.FromArgb(23, 34, 17);

            farmsUC.BackToMarketplace += (s, ev) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.Controls.Remove(farmsUC);
                    farmsUC.Dispose();
                }));
            };

            farmsUC.ViewFarmProducts += (s, ev) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.Controls.Remove(farmsUC);
                    farmsUC.Dispose();
                    txtSearch.Text = ev.FarmName;
                    ApplyFilters();
                }));
            };

            this.Controls.Add(farmsUC);
            farmsUC.BringToFront();
        }
        private void FarmMapButton_Click(object sender, EventArgs e)
        {
            ShowFarmsPage();
        }
    }
}