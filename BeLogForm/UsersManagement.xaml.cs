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
using System.Collections.ObjectModel;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for UsersManagement.xaml
    /// </summary>
    public partial class UsersManagement : Window
    {

        private enum WindowState
        { 
            StartState,
            ReadyState,
            UserSaved,
            UserDeleted,
            UserAdded,
            ClosingState,
            ErrorState
        }

        private WindowState m_CurrentWindowState;

        //private static BaseLogic baseLogic = new BaseLogic();
        private static BeLogDB db = new BeLogDB();
        private ObservableCollection<User> _users;

        List<String> AppRights = new List<String>();

        public UsersManagement()
        {
            InitializeComponent();
            stateChangeForWindow(WindowState.StartState);
        }


        private void stateChangeForWindow(WindowState state)
        {
            switch (state)
            { 
                case WindowState.StartState:
                    
                    this.Userlist = new ObservableCollection<User>(db.Users);
                    this.lbxUsers.ItemsSource = Userlist;
                    this.lbxUsers.DisplayMemberPath = "LoginUserName";
                    this.lbxUsers.Focus();

                    //foreach (String str in System.Enum.GetNames(typeof(BeLog.Logic.ApplicationAccess)))
                    //{
                    //    AppRights.Add(str);
                    //}
                    //this.lbxRights.ItemsSource = AppRights;

                    stateChangeForWindow(WindowState.ReadyState);

                    break;
                case WindowState.ReadyState:
                    // clear fields
                    this.gridEdit.DataContext = new User();
                    this.btnCancel.Focus();
                    break;
                case WindowState.UserAdded:
                    System.Diagnostics.Debug.Print("User added!");
                    stateChangeForWindow(WindowState.ReadyState);
                    break;
                case WindowState.UserDeleted:
                    System.Diagnostics.Debug.Print("User deleted!");
                    stateChangeForWindow(WindowState.ReadyState);
                    break;
                case WindowState.UserSaved:
                    System.Diagnostics.Debug.Print("User saved!");
                    stateChangeForWindow(WindowState.ReadyState);
                    break;
                case WindowState.ErrorState:
                    System.Diagnostics.Debug.Print("Error state");
                    if (this.WindowError.Length > 0)
                    {
                        MessageBox.Show(this.WindowError);
                        this.WindowError = String.Empty;
                    }
                    break;
                case WindowState.ClosingState:
                    this.Close();
                    break;
            }
        }

        private User _selUser = new User();
        public User SelectedUser
        {
            get { return _selUser; }
            set { _selUser = value; }
        }

        private String _error = String.Empty;
        public String WindowError
        {
            get { return _error; }
            set 
            { 
              if(value != _error)
                _error = value; 
            }
        }

        public ObservableCollection<User> Userlist
        {
            get { return _users; }

            set
            {
                if (value != _users)
                {
                    _users = value;
                    this.lbxUsers.ItemsSource = value;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            stateChangeForWindow(WindowState.ClosingState);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            saveUser((User)this.gridEdit.DataContext);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            saveUser((User)this.gridEdit.DataContext);
        }

        private void lbxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedUser = (User)lbxUsers.SelectedItem;
            this.gridEdit.DataContext = this.SelectedUser;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User tempUser = (User)lbxUsers.SelectedItem;
                if (tempUser.ID != 0)
                {
                    if (MessageBox.Show("I am going to delete " + tempUser.LoginUserName + "! Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    Userlist.Remove((User)lbxUsers.SelectedItem);
                    //db.Users.DeleteOnSubmit((User)lbxUsers.SelectedItem);
                    db.Users.DeleteOnSubmit(db.Users.Where(c => c.ID == tempUser.ID).First());
                    db.SubmitChanges();
                    stateChangeForWindow(WindowState.UserDeleted);
                }
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                this.WindowError = sqlex.Message;
                stateChangeForWindow(WindowState.ErrorState);
            }
            catch (Exception ex)
            {
                this.WindowError = ex.Message;
                stateChangeForWindow(WindowState.ErrorState);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //this.gridEdit.DataContext = new User();
            this.SelectedUser = new User();
            this.gridEdit.DataContext = this.SelectedUser;
            this.textBox1.Focus();
        }

        private void saveUser(User tempUser)
        {
            if (tempUser.LoginUserName == null)
                return;

            if (!FormInfoGatherer.ValidateControl(this.textBox1))
                return;
            if (!FormInfoGatherer.ValidateControl(this.textBox3))
                return;
            
            tempUser.LoginPassword = "DefaultPass";
            tempUser.ApplicationRights = "Admin;";

            try
            {

                if (tempUser.ID == 0)
                {

                    if (db.Users.Where(c => c.LoginUserName == tempUser.LoginUserName).Count() > 0)
                        if (MessageBox.Show("There is already a user with that username! Save anyway?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return;
                    db.Users.InsertOnSubmit(tempUser);
                    Userlist.Add(tempUser);

                    db.SubmitChanges();
                    stateChangeForWindow(WindowState.UserAdded);
                }
                else
                {
                    User cuser = db.Users.Where(c => c.ID == tempUser.ID).First();
                    cuser.LoginUserName = tempUser.LoginUserName;
                    cuser.Abreviation = tempUser.Abreviation;
                    cuser.ApplicationRights = tempUser.ApplicationRights;
                    cuser.LoginPassword = tempUser.LoginPassword;

                    // Why? To refresh the listbox itemsource somehow
                    int pos = Userlist.IndexOf(tempUser);
                    Userlist.Remove(tempUser);
                    Userlist.Insert(pos, cuser);

                    db.SubmitChanges();
                    stateChangeForWindow(WindowState.UserSaved);
                }

            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                this.WindowError = sqlex.Message;
                stateChangeForWindow(WindowState.ErrorState);
            }
            catch (InvalidOperationException iex)
            {
                this.WindowError = iex.Message;
                stateChangeForWindow(WindowState.ErrorState);
            }
        }

        private void lbxRights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //List<String> templist;

            //if (this.SelectedUser.ApplicationRights != null)
            //    templist = this.SelectedUser.ApplicationRights.Split(';').ToList();
            //else
            //{
            //    templist = new List<String>();
            //    this.SelectedUser.ApplicationRights = String.Empty;
            //}
            //foreach (String right in lbxRights.SelectedItems)
            //{
            //    if (this.SelectedUser.ApplicationRights.Contains(right))
            //    {
            //        templist.Remove(right);
            //        //this.SelectedUser.ApplicationRights.Replace(right+";","");
            //    }
            //    else
            //        templist.Add(right); 
            //}

            //this.SelectedUser.ApplicationRights = String.Empty;
            //foreach (String r in templist)
            //{
            //    if (r.Length > 0)
            //        this.SelectedUser.ApplicationRights += r + ";";
            //}

        }
    }
}
