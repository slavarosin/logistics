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
using DataAccessLayer;
using System.Collections.ObjectModel;

namespace BeLogForm.Controls
{
    /// <summary>
    /// Interaction logic for CtrlLogin.xaml
    /// </summary>
    public partial class CtrlLogin : UserControl
    {

        private BeLogDB db = new BeLogDB();
        private User currentUser;
        public delegate void SubmitLoginEvent(object sender, SubmitLoginEventArgs e);
        public event SubmitLoginEvent SubmitLogin;

        private event EventHandler LoggedChanged;
        private bool _logged = false;

        private ObservableCollection<User> _users;

        public CtrlLogin()
        {
            try
            {
                InitializeComponent();

                System.Diagnostics.Debug.Print(db.Connection.ConnectionString);

                this.Userlist = new ObservableCollection<User>(db.Users);

                //this.comboBoxUsers.ItemsSource = this.Userlist;
                this.comboBoxUsers.DisplayMemberPath = "LoginUserName";
                selectPrevUser();

                InitializeKeys();
                this.LoggedChanged += new EventHandler(CtrlLogin_LoggedChanged);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    this.comboBoxUsers.ItemsSource = value;
                }
            }
        }
        
        public bool LoggedIn
        {

            get { return _logged; }

            set
            {
                if (value != _logged)
                {
                    _logged = value;
                    OnLoggedChanged(EventArgs.Empty);
                }
            }

        }

        protected virtual void OnLoggedChanged(EventArgs e)
        {
            if (LoggedChanged != null)
            {
                LoggedChanged(this, e);
            }
        } 
        
        void CtrlLogin_LoggedChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("login/logout");
            if (LoggedIn)
            {
                // Login event fired
                this.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                // Logout event fired
                db.Refresh(System.Data.Linq.RefreshMode.KeepChanges, db.Users);
                this.Userlist = new ObservableCollection<User>(db.Users);
                
                this.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void InitializeKeys()
        {
            // Any comments on that?
            var CommitCommand = new RoutedCommand();
            CommandBinding cb = new CommandBinding(CommitCommand,
            CommitCommandExecute, CommitCommandCanExecute);
            this.CommandBindings.Add(cb);
            KeyGesture kg = new KeyGesture(Key.Enter, ModifierKeys.None);
            InputBinding ib = new InputBinding(CommitCommand, kg);
            this.InputBindings.Add(ib);
        }
        private void CommitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommitCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            btnLogin_Click(null, null);
        }

        /// <summary>
        /// Select previously logged user
        /// </summary>
        private void selectPrevUser()
        {
            if (this.comboBoxUsers.Items.Count > 0)
                {
                    this.comboBoxUsers.SelectedIndex = 0;
                    foreach (User usr in Userlist)
                    {
                        if (usr.LoginUserName.Equals(Properties.Settings.Default.LastLoggedUser))
                        {
                            this.comboBoxUsers.SelectedItem = usr;
                            return;
                        }
                    }    
                }
        }
            
    
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentUser != null)
                this.SubmitLogin(this, new SubmitLoginEventArgs(this.currentUser));

        }
        
        public class SubmitLoginEventArgs : EventArgs
        {
            public User loggedUser { get; set; }

            public SubmitLoginEventArgs(User loggedUser)
            {
                this.loggedUser = loggedUser;
                // Save last logged user to a settings file
                Properties.Settings.Default.LastLoggedUser = loggedUser.LoginUserName;
                Properties.Settings.Default.Save();
            }
        }

        private void comboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                this.currentUser = (User)this.comboBoxUsers.SelectedItem;
        }

        private void UserControlLogin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.comboBoxUsers.Focus();
        }

    }
}
