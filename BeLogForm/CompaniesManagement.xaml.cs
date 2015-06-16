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
using System.ComponentModel;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for CompaniesManagement.xaml
    /// </summary>
    public partial class CompaniesManagement : Window
    {

        private enum WindowState
        {
            StartState,
            ReadyState,
            CompanySaved,
            CompanyDeleted,
            CompanyAdded,
            ClosingState,
            ErrorState
        }

        private WindowState m_CurrentWindowState;
        
        private static BeLogDB db = new BeLogDB();
        private ObservableCollection<Company> _companies;
        private Company _selCompany = new Company();
        
        List<CMRForm.CountryInfo> CompanyTypes = new List<CMRForm.CountryInfo>();

        public CompaniesManagement(Company argCompany,BeLog.Logic.ComboType argCotype, ref BaseLogic bl)
            : this(ref bl)
        {
            this.SelectedCompany = argCompany;
            this.cmbTypes.SelectedIndex = (int)argCotype;
            this.txtFilter.Text = this.SelectedCompany.Name;
        }

        public CompaniesManagement(BeLog.Logic.ComboType argCotype, ref BaseLogic bl)
            : this(ref bl)
        {
            this.cmbTypes.SelectedIndex = (int)argCotype;
        }

        public CompaniesManagement(ref BaseLogic bl)
        {
            InitializeComponent();
            stateChangeForWindow(WindowState.StartState);
        }

        private void stateChangeForWindow(WindowState state)
        {
            switch (state)
            {
                case WindowState.StartState:

                    this.Companylist = new ObservableCollection<Company>(db.Companies);
                    this.lbxCompanies.ItemsSource = Companylist;
                    this.lbxCompanies.DisplayMemberPath = "Name";
                    this.lbxCompanies.Focus();

                    this.cmbCountries.ItemsSource = db.Countries;
                    this.cmbCountries.DisplayMemberPath = "CountryName";

                    int cinfocnt = 0;
                    foreach (String cinfo in System.Enum.GetNames(typeof(BeLog.Logic.ComboType)))
                    {
                        CompanyTypes.Add(new CMRForm.CountryInfo { Value = cinfocnt.ToString(), Name = cinfo });
                        cinfocnt++;
                    }

                    this.cmbTypes.ItemsSource = CompanyTypes;
                    this.cmbTypes.DisplayMemberPath = "Name";
                    this.cmbTypes.SelectedValuePath = "Value";

                    stateChangeForWindow(WindowState.ReadyState);   
                    break;
                case WindowState.ReadyState:
                    
                    filter();
                    this.SelectedCompany = new Company();
                    this.tbName.Focus();
                    
                    break;
                case WindowState.CompanyAdded:
                    System.Diagnostics.Debug.Print("User added!");
                    this.Companylist = new ObservableCollection<Company>(db.Companies);
                    stateChangeForWindow(WindowState.ReadyState);
                    break;
                case WindowState.CompanyDeleted:
                    System.Diagnostics.Debug.Print("User deleted!");
                    stateChangeForWindow(WindowState.ReadyState);
                    break;
                case WindowState.CompanySaved:
                    System.Diagnostics.Debug.Print("User saved!");
                    this.Companylist = new ObservableCollection<Company>(db.Companies);
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

        private String _error = String.Empty;
        public String WindowError
        {
            get { return _error; }
            set
            {
                if (value != _error)
                    _error = value;
            }
        }

        public Company SelectedCompany
        {
            get { return _selCompany; }
            set 
            {
                if (value != _selCompany)
                {
                    _selCompany = value;
                    this.gridEdit.DataContext = value;
                }
            }
        }

        public ObservableCollection<Company> Companylist
        {
            get { return _companies; }

            set
            {
                if (value != _companies)
                {
                    _companies = value;
                    this.lbxCompanies.ItemsSource = value;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            stateChangeForWindow(WindowState.ClosingState);
        }

        private void lbxCompanies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedCompany = (Company)lbxCompanies.SelectedItem;
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        private void filter()
        {
            if (this.radioBtnStartsWith.IsChecked == true)
            {
                this.Companylist = new ObservableCollection<Company>(db.Companies.Where(c => c.Name.StartsWith(this.txtFilter.Text)));
            }
            else if (this.radioBtnContains.IsChecked == true)
            {
                this.Companylist = new ObservableCollection<Company>(db.Companies.Where(c => c.Name.Contains(this.txtFilter.Text)));
            }
            this.lbxCompanies.ItemsSource = Companylist;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lbxCompanies.SelectedItems.Count != 0)
                {

                    if (MessageBox.Show("I am going to delete " + lbxCompanies.SelectedItems.Count.ToString() + " Companies! Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    foreach (Company tempItem in lbxCompanies.SelectedItems)
                    {
                        if (tempItem.ID != 0)
                        {
                            //Companylist.Remove((Company)lbxCompanies.SelectedItem);
                            //db.Users.DeleteOnSubmit((User)lbxUsers.SelectedItem);
                            db.Companies.DeleteOnSubmit(db.Companies.Where(c => c.ID == tempItem.ID).First());

                            /*
                             * LINQ to SQL does not support or recognize cascade-delete operations. 
                             * If you want to delete a row in a table that has constraints against it, you must either set the 
                             * ON DELETE CASCADE rule in the foreign-key constraint in the database, or use your own code to first 
                             * delete the child objects that prevent the parent object from being deleted. Otherwise, an exception is thrown. 
                             * For more information, see How to: Delete Rows From the Database (LINQ to SQL). 
                             * http://msdn.microsoft.com/en-us/library/bb386925.aspx
                             */
                        }
                    }
                }

                else if (SelectedCompany.ID != 0)
                {
                    if (MessageBox.Show("I am going to delete " + SelectedCompany.Name + "! Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    db.Companies.DeleteOnSubmit(db.Companies.Where(c => c.ID == SelectedCompany.ID).First());
                }

                db.SubmitChanges();
                Companylist = new ObservableCollection<Company>(db.Companies);

                stateChangeForWindow(WindowState.CompanyDeleted);
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
            //this.gridEdit.DataContext = new Company();
            stateChangeForWindow(WindowState.ReadyState);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            saveCompany((Company)this.gridEdit.DataContext);
        }

        private void saveCompany(Company tempCompany)
        {

            if (!FormInfoGatherer.ValidateControl(this.tbName))
                return;
            if (!FormInfoGatherer.ValidateControl(this.tbAddress))
                return;
            if (!FormInfoGatherer.ValidateControl(this.cmbCountries))
                return;
            if (!FormInfoGatherer.ValidateControl(this.cmbTypes))
                return;

            try
            {

                if (tempCompany.ID == 0)
                {

                    if (db.Companies.Where(c => c.Name == tempCompany.Name).Count() > 0)
                        if (MessageBox.Show("There is already a company with that name! Save anyway?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return;
                    db.Companies.InsertOnSubmit(tempCompany);
                    //Companylist.Add(tempCompany);

                    db.SubmitChanges();
                    stateChangeForWindow(WindowState.CompanyAdded);
                }
                else
                {
                    Company cco = db.Companies.Where(c => c.ID == tempCompany.ID).First();
                    // FIXME: Is there any other way to do this? (Also in UserManagement)
                    cco.Name = tempCompany.Name;
                    cco.Country = tempCompany.Country;
                    cco.CompanyType = tempCompany.CompanyType;
                    cco.ContactPerson = tempCompany.ContactPerson;
                    cco.Fax = tempCompany.Fax;
                    cco.Phone = tempCompany.Phone;

                    // Why? To refresh the listbox itemsource somehow
                    //int pos = Companylist.IndexOf(tempCompany);
                    //Companylist.Remove(tempCompany);
                    //Companylist.Insert(pos, cco);
                    db.SubmitChanges();
                    stateChangeForWindow(WindowState.CompanySaved);
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

        private void radioBtnStartsWith_Checked(object sender, RoutedEventArgs e)
        {
            filter();
        }

        private void radioBtnContains_Checked(object sender, RoutedEventArgs e)
        {
            filter();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            
        }
    }
}
