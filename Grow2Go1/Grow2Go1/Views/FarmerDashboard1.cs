using Grow2Go1.Models;
using Grow2Go1.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard1 : UserControl
    {
        private int _farmId;
        private Panel _overlayPanel;
        private FlowLayoutPanel _productsPanel;
        private bool _productsLoaded = false;
        private readonly string _connectionString =
            "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;";

        public FarmerDashboard1()
        {
            InitializeComponent();
        }

        public FarmerDashboard1(int farmId) : this()
        {
            _farmId = farmId;
        }

        private void FarmerDashboard1_Load(object sender, EventArgs e) { }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!_productsLoaded && this.Width > 300 && _farmId > 0)
            {
                _productsLoaded = true;
                LoadProducts();
            }
        }

        public void LoadProducts()
        {
            if (_productsPanel != null)
            {
                this.Controls.Remove(_productsPanel);
                _productsPanel.Dispose();
                _productsPanel = null;
            }

            var repo = new ProductRepository(_connectionString);
            List<Product> products = repo.GetProductsByFarm(_farmId);

            if (products.Count == 0)
            {
                NoOrdersAvailable.Visible = true;
            }
            else
            {
                NoOrdersAvailable.Visible = false;
                ShowProductCards(products);
            }
        }

        private void ShowProductCards(List<Product> products)
        {
            _productsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 95),
                Size = new Size(this.Width - 40, this.Height - 105),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                AnchorStyles.Right | AnchorStyles.Bottom
            };

            int totalMargin = 8 * 2 * 4;
            int cardWidth = (_productsPanel.Width - totalMargin) / 4;

            foreach (var product in products)
                _productsPanel.Controls.Add(CreateProductCard(product, cardWidth));

            this.Controls.Add(_productsPanel);
            _productsPanel.BringToFront();
        }

        private Panel CreateProductCard(Product product, int cardWidth)
        {
            bool isActive = product.Stock > 0;

            // ── Card container ──
            var card = new Panel
            {
                Size = new Size(cardWidth, 450),
                Margin = new Padding(8),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = MakeRoundedRect(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // ── Product image ──
            Image productImage = null;
            if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
            {
                try { productImage = Image.FromFile(product.ImagePath); }
                catch { }
            }

            var imageBox = new PictureBox
            {
                Size = new Size(cardWidth, 260),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Normal
            };

            imageBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (var clipPath = new GraphicsPath())
                {
                    int r = 10;
                    clipPath.AddArc(0, 0, r * 2, r * 2, 180, 90);
                    clipPath.AddArc(imageBox.Width - r * 2, 0, r * 2, r * 2, 270, 90);
                    clipPath.AddLine(imageBox.Width, r, imageBox.Width, imageBox.Height);
                    clipPath.AddLine(imageBox.Width, imageBox.Height, 0, imageBox.Height);
                    clipPath.AddLine(0, imageBox.Height, 0, r);
                    clipPath.CloseFigure();
                    e.Graphics.SetClip(clipPath);
                }

                if (productImage != null)
                {
                    float imgRatio = (float)productImage.Width / productImage.Height;
                    float boxRatio = (float)imageBox.Width / imageBox.Height;

                    RectangleF srcRect;
                    if (imgRatio > boxRatio)
                    {
                        float srcW = productImage.Height * boxRatio;
                        float srcX = (productImage.Width - srcW) / 2f;
                        srcRect = new RectangleF(srcX, 0, srcW, productImage.Height);
                    }
                    else
                    {
                        float srcH = productImage.Width / boxRatio;
                        float srcY = (productImage.Height - srcH) / 2f;
                        srcRect = new RectangleF(0, srcY, productImage.Width, srcH);
                    }

                    e.Graphics.DrawImage(productImage,
                        new Rectangle(0, 0, imageBox.Width, imageBox.Height),
                        srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(240, 240, 240)),
                        0, 0, imageBox.Width, imageBox.Height);

                    using (var font = new Font("Segoe UI", 11))
                    {
                        string text = "No Image";
                        var sz = e.Graphics.MeasureString(text, font);
                        e.Graphics.DrawString(text, font,
                            new SolidBrush(Color.FromArgb(180, 180, 180)),
                            (imageBox.Width - sz.Width) / 2f,
                            (imageBox.Height - sz.Height) / 2f);
                    }
                }

                e.Graphics.ResetClip();
            };
            card.Controls.Add(imageBox);

            // ── Calculate badge dimensions FIRST so name label knows where to stop ──
            int badgeWidth = isActive ? 102 : 145;
            int badgeX = cardWidth - badgeWidth - 8;

            // ── Product name — stops before badge ──
            var nameLabel = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(12, 270),
                Size = new Size(badgeX - 17, 28),
                AutoEllipsis = true
            };
            card.Controls.Add(nameLabel);

            // ── Status badge ──
            var badge = new Label
            {
                Text = isActive ? "active" : "out-of-stock",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = isActive
                    ? Color.FromArgb(76, 175, 80)
                    : Color.FromArgb(244, 67, 54),
                Location = new Point(badgeX, 270),
                Size = new Size(badgeWidth, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(badge);

            // ── Description ──
            var descLabel = new Label
            {
                Text = string.IsNullOrEmpty(product.Description) ? "" : product.Description,
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(12, 306),
                Size = new Size(cardWidth - 24, 44),
                AutoEllipsis = true
            };
            card.Controls.Add(descLabel);

            // ── Price ──
            var priceLabel = new Label
            {
                Text = "₱" + product.Price.ToString("0.00") + " / " + product.Unit,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(20, 20, 20),
                BackColor = Color.Transparent,
                Location = new Point(12, 358),
                Size = new Size(cardWidth - 115, 28)
            };
            card.Controls.Add(priceLabel);

            // ── Stock ──
            var stockLabel = new Label
            {
                Text = "Stock: " + product.Stock,
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(cardWidth - 110, 360),
                Size = new Size(100, 26),
                TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(stockLabel);

            // ── Edit button ──
            var editBtn = new Button
            {
                Text = "✏ Edit",
                Font = new Font("Segoe UI", 10),
                Location = new Point(12, 405),
                Size = new Size(95, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 50),
                Cursor = Cursors.Hand
            };
            editBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            editBtn.Click += (s, e) => ShowEditProductOverlay(product);
            card.Controls.Add(editBtn);

            return card;
        }

        // ── Add Product overlay ──────────────────────────────────────────────
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            ShowAddProductOverlay();
        }

        private void ShowAddProductOverlay()
        {
            _overlayPanel = new Panel
            {
                Size = this.Size,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(120, 0, 0, 0)
            };

            var addControl = new AddProductControl(_farmId);
            CenterControl(addControl);

            addControl.ProductAdded += (s, ev) => CloseOverlay(true);
            addControl.Cancelled += (s, ev) => CloseOverlay(false);

            _overlayPanel.Controls.Add(addControl);
            this.Controls.Add(_overlayPanel);
            _overlayPanel.BringToFront();
        }

        // ── Edit Product overlay ─────────────────────────────────────────────
        private void ShowEditProductOverlay(Product product)
        {
            _overlayPanel = new Panel
            {
                Size = this.Size,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(120, 0, 0, 0)
            };

            var editControl = new AddProductControl(_farmId, product);
            CenterControl(editControl);

            editControl.ProductUpdated += (s, ev) => CloseOverlay(true);
            editControl.ProductDeleted += (s, ev) => CloseOverlay(true);
            editControl.Cancelled += (s, ev) => CloseOverlay(false);

            _overlayPanel.Controls.Add(editControl);
            this.Controls.Add(_overlayPanel);
            _overlayPanel.BringToFront();
        }

        private void CenterControl(Control control)
        {
            control.Location = new Point(
                (this.Width - control.Width) / 2,
                (this.Height - control.Height) / 2
            );
        }

        private void CloseOverlay(bool refreshProducts)
        {
            if (_overlayPanel != null)
            {
                this.Controls.Remove(_overlayPanel);
                _overlayPanel.Dispose();
                _overlayPanel = null;
            }

            if (refreshProducts)
                LoadProducts();
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

        // ── Keep these so the designer doesn't break ──
        private void guna2HtmlLabel3_Click(object sender, EventArgs e) { }
        private void FarmMapButton_Click(object sender, EventArgs e) { }
        private void NoOrdersAvailable_Click(object sender, EventArgs e) { }
    }
}   