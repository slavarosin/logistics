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
    /// Interaction logic for InvoiceSearch.xaml
    /// </summary>
    public partial class OrderSearch : Window
    {
        public OrderSearch()
        {
            InitializeComponent();
            lbxUserName.DataContext = baseLogic.db.Invoices;
            lbxUserName.ItemsSource = baseLogic.db.Invoices;
        }

        private int _selInvoiceID = 0;
        public int SelectedInvoiceID
        {
            get { return _selInvoiceID; }
            set { _selInvoiceID = value; }
        }
        BaseLogic baseLogic = new BaseLogic();

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectedInvoiceID = (((Invoice)lbxUserName.SelectedValue).ID);
            this.Close();
        }

        private void lbxInvoiceID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedInvoiceID = (((Invoice)lbxUserName.SelectedValue).ID);
            this.Close();
        }
    }
}
