using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;


namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
      
       
        private void App_DispatherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error occured " + e.Exception.ToString());
            if (e.Exception.InnerException != null)
            {
                MessageBox.Show(e.Exception.InnerException.ToString());
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.InnerException.ToString());
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.InnerException.Message);
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.InnerException.StackTrace);
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.Message);
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.StackTrace);
                System.Diagnostics.EventLog.WriteEntry("BeLogForm", e.Exception.ToString());
            }
            e.Handled = true;
        }
     
    }
}
