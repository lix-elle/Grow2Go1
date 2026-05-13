using Grow2Go.Models;
using System;
using System.Windows.Forms;

namespace Grow2Go1.Views
{
    public partial class FarmerDashboard : Form
    {
        private User _currentUser;

        public FarmerDashboard(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void FarmerDashboard_Load(object sender, EventArgs e)
        {
            this.Text = "Farmer Dashboard - " + _currentUser.FullName;
        }
    }
}