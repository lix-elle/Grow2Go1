using Grow2Go1.Models;
using Grow2Go1.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class AddProductControl : UserControl
    {
        private int _farmId;
        private string _selectedImagePath = "";
        private bool _isEditMode = false;
        private Product _productToEdit = null;

        public event EventHandler ProductAdded;
        public event EventHandler ProductUpdated;
        public event EventHandler ProductDeleted;
        public event EventHandler Cancelled;

        // ── Add mode constructor ─────────────────────────────────────────────
        public AddProductControl(int farmId)
        {
            InitializeComponent();
            _farmId = farmId;
            _isEditMode = false;
            this.AutoScroll = true;
            SetupDropdowns();

            // Hide delete button in Add mode
            btnDelete.Visible = false;
        }

        // ── Edit mode constructor ────────────────────────────────────────────
        public AddProductControl(int farmId, Product product) : this(farmId)
        {
            _isEditMode = true;
            _productToEdit = product;
            PreFillForm(product);

            // Show delete button in Edit mode
            btnDelete.Visible = true;
        }

        // ── Required so designer doesn't break ──────────────────────────────
        public AddProductControl()
        {
            InitializeComponent();
            this.AutoScroll = true;
            SetupDropdowns();
        }

        private void SetupDropdowns()
        {
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUnit.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbCategory.Items.AddRange(new string[]
            {
                "Fruits", "Vegetables", "Herbs",
                "Dairy & Eggs", "Meat", "Grains"
            });
            cmbCategory.SelectedIndex = -1;

            cmbUnit.Items.AddRange(new string[]
            {
                "per lb", "each", "per dozen",
                "per pint", "per bunch", "per bag",
                "per kg", "per piece"
            });
            cmbUnit.SelectedIndex = -1;
        }

        // ── Pre-fill all fields when in Edit mode ────────────────────────────
        private void PreFillForm(Product product)
        {
            txtProductName.Text = product.Name;
            txtPrice.Text = product.Price.ToString("0.00");
            txtStock.Text = product.Stock.ToString();
            txtDescription.Text = product.Description;

            int catIndex = cmbCategory.Items.IndexOf(product.Category);
            cmbCategory.SelectedIndex = catIndex >= 0 ? catIndex : -1;

            int unitIndex = cmbUnit.Items.IndexOf(product.Unit);
            cmbUnit.SelectedIndex = unitIndex >= 0 ? unitIndex : -1;

            if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
            {
                picProductImage.Image = Image.FromFile(product.ImagePath);
                _selectedImagePath = product.ImagePath;
            }

            // Change button text to show we're editing
            btnAddProduct.Text = "Update Product";
        }

        // ── Browse image ─────────────────────────────────────────────────────
        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Product Image";
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = dialog.FileName;
                    picProductImage.Image = Image.FromFile(_selectedImagePath);
                }
            }
        }

        // ── Add / Update button ──────────────────────────────────────────────
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            // ── Validate ──
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter a product name.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbUnit.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a unit.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price.", "Invalid Price",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity.", "Invalid Stock",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string savedImagePath = SaveImageToAppFolder(_selectedImagePath);
            var repo = new ProductRepository(
                "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;");

            if (_isEditMode)
            {
                // ── UPDATE ──
                bool success = repo.UpdateProduct(
                    _productToEdit.ProductId,
                    txtProductName.Text.Trim(),
                    cmbCategory.SelectedItem.ToString(),
                    cmbUnit.SelectedItem.ToString(),
                    txtDescription.Text.Trim(),
                    price,
                    stock,
                    string.IsNullOrEmpty(savedImagePath)
                        ? _productToEdit.ImagePath
                        : savedImagePath
                );

                if (success)
                {
                    MessageBox.Show("Product updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Failed to update product.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // ── ADD ──
                bool success = repo.AddProduct(
                    _farmId,
                    txtProductName.Text.Trim(),
                    cmbCategory.SelectedItem.ToString(),
                    cmbUnit.SelectedItem.ToString(),
                    txtDescription.Text.Trim(),
                    price,
                    stock,
                    savedImagePath
                );

                if (success)
                {
                    MessageBox.Show("Product added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProductAdded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Failed to add product.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Delete button ────────────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to delete \"" + _productToEdit.Name + "\"?\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                var repo = new ProductRepository(
                    "Server=localhost;Database=grow2go;Uid=root;Pwd=12345;");
                bool success = repo.DeleteProduct(_productToEdit.ProductId);

                if (success)
                {
                    MessageBox.Show("Product deleted successfully!", "Deleted",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProductDeleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Failed to delete product.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Cancel button ────────────────────────────────────────────────────
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        // ── Save image to ProductImages folder ───────────────────────────────
        private string SaveImageToAppFolder(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath)) return "";
            try
            {
                string imagesFolder = Path.Combine(Application.StartupPath, "ProductImages");
                Directory.CreateDirectory(imagesFolder);
                string fileName = Guid.NewGuid() + Path.GetExtension(sourcePath);
                string destPath = Path.Combine(imagesFolder, fileName);
                File.Copy(sourcePath, destPath, overwrite: true);
                return destPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Image save error: " + ex.Message);
                return "";
            }
        }

        // ── Empty handlers required by Designer ──
        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtDescription_TextChanged(object sender, EventArgs e) { }
        private void guna2HtmlLabel1_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel5_Click(object sender, EventArgs e) { }
        private void AddproductControl_Load(object sender, EventArgs e) { }
        private void picProductImage_Click(object sender, EventArgs e) { }


        private void txtProductName_TextChanged(object sender, EventArgs e) { }
        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtPrice_TextChanged(object sender, EventArgs e) { }
        private void txtStock_TextChanged(object sender, EventArgs e) { }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel3_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel4_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel6_Click(object sender, EventArgs e) { }
        private void guna2HtmlLabel7_Click(object sender, EventArgs e) { }
    }
}