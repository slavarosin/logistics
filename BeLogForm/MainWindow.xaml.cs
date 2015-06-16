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
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows.Markup;
using BeLog.Logic;
using BeLogPrinting;
using System.IO;
using BeLogForm.Controls;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CtrlApplications ctrlAppl;
        
        public MainWindow()
        {
            InitializeComponent();
            ctrlLogin.SubmitLogin += new Controls.CtrlLogin.SubmitLoginEvent(ctrlLogin_SubmitLogin);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;

            //System.Diagnostics.Debug.Print(version.Build.ToString());
            //System.Diagnostics.Debug.Print(version.Major.ToString());
            //System.Diagnostics.Debug.Print(version.Minor.ToString());
            //System.Diagnostics.Debug.Print(version.Revision.ToString());
        }

        void ctrlLogin_SubmitLogin(object sender, Controls.CtrlLogin.SubmitLoginEventArgs e)
        {
            if (e.loggedUser!=null)
            {
                //if (e.loggedUser.IsAdmin)
                //{
                //    this.MaxHeight = this.Height = 400;
                //    grdMain.MaxHeight = grdMain.Height = 375;
                
                //}
                //else
                //{
                //    this.MaxHeight = this.Height = 100;
                //    grdMain.MaxHeight = grdMain.Height = 75;
                //}
                
                //this.ctrlLogin.Visibility = Visibility.Hidden;
                this.ctrlLogin.LoggedIn = true;
                ctrlAppl = new CtrlApplications(e.loggedUser);
                grdMain.Children.Add(ctrlAppl);
            
            }
        }
        public void LogOutUser()
        {

            grdMain.Children.Remove(ctrlAppl);
            
            this.MaxHeight = this.MinHeight = this.Height = 300;
            this.MaxWidth = this.MinWidth = this.Width = 340;
            
            //this.ctrlLogin.Margin = new Thickness(0, 0, 0, 0);
            //this.ctrlLogin.Visibility = Visibility.Visible;
            this.ctrlLogin.LoggedIn = false;


        }

    }
}
