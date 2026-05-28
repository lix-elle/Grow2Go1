using Grow2Go.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard3 : UserControl
    {
        private int _farmId;
        private int _userId;
        private bool _profileLoaded = false;

        public FarmerDashboard3()
        {
            InitializeComponent();
        }

        public FarmerDashboard3(int farmId, int userId) : this()
        {
            _farmId = farmId;
            _userId = userId;
        }

        private void FarmerDashboard3_Load(object sender, EventArgs e) { }

        // ── Load when UC is properly sized ───────────────────────────────────
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!_profileLoaded && this.Width > 300 && _farmId > 0)
            {
                _profileLoaded = true;
                LoadFarmProfile();
                SetEditMode(false); // start in read-only mode
            }
        }

        // ── Load farm data from DB ───────────────────────────────────────────
        private void LoadFarmProfile()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT f.farm_name, f.location, f.description,
                               f.phone_number, f.delivery_zones, f.delivery_days,
                               u.full_name AS owner_name
                        FROM farms f
                        JOIN users u ON f.user_id = u.user_id
                        WHERE f.farm_id = @farmId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmId", _farmId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtFarmName.Text = reader.IsDBNull(reader.GetOrdinal("farm_name")) ? "" : reader.GetString("farm_name");
                                txtOwnerName.Text = reader.IsDBNull(reader.GetOrdinal("owner_name")) ? "" : reader.GetString("owner_name");
                                txtFarmDescription.Text = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description");
                                txtLocation.Text = reader.IsDBNull(reader.GetOrdinal("location")) ? "" : reader.GetString("location");
                                txtPhoneNumber.Text = reader.IsDBNull(reader.GetOrdinal("phone_number")) ? "" : reader.GetString("phone_number");
                                txtDeliveryZones.Text = reader.IsDBNull(reader.GetOrdinal("delivery_zones")) ? "" : reader.GetString("delivery_zones");
                                txtDeliveryDays.Text = reader.IsDBNull(reader.GetOrdinal("delivery_days")) ? "" : reader.GetString("delivery_days");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load farm profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Save farm data to DB ─────────────────────────────────────────────
        private void SaveFarmProfile()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        UPDATE farms SET
                            farm_name       = @farmName,
                            location        = @location,
                            description     = @description,
                            phone_number    = @phone,
                            delivery_zones  = @zones,
                            delivery_days   = @days
                        WHERE farm_id = @farmId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@farmName", txtFarmName.Text.Trim());
                        cmd.Parameters.AddWithValue("@location", txtLocation.Text.Trim());
                        cmd.Parameters.AddWithValue("@description", txtFarmDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", txtPhoneNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@zones", txtDeliveryZones.Text.Trim());
                        cmd.Parameters.AddWithValue("@days", txtDeliveryDays.Text.Trim());
                        cmd.Parameters.AddWithValue("@farmId", _farmId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Farm profile saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save farm profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Toggle edit mode on/off ──────────────────────────────────────────
        private void SetEditMode(bool isEditing)
        {
            // Use ONLY ReadOnly (not Enabled) — works better with Guna2 textboxes
            // that already have pre-filled text
            txtFarmName.ReadOnly = !isEditing;
            txtFarmDescription.ReadOnly = !isEditing;
            txtLocation.ReadOnly = !isEditing;
            txtPhoneNumber.ReadOnly = !isEditing;
            txtDeliveryZones.ReadOnly = !isEditing;
            txtDeliveryDays.ReadOnly = !isEditing;

            // Owner name always stays read-only
            txtOwnerName.ReadOnly = true;

            // Force Farm Name to refresh so it accepts typing
            if (isEditing)
            {
                string current = txtFarmName.Text;
                txtFarmName.Clear();
                txtFarmName.Text = current;
                txtFarmName.SelectionStart = txtFarmName.Text.Length;
            }

            // Toggle buttons
            btnEdit.Visible = !isEditing;
            SaveButton.Visible = isEditing;
        }

        // ── Edit button ──────────────────────────────────────────────────────
        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetEditMode(true);
        }

        // ── Save button ──────────────────────────────────────────────────────
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFarmName.Text))
            {
                MessageBox.Show("Farm name cannot be empty.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFarmProfile();
            SetEditMode(false);
        }

        // ── Keep these so the designer doesn't break ──
        private void guna2CustomGradientPanel4_Paint(object sender, PaintEventArgs e) { }
        private void MarketplaceButton_Click(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }
        private void OwnerName_Click(object sender, EventArgs e) { }
        private void FarmDescription_Click(object sender, EventArgs e) { }
        private void Location_Click(object sender, EventArgs e) { }
        private void DeliveryZones_Click(object sender, EventArgs e) { }
        private void DeliveryDays_Click(object sender, EventArgs e) { }
        private void PhoneNum_Click(object sender, EventArgs e) { }
        private void guna2TextBox8_TextChanged(object sender, EventArgs e) { }

        private void btnEdit_Click_1(object sender, EventArgs e)
        {
            SetEditMode(true);
        }

        private void SaveButton_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFarmName.Text))
            {
                MessageBox.Show("Farm name cannot be empty.", "Missing Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFarmProfile();
            SetEditMode(false);
        }
    }
}