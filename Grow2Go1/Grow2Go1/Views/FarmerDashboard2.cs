using Grow2Go1.Models;
using Grow2Go1.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard2 : UserControl
    {
        private int _farmId;
        private bool _ordersLoaded = false;
        private Panel _ordersPanel;

        public FarmerDashboard2()
        {
            InitializeComponent();
        }

        public FarmerDashboard2(int farmId) : this()
        {
            _farmId = farmId;
        }

        private void FarmerDashboard2_Load(object sender, EventArgs e) { }

        // ── Wait for proper UC size before loading ───────────────────────────
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!_ordersLoaded && this.Width > 300 && _farmId > 0)
            {
                _ordersLoaded = true;

                // Hide all designer placeholder controls first
                foreach (Control c in this.Controls)
                    c.Visible = false;

                LoadOrders();
            }
        }

        // ── Load orders from DB ──────────────────────────────────────────────
        public void LoadOrders()
        {
            if (_ordersPanel != null)
            {
                this.Controls.Remove(_ordersPanel);
                _ordersPanel.Dispose();
                _ordersPanel = null;
            }

            var repo = new OrderRepository();
            var orders = repo.GetOrdersByFarm(_farmId);

            // Show/hide empty state label
            if (this.Controls.ContainsKey("lblNoOrders"))
                this.Controls["lblNoOrders"].Visible = orders.Count == 0;
            else if (orders.Count == 0)
            {
                var lbl = new Label
                {
                    Name = "lblNoOrders",
                    Text = "No Orders Yet",
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    ForeColor = Color.FromArgb(150, 150, 150),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(this.Width, 60),
                    Location = new Point(0, this.Height / 2 - 30)
                };
                this.Controls.Add(lbl);
                return;
            }

            if (orders.Count == 0) return;

            // Build scrollable orders panel
            _ordersPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(this.Width - 40, this.Height - 80),
                AutoScroll = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                             AnchorStyles.Right | AnchorStyles.Bottom
            };

            int yOffset = 0;
            var itemRepo = new OrderRepository();

            foreach (var order in orders)
            {
                var items = itemRepo.GetOrderItems(order.OrderId);
                var card = CreateOrderCard(order, items, _ordersPanel.Width - 20);
                card.Location = new Point(0, yOffset);
                _ordersPanel.Controls.Add(card);
                yOffset += card.Height + 15; // 15px gap between cards
            }

            this.Controls.Add(_ordersPanel);
            _ordersPanel.BringToFront();
        }

        // ── Build one order card ─────────────────────────────────────────────
        private Panel CreateOrderCard(Order order, List<OrderItem> items, int cardWidth)
        {
            // Dynamic height: base + items
            int baseHeight = 140;
            int itemHeight = 25;
            int cardHeight = baseHeight + (items.Count * itemHeight) + 50;

            var card = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.White
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Status badge (top right) ──
            Color badgeColor = GetStatusColor(order.Status);
            var statusBadge = new Label
            {
                Text = char.ToUpper(order.Status[0]) + order.Status.Substring(1),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = badgeColor,
                Size = new Size(95, 26),
                Location = new Point(cardWidth - 110, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(statusBadge);

            // ── Customer initials avatar ──
            string initials = GetInitials(order.CustomerName);
            var avatar = new Panel
            {
                Size = new Size(48, 48),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                    e.Graphics.FillEllipse(brush, 0, 0, 47, 47);
                using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sz = e.Graphics.MeasureString(initials, font);
                    e.Graphics.DrawString(initials, font, brush,
                        (48 - sz.Width) / 2f, (48 - sz.Height) / 2f);
                }
            };
            card.Controls.Add(avatar);

            // ── Customer name ──
            var nameLabel = new Label
            {
                Text = order.CustomerName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Location = new Point(78, 18),
                Size = new Size(300, 24),
                BackColor = Color.Transparent
            };
            card.Controls.Add(nameLabel);

            // ── Order ID ──
            var orderIdLabel = new Label
            {
                Text = "ORD-" + order.OrderId.ToString("D3"),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(78, 42),
                Size = new Size(200, 20),
                BackColor = Color.Transparent
            };
            card.Controls.Add(orderIdLabel);

            // ── Order date ──
            var dateLabel = new Label
            {
                Text = "📅 Ordered: " + order.CreatedAt.ToString("yyyy-MM-dd"),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(20, 78),
                Size = new Size(300, 20),
                BackColor = Color.Transparent
            };
            card.Controls.Add(dateLabel);

            // ── Confirm Order button (only shown for pending orders) ──
            if (order.Status == "pending")
            {
                var confirmBtn = new Button
                {
                    Text = "✔ Confirm Order",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(30, 30, 30),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(155, 36),
                    Location = new Point(20, cardHeight - 52),
                    Cursor = Cursors.Hand
                };
                confirmBtn.FlatAppearance.BorderSize = 0;
                confirmBtn.Click += (s, e) =>
                {
                    var result = MessageBox.Show(
                        "Confirm this order from " + order.CustomerName + "?",
                        "Confirm Order",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        var repo = new OrderRepository();
                        if (repo.UpdateOrderStatus(order.OrderId, "confirmed"))
                        {
                            MessageBox.Show("Order confirmed!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOrders(); // refresh
                        }
                    }
                };
                card.Controls.Add(confirmBtn);
            }

            // ── RIGHT SIDE: Order items ──
            int rightX = cardWidth / 2 + 20;
            int rightWidth = cardWidth / 2 - 30;

            var itemsHeader = new Label
            {
                Text = "Order Items:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Location = new Point(rightX, 20),
                Size = new Size(rightWidth, 22),
                BackColor = Color.Transparent
            };
            card.Controls.Add(itemsHeader);

            int itemY = 48;
            foreach (var item in items)
            {
                // Item name × qty
                var itemNameLabel = new Label
                {
                    Text = item.ProductName + " × " + item.Quantity,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(rightX, itemY),
                    Size = new Size(rightWidth - 80, 22),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(itemNameLabel);

                // Item price
                var itemPriceLabel = new Label
                {
                    Text = "₱" + (item.UnitPrice * item.Quantity).ToString("0.00"),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(rightX + rightWidth - 75, itemY),
                    Size = new Size(75, 22),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(itemPriceLabel);

                itemY += itemHeight;
            }

            // Separator line
            var separator = new Panel
            {
                Size = new Size(rightWidth, 1),
                Location = new Point(rightX, itemY + 4),
                BackColor = Color.FromArgb(220, 220, 220)
            };
            card.Controls.Add(separator);

            // Total label
            var totalLabel = new Label
            {
                Text = "Total:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Location = new Point(rightX, itemY + 12),
                Size = new Size(rightWidth - 80, 24),
                BackColor = Color.Transparent
            };
            card.Controls.Add(totalLabel);

            // Total amount
            var totalAmount = new Label
            {
                Text = "₱" + order.TotalAmount.ToString("0.00"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Location = new Point(rightX + rightWidth - 75, itemY + 12),
                Size = new Size(75, 24),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            card.Controls.Add(totalAmount);

            return card;
        }

        // ── Helper: get initials from full name ──────────────────────────────
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "?";
            var parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return ("" + parts[0][0] + parts[parts.Length - 1][0]).ToUpper();
            return ("" + fullName[0]).ToUpper();
        }

        // ── Helper: badge color by status ────────────────────────────────────
        private Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "pending": return Color.FromArgb(255, 193, 7);   // yellow
                case "confirmed": return Color.FromArgb(76, 175, 80);   // green
                case "completed": return Color.FromArgb(33, 150, 243);  // blue
                case "cancelled": return Color.FromArgb(244, 67, 54);   // red
                default: return Color.Gray;
            }
        }

        // ── Helper: rounded rectangle ────────────────────────────────────────
        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Keep these so the designer doesn't break ──
        private void guna2HtmlLabel4_Click(object sender, EventArgs e) { }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { }
    }
}