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
    /// Interaction logic for CMRSearch.xaml
    /// </summary>
    public partial class CMRSearch : Window
    {
        private string _result = null;
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }
        BaseLogic baseLogic = new BaseLogic();

        public CMRSearch()
        {
            InitializeComponent();
            lbxCMRID.DataContext = baseLogic.db.CMRs;
            lbxCMRID.ItemsSource = baseLogic.db.CMRs;
           
        }

      

        private void lbxCMRID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Result = ((CMR)lbxCMRID.SelectedValue).ID.ToString();
                this.Close();
            }
            catch (NullReferenceException n)
            {
                MessageBox.Show(n.Message); 
            }
        }

        private void lbxCMRID_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = ((CMR)lbxCMRID.SelectedValue).ID.ToString();
                this.Close();
            }
            catch (NullReferenceException n)
            {
                MessageBox.Show(n.Message);
        }
            }
    }
}
