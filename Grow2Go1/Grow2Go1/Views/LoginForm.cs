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

            // Then show sign in
            signInPanel.Visible = true;

            // Toggle button styles — Sign In active
            btnSignIn.FillColor = Color.FromArgb(90, 140, 40);
            btnSignIn.ForeColor = Color.White;
            btnSignUp.FillColor = Color.Transparent;
            btnSignUp.ForeColor = Color.FromArgb(80, 80, 80);
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            signInPanel.Visible = false;
            signUpPanel.Visible = true;

            // Toggle button styles — Sign Up active
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

            // TODO: DB login logic here
            MessageBox.Show("Login clicked! DB logic coming soon.",
                "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Forgot password feature coming soon!",
                "Forgot Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toggleContainer_Paint(object sender, PaintEventArgs e) { }

        private void label8_Click(object sender, EventArgs e)
        {
            label8.BackColor = Color.Transparent;
            label8.Parent = this;
        }

        private void namelbl_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtFullname_TextChanged(object sender, EventArgs e)
        {

        }

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
    }
}