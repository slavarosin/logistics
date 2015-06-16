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
    /// Interaction logic for TransportManifestSearch.xaml
    /// </summary>
    public partial class TransportManifestSearch : Window
    {
        public TransportManifestSearch()
        {
            InitializeComponent();
            lbxUserName.DataContext = baseLogic.db.Manifests;
            lbxUserName.ItemsSource = baseLogic.db.Manifests;
        }

        private int _selManifestID = 0;
        public int SelectedManifestID
        {
            get { return _selManifestID; }
            set { _selManifestID = value; }
        }
        BaseLogic baseLogic = new BaseLogic();

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectedManifestID = (((Manifest)lbxUserName.SelectedValue).id);
            this.Close();
        }

        private void lbxInvoiceID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedManifestID = (((Manifest)lbxUserName.SelectedValue).id);
            this.Close();
        }
    }
}
