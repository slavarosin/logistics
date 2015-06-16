using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BeLog.Logic;
using DataAccessLayer;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class UserForm : Window
    {
        BaseLogic baseLogic = new BaseLogic();
        
        private User _selUser = new User();
        public User SelectedUser
        {
            get { return _selUser; }
            set { _selUser = value; }
        }
        
        public UserForm(User user)
            : this()
        {
            this.SelectedUser = user;
            bool l = user.IsAdmin;
            this.userEditGrd.DataContext = this.SelectedUser;
        }
        
        public UserForm()
        {
            InitializeComponent();
            this.userEditGrd.DataContext = this.SelectedUser;
            //lbxUserName.ItemsSource = baseLogic.db.Users;
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            this.SelectedUser = null;
            this.Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedUser.LoginUserName != null)
            {
                this.Close();
            }
        }

    }
}
