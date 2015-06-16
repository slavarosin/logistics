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

namespace BeLogForm.Controls
{
    /// <summary>
    /// Interaction logic for CtrlApplications.xaml
    /// </summary>
    public partial class CtrlApplications : UserControl
    {
        User loggedUser = null;

        public User LoggedUser
        {
            get { return loggedUser; }
            set { loggedUser = value; }
        }
  
     
        public CtrlApplications(User loggedUser)
        {
            // Apply user
            this.loggedUser = loggedUser;
            
            // Create a button CMR
            Button btnCMRForm = new Button();
            btnCMRForm.Height = 25.0d;
            btnCMRForm.Width = 70.0d;
            btnCMRForm.Margin = new Thickness(-160, 20, 0, 0);
            btnCMRForm.Content = "CMR Form";
            btnCMRForm.Click += new RoutedEventHandler(btnCMRForm_Click);
            InitializeComponent();
            if (loggedUser.RightsList.Contains("CMRForm") || loggedUser.RightsList.Contains("Admin"))
            {
                MenuItem menu = new MenuItem();
                menu.Header = "CMR Form";
                menu.Click += new RoutedEventHandler(btnCMRForm_Click);
                menu1.Items.Add(menu);
                this.grdApplList.Children.Insert(0, btnCMRForm);
            }

            // Create a button Invoice (Order)
            Button btnInvoiceForm = new Button();
            btnInvoiceForm.Height = 25.0d;
            btnInvoiceForm.Width = 80.0d;
            btnInvoiceForm.Margin = new Thickness(-10, 20, 0, 0);
            btnInvoiceForm.Content = "Order Form";
            btnInvoiceForm.Click += new RoutedEventHandler(btnInvoiceForm_Click);
            InitializeComponent();
            if (loggedUser.RightsList.Contains("OrderForm") || loggedUser.RightsList.Contains("Admin"))
            {
                MenuItem menu = new MenuItem();
                menu.Header = "Order Form";
                menu.Click += new RoutedEventHandler(btnInvoiceForm_Click);
                menu1.Items.Add(menu);
                this.grdApplList.Children.Insert(1, btnInvoiceForm);
            }

            // Create a button Transport Manifest (Order)
            Button btnTransManifestForm = new Button();
            btnTransManifestForm.Height = 25.0d;
            btnTransManifestForm.Width = 100.0d;
            btnTransManifestForm.Margin = new Thickness(170, 20, 0, 0);
            btnTransManifestForm.Content = "Transport Manifest";
            btnTransManifestForm.Click += new RoutedEventHandler(btnTransManifestForm_Click);
            InitializeComponent();
            if (loggedUser.RightsList.Contains("TransportManifestForm") || loggedUser.RightsList.Contains("Admin"))
            {
                MenuItem menu = new MenuItem();
                menu.Header = "Transport Manifest";
                menu.Click += new RoutedEventHandler(btnTransManifestForm_Click);
                menu1.Items.Add(menu);
                this.grdApplList.Children.Insert(1, btnTransManifestForm);
            }
            if (!loggedUser.RightsList.Contains("Admin"))
                this.MenuItemEdit.IsEnabled = false;
            else
                this.MenuItemEdit.IsEnabled = true;
            // Roman: disabled, Users management from menu
            //ctrlAdmin.Visibility = this.loggedUser.IsAdmin ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden; 
        }

        void btnTransManifestForm_Click(object sender, RoutedEventArgs e)
        {
            TransportManifest form = new TransportManifest(loggedUser);
            form.ShowDialog();
        }
        void btnInvoiceForm_Click(object sender, RoutedEventArgs e)
        {
            OrderForm form = new OrderForm(loggedUser);
            form.ShowDialog();
        }

        void btnCMRForm_Click(object sender, RoutedEventArgs e)
        {
            CMRForm form = new CMRForm(loggedUser);
            form.ShowDialog();
        }
        
        
        public CtrlApplications()
        {
            InitializeComponent();

        }
        private void fileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)this.Parent).Parent).Close();
        }
        private void fileLogOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)this.Parent).Parent).LogOutUser();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // User managment Dialog
            UsersManagement uw = new UsersManagement();
            uw.ShowDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //CompaniesManagement cw = new CompaniesManagement();
            //cw.ShowDialog();
        }
    }
}
