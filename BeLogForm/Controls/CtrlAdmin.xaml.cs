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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BeLog.Logic;
using DataAccessLayer;
using System.Collections.ObjectModel;

namespace BeLogForm.Controls
{
    /// <summary>
    /// Interaction logic for ctrlAdmin.xaml
    /// </summary>
    public partial class CtrlAdmin : UserControl
    {
        private static BeLogDB db = new BeLogDB();
        private ObservableCollection<User> _users;
        
        public CtrlAdmin()
        {
            InitializeComponent();

            try
            {
                _users = new ObservableCollection<User>(db.Users);
                this.lbxUsers.ItemsSource = _users;
                this.lbxUsers.DisplayMemberPath = "LoginUserName";
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {

                MessageBox.Show(sqlex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            UserForm uf;
            if (lbxUsers.SelectedIndex > -1)
            {
                uf = new UserForm((User)lbxUsers.SelectedItem);
                
            }
            else
            {
                uf = new UserForm();
            }

            uf.ShowDialog();
            saveUser(uf.SelectedUser);
        
        }
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //if ((string)cbxRights.SelectedItem == "Admin")
            //{
            //    lbxRights.ItemsSource = new List<string>();
            //}
            //if (!((List<string>)lbxRights.ItemsSource).Contains(cbxRights.SelectedItem)
            //       && !((List<string>)lbxRights.ItemsSource).Contains("Admin"))
            //{
            //    var tempList = (List<string>)lbxRights.ItemsSource;
            //    tempList.Add((string)cbxRights.SelectedItem);
            //    lbxRights.ItemsSource = tempList;
            //}

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //if (lbxRights.SelectedItem != null)
            //{
            //    var tempList = (List<string>)lbxRights.ItemsSource;
            //    tempList.Remove((string)lbxRights.SelectedItem);
            //    lbxRights.ItemsSource = tempList;
            //}
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (grdAdmin.DataContext is User)
            {
                var tempUser = (User)grdAdmin.DataContext;
                db.Users.InsertOnSubmit(tempUser);
            }
            db.SubmitChanges();
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            //grdAdmin.DataContext = new User();
            UserForm uf = new UserForm();
            uf.ShowDialog();
            saveUser(uf.SelectedUser);
        }

        private void saveUser(User tempUser)
        {
            if (tempUser == null)
                return;

            // TODO: Think of something if user1 will be renamed to userX, but there is already a user with name userX
            try
            {
                
                if (tempUser.ID == 0)
                {

                    if (db.Users.Where(c => c.LoginUserName == tempUser.LoginUserName).Count() > 0)
                        if (MessageBox.Show("There is already a user with that username! Save anyway?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return;
                    db.Users.InsertOnSubmit(tempUser);
                    _users.Add(tempUser);
                }
                else
                {
                    User cuser = db.Users.Where(c => c.ID == tempUser.ID).First();
                    cuser.LoginUserName = tempUser.LoginUserName;
                    cuser.Abreviation = tempUser.Abreviation;
                    cuser.ApplicationRights = tempUser.ApplicationRights;
                    cuser.LoginPassword = tempUser.LoginPassword;

                    // Why? To refresh the listbox itemsource somehow
                    int pos = _users.IndexOf(tempUser);
                    _users.Remove(tempUser);
                    _users.Insert(pos, cuser);
                }

                db.SubmitChanges();
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                System.Diagnostics.Debug.Print(sqlex.Message);
            }
            catch (InvalidOperationException iex)
            {
                System.Diagnostics.Debug.Print(iex.Message);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User tempUser = (User)lbxUsers.SelectedItem;
                if (tempUser.ID != 0)
                {
                    if (MessageBox.Show("I am going to delete " + tempUser.LoginUserName + "! Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    _users.Remove((User)lbxUsers.SelectedItem);
                    //db.Users.DeleteOnSubmit((User)lbxUsers.SelectedItem);
                    db.Users.DeleteOnSubmit(db.Users.Where(c => c.ID == tempUser.ID).First());
                }
                db.SubmitChanges();
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                System.Diagnostics.Debug.Print(sqlex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        private void lbxUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserForm uf;
            if (lbxUsers.SelectedIndex > -1)
            {
                uf = new UserForm((User)lbxUsers.SelectedItem);
                uf.ShowDialog();
                saveUser(uf.SelectedUser);
        
            }
           
        }

    }
}
