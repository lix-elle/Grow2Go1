using Grow2Go.Repositories;
using Grow2Go.Models;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class LoginForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        private void DisableTransition()
        {
            int noTransitions = 1;
            DwmSetWindowAttribute(this.Handle,
                DWMWA_TRANSITIONS_FORCEDISABLED,
                ref noTransitions,
                sizeof(int));
        }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            DisableTransition();
            signInPanel.Visible = true;
            signUpPanel.Visible = false;
            signUpCustomer.Visible = false;
            signUpFarmer.Visible = false;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            signUpPanel.Visible = false;
            signUpCustomer.Visible = false;
            signUpFarmer.Visible = false;
            signInPanel.Visible = true;

            btnSignIn.FillColor = Color.FromArgb(90, 140, 40);
            btnSignIn.ForeColor = Color.White;
            btnSignUp.FillColor = Color.Transparent;
            btnSignUp.ForeColor = Color.FromArgb(80, 80, 80);
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            signInPanel.Visible = false;
            signUpPanel.Visible = true;

            guna2Button1.FillColor = Color.FromArgb(90, 140, 40);
            guna2Button1.ForeColor = Color.White;
            guna2Button2.FillColor = Color.Transparent;
            guna2Button2.ForeColor = Color.FromArgb(80, 80, 80);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your email and password.",
                    "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userRepo = new UserRepository();

            var user = userRepo.GetUserByCredentials(email, password, "farmer");
            if (user == null)
                user = userRepo.GetUserByCredentials(email, password, "customer");

            if (user == null)
            {
                MessageBox.Show("Invalid email or password.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (user.Role == "farmer")
            {
                var dashboard = new FarmerDashboard(user);
                dashboard.Show();
            }
            else if (user.Role == "customer")
            {
                var dashboard = new CustomerDashboard(user);
                dashboard.Show();
            }

            this.Hide();
        }

        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            string email = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter your registered email address:",
                "Forgot Password", "");

            if (string.IsNullOrEmpty(email))
                return;

            var userRepo = new UserRepository();

            if (!userRepo.EmailExists(email))
            {
                MessageBox.Show("No account found with that email.",
                    "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Account found! Password reset feature coming soon.",
                "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toggleContainer_Paint(object sender, PaintEventArgs e) { }

        private void label8_Click(object sender, EventArgs e)
        {
            label8.BackColor = Color.Transparent;
            label8.Parent = this;
        }

        private void namelbl_Click(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
        private void guna2TextBox3_TextChanged(object sender, EventArgs e) { }
        private void label15_Click(object sender, EventArgs e) { }
        private void label16_Click(object sender, EventArgs e) { }
        private void guna2TextBox5_TextChanged(object sender, EventArgs e) { }
        private void txtFullname_TextChanged(object sender, EventArgs e) { }

        private void customerBtn_Click(object sender, EventArgs e)
        {
            signUpPanel.Visible = false;
            signUpCustomer.Visible = true;
        }

        private void farmerBtn_Click(object sender, EventArgs e)
        {
            signUpPanel.Visible = false;
            signUpFarmer.Visible = true;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            signUpCustomer.Visible = false;
            signUpPanel.Visible = true;
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            signUpFarmer.Visible = false;
            signUpPanel.Visible = true;
        }

        private void createAccBtn1_Click(object sender, EventArgs e)
        {
            string fullName = txtFullname1.Text.Trim();
            string phone = txtPhoneNum1.Text.Trim();
            string email = txtEmail1.Text.Trim();
            string password = txtPass1.Text.Trim();
            string confirmPassword = txtConfirmPass1.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.",
                    "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var userRepo = new UserRepository();

            if (userRepo.EmailExists(email))
            {
                MessageBox.Show("Email already exists. Please use a different email.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool created = userRepo.CreateUser(fullName, email, phone, password, "customer");

            if (created)
            {
                // Clear all fields
                txtFullname1.Text = "";
                txtPhoneNum1.Text = "";
                txtEmail1.Text = "";
                txtPass1.Text = "";
                txtConfirmPass1.Text = "";

                // Success popup
                MessageBox.Show("Account created successfully!\nPlease sign in with your new credentials.",
                    "Welcome to Grow2Go!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Go back to Sign In
                signUpCustomer.Visible = false;
                signUpPanel.Visible = false;
                signUpFarmer.Visible = false;
                signInPanel.Visible = true;

                // Set Sign In toggle as active
                btnSignIn.FillColor = Color.FromArgb(90, 140, 40);
                btnSignIn.ForeColor = Color.White;
                btnSignUp.FillColor = Color.Transparent;
                btnSignUp.ForeColor = Color.FromArgb(80, 80, 80);
            }
            else
            {
                MessageBox.Show("Something went wrong. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void createAccBtn2_Click(object sender, EventArgs e)
        {
            string fullName = txtFullname2.Text.Trim();
            string phone = txtPhoneNum2.Text.Trim();
            string email = txtEmail2.Text.Trim();
            string password = txtPass2.Text.Trim();
            string confirmPassword = txtConfirmPass2.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.",
                    "Missing Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var userRepo = new UserRepository();

            if (userRepo.EmailExists(email))
            {
                MessageBox.Show("Email already exists. Please use a different email.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool created = userRepo.CreateUser(fullName, email, phone, password, "farmer");

            if (created)
            {
                // Create farm entry for the farmer
                int userId = userRepo.GetLastInsertedUserId(email);
                userRepo.CreateFarm(userId, fullName);

                // Clear all fields
                txtFullname2.Text = "";
                txtPhoneNum2.Text = "";
                txtEmail2.Text = "";
                txtPass2.Text = "";
                txtConfirmPass2.Text = "";

                // Success popup
                MessageBox.Show("Account created successfully!\nPlease sign in with your new credentials.",
                    "Welcome to Grow2Go!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Go back to Sign In
                signUpFarmer.Visible = false;
                signUpCustomer.Visible = false;
                signUpPanel.Visible = false;
                signInPanel.Visible = true;

                // Set Sign In toggle as active
                btnSignIn.FillColor = Color.FromArgb(90, 140, 40);
                btnSignIn.ForeColor = Color.White;
                btnSignUp.FillColor = Color.Transparent;
                btnSignUp.ForeColor = Color.FromArgb(80, 80, 80);
            }
            else
            {
                MessageBox.Show("Something went wrong. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}