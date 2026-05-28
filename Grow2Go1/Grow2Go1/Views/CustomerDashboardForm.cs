using Grow2Go.Models;
using System;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class CustomerDashboardForm : Form
    {
        private User _currentUser;

        public CustomerDashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void CustomerDashboardForm_Load(object sender, EventArgs e)
        {
            this.Text = "Customer Dashboard - " + _currentUser.FullName;
        }
    }
}
